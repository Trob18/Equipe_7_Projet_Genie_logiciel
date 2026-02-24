using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows; 

namespace EasySave.WPF.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
        private static readonly SemaphoreSlim _largeFileSemaphore = new SemaphoreSlim(1, 1);
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
        private bool _isStopped = false;

        public string Name { get; set; }

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set { _sourceDirectory = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShortSourceDirectory)); }
        }

        private string _targetDirectory;
        public string TargetDirectory
        {
            get => _targetDirectory;
            set { _targetDirectory = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShortTargetDirectory)); }
        }

        public string ShortSourceDirectory => GetShortPath(SourceDirectory);
        public string ShortTargetDirectory => GetShortPath(TargetDirectory);

        public BackupType Type { get; set; }
        public string TranslatedType => ResourceSettings.GetString(Type.ToString());

        private string GetShortPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            var parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 3) return path;
            return "...\\" + Path.Combine(parts[parts.Length - 3], parts[parts.Length - 2], parts[parts.Length - 1]);
        }

        private BackupState _state;
        public BackupState State
        {
            get => _state;
            set 
            { 
                _state = value; 
                if (_state == BackupState.Paused)
                    _pauseEvent.Reset();
                else
                    _pauseEvent.Set();

                OnPropertyChanged(); 
                OnPropertyChanged(nameof(ProgressText)); 
                OnPropertyChanged(nameof(TranslatedState));
            }
        }

        public string TranslatedState => ResourceSettings.GetString(State.ToString());

        private string _remainingTimeText;
        public string RemainingTimeText
        {
            get => _remainingTimeText;
            set { _remainingTimeText = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressText)); }
        }

        public string ProgressText
        {
            get
            {
                if (State == BackupState.Error) return ResourceSettings.GetString("Error");
                if (Progress == 0 && State == BackupState.Inactive) return "";
                if (Progress == 100 && State == BackupState.Inactive) return ResourceSettings.GetString("Success");
                string text = $"{Progress}%";
                if (State == BackupState.Active && !string.IsNullOrEmpty(RemainingTimeText))
                {
                    text += $" ({RemainingTimeText})";
                }
                return text;
            }
        }

        public event EventHandler<BackupProgressEventArgs> OnProgressUpdate;

        public event EventHandler<(string source, string target, long size, float time, float encryptionTime)> OnFileCopied;

        public event EventHandler<string> OnBlockedProcessDetected;

        private int _progress;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressText)); }
        }

        public BackupJob(string name, string source, string target, BackupType type)
        {
            Name = name;
            SourceDirectory = source;
            TargetDirectory = target;
            Type = type;
            State = BackupState.Inactive;
            RemainingTimeText = "";
        }

        public BackupJob()
        {
            RemainingTimeText = "";
        }

        public void InitializeJobData()
        {
            if (string.IsNullOrEmpty(SourceDirectory) || !Directory.Exists(SourceDirectory)) return;

            try
            {
                string[] allFiles = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);
                long total = 0;
                long processed = 0;

                foreach (var filePath in allFiles)
                {
                    FileInfo fi = new FileInfo(filePath);
                    total += fi.Length;

                    if (!string.IsNullOrEmpty(TargetDirectory))
                    {
                        string relativePath = Path.GetRelativePath(SourceDirectory, filePath);
                        string targetFilePath = Path.Combine(TargetDirectory, relativePath);

                        if (File.Exists(targetFilePath))
                        {
                            FileInfo targetFi = new FileInfo(targetFilePath);
                            // Vérification rapide : taille identique et source pas plus récente que cible
                            if (fi.Length == targetFi.Length && fi.LastWriteTime <= targetFi.LastWriteTime)
                            {
                                processed += fi.Length;
                            }
                        }
                    }
                }

                TotalSize = total;
                CurrentSizeProcessed = processed;
                
                // Si tout est déjà identique, on met le progrès à 100% visuellement au démarrage
                if (TotalSize > 0 && CurrentSizeProcessed == TotalSize)
                {
                    Progress = 100;
                }
                else
                {
                    Progress = TotalSize == 0 ? 0 : (int)(CurrentSizeProcessed * 100 / TotalSize);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing job data for {Name}: {ex.Message}");
            }
        }

        public void Pause() => State = BackupState.Paused;
        public void Resume() => State = BackupState.Active;
        public void Stop()
        {
            _isStopped = true;
            _pauseEvent.Set(); // Unblock if paused
            State = BackupState.Inactive;
        }

        // helper : exécuter une action sur le thread UI (si possible)
        private void RunOnUI(Action action)
        {
            try
            {
                if (Application.Current?.Dispatcher != null)
                {
                    if (Application.Current.Dispatcher.CheckAccess())
                        action();
                    else
                        Application.Current.Dispatcher.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch
            {
                action();
            }
        }

        private long _totalSize;
        public long TotalSize
        {
            get => _totalSize;
            set { _totalSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(SizeProgressText)); }
        }

        private long _currentSizeProcessed;
        public long CurrentSizeProcessed
        {
            get => _currentSizeProcessed;
            set { _currentSizeProcessed = value; OnPropertyChanged(); OnPropertyChanged(nameof(SizeProgressText)); }
        }

        public string SizeProgressText => $"{FormatSize(CurrentSizeProcessed)} / {FormatSize(TotalSize)}";

        private string _throughputText;
        public string ThroughputText
        {
            get => _throughputText;
            set { _throughputText = value; OnPropertyChanged(); }
        }

        private string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double doubleBytes = bytes;
            int i = 0;
            while (doubleBytes >= 1024 && i < units.Length - 1)
            {
                doubleBytes /= 1024;
                i++;
            }
            return $"{doubleBytes:F2} {units[i]}";
        }

        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond <= 0) return "0 B/s";
            string[] units = { "B/s", "KB/s", "MB/s", "GB/s" };
            int i = 0;
            while (bytesPerSecond >= 1024 && i < units.Length - 1)
            {
                bytesPerSecond /= 1024;
                i++;
            }
            return $"{bytesPerSecond:F1} {units[i]}";
        }

        public void Execute()
        {
            _isStopped = false;
            _pauseEvent.Set();

            RunOnUI(() =>
            {
                State = BackupState.Active;
                Progress = 0;
                CurrentSizeProcessed = 0;
                TotalSize = 0;
                RemainingTimeText = "";
                ThroughputText = "";
            });

            Stopwatch overallStopwatch = Stopwatch.StartNew();
            Stopwatch updateStopwatch = Stopwatch.StartNew();

            var blockedProcessNames = GetBlockedProcessNames();

            CheckBlockedProcesses(blockedProcessNames);

            if (!Directory.Exists(SourceDirectory))
            {
                RunOnUI(() => State = BackupState.Error);
                return;
            }

            // Si sauvegarde complète, on vide la cible avant de commencer
            if (Type == BackupType.Full && Directory.Exists(TargetDirectory))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(TargetDirectory);
                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning target directory: {ex.Message}");
                    // On peut choisir de continuer ou d'arrêter selon la politique souhaitée
                }
            }

            string[] allFiles;
            try
            {
                allFiles = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Access denied to directory: {ex.Message}");
                RunOnUI(() => State = BackupState.Error);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing files: {ex.Message}");
                RunOnUI(() => State = BackupState.Error);
                return;
            }

            List<string> encryptedExtensions = AppSettings.Instance.EncryptedExtensions
                                                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(ext => ext.ToLower().Trim())
                                                .ToList();



            List<string> priorityExts = AppSettings.Instance.PriorityExtensions
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.ToLower().Trim())
                .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
                .ToList();

            var priorityFiles = new List<string>();
            var standardFiles = new List<string>();

            foreach (var file in allFiles)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (priorityExts.Contains(extension))
                {
                    priorityFiles.Add(file);
                }
                else
                {
                    standardFiles.Add(file);
                }
            }

            allFiles = priorityFiles.Concat(standardFiles).ToArray();
            int totalFiles = allFiles.Length;
            int processedCount = 0;

            long calculatedTotalSize = 0;
            foreach (var f in allFiles)
            {
                try
                {
                    calculatedTotalSize += new FileInfo(f).Length;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting file size for {f}: {ex.Message}");
                }
            }
            TotalSize = calculatedTotalSize;

            string cryptoSoftPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "CryptoSoft", "bin", "Debug", "net8.0", "win-x64", "CryptoSoft.exe"
            );

            if (!File.Exists(cryptoSoftPath))
            {
                cryptoSoftPath = Path.GetFullPath(cryptoSoftPath);
            }

            string encryptionKey = "EasySaveEncryptionKey";

            foreach (var filePath in allFiles)
            {
                if (_isStopped) break;
                _pauseEvent.Wait();

                // si process métier apparaît pendant la sauvegarde
                CheckBlockedProcesses(blockedProcessNames);

                try
                {
                    string relativePath = Path.GetRelativePath(SourceDirectory, filePath);
                    string targetFilePath = Path.Combine(TargetDirectory, relativePath);
                    string targetDir = Path.GetDirectoryName(targetFilePath);

                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    long currentFileSize = new FileInfo(filePath).Length;

                    bool shouldCopy = false;
                    if (Type == BackupType.Full) shouldCopy = true;
                    else if (Type == BackupType.Differential)
                    {
                        if (!File.Exists(targetFilePath) || File.GetLastWriteTime(filePath) > File.GetLastWriteTime(targetFilePath))
                            shouldCopy = true;
                    }

                    if (shouldCopy)
                    {
                        bool shouldEncrypt = false;
                        if (AppSettings.Instance.EncryptAll)
                        {
                            shouldEncrypt = true;
                        }
                        else
                        {
                            string fileExtension = Path.GetExtension(filePath).ToLower();
                            shouldEncrypt = encryptedExtensions.Contains(fileExtension);
                        }

                        long copyTime = 0;
                        long encryptionTime = 0;
                        Stopwatch stopwatchTotal = Stopwatch.StartNew();

                        bool isLargeFile = currentFileSize > (AppSettings.Instance.MaxLargeFileSizeMO * 1024 * 1024);
                        bool semaphoreAcquired = false;

                        try
                        {
                            if (isLargeFile)
                            {
                                _largeFileSemaphore.Wait();
                                semaphoreAcquired = true;
                            }

                            if (shouldEncrypt && File.Exists(cryptoSoftPath))
                            {
                                // STRATÉGIE SÉCURISÉE : Chiffrement LOCAL avant ENVOI
                                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + Path.GetExtension(filePath));
                                try
                                {
                                    // 1. Copie vers un fichier temporaire LOCAL (vitesse maximale)
                                    CopyFileInChunks(filePath, tempFile, totalFiles, processedCount, updateStopwatch);

                                    // 2. Chiffrement du fichier temporaire par CryptoSoft
                                    _pauseEvent.Wait();
                                    ProcessStartInfo startInfo = new ProcessStartInfo
                                    {
                                        FileName = cryptoSoftPath,
                                        Arguments = $"\"{tempFile}\" \"{encryptionKey}\"",
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    };

                                    using (Process process = Process.Start(startInfo))
                                    {
                                        process.WaitForExit();
                                        encryptionTime = (process.ExitCode >= 0) ? process.ExitCode : 0;
                                    }

                                    // 3. ENVOI du fichier DÉJÀ CHIFFRÉ vers la destination
                                    if (File.Exists(targetFilePath)) File.Delete(targetFilePath);
                                    File.Move(tempFile, targetFilePath);
                                }
                                finally
                                {
                                    if (File.Exists(tempFile)) File.Delete(tempFile);
                                }
                            }
                            else
                            {
                                // Copie directe si pas de chiffrement
                                CopyFileInChunks(filePath, targetFilePath, totalFiles, processedCount, updateStopwatch);
                            }
                        }
                        finally
                        {
                            if (semaphoreAcquired)
                            {
                                _largeFileSemaphore.Release();
                            }
                        }
                        
                        stopwatchTotal.Stop();
                        copyTime = stopwatchTotal.ElapsedMilliseconds - encryptionTime;

                        if (_isStopped) break;

                        OnFileCopied?.Invoke(this, (filePath, targetFilePath, currentFileSize, copyTime, encryptionTime));
                    }
                    else
                    {
                        // File skipped but counts as processed size for progress
                        CurrentSizeProcessed += currentFileSize;
                    }

                    processedCount++;

                    // Temps restant retiré selon demande utilisateur.

                    // Force update after each file anyway
                    OnProgressUpdate?.Invoke(this, new BackupProgressEventArgs(
                        totalFiles,
                        processedCount,
                        TotalSize,
                        CurrentSizeProcessed,
                        Path.GetFileName(filePath),
                        filePath,
                        targetFilePath
                    ));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    processedCount++;
                }
            }

            overallStopwatch.Stop();
            updateStopwatch.Stop();

            RunOnUI(() =>
            {
                State = BackupState.Inactive;
                RemainingTimeText = "";
                ThroughputText = "";
                // (Progress restera à 100 via les events, sinon tu peux forcer ici si tu veux)
            });
        }

        private void CopyFileInChunks(string sourcePath, string targetPath, int totalFiles, int processedCount, Stopwatch updateStopwatch)
        {
            const int bufferSize = 64 * 1024; // 64KB
            byte[] buffer = new byte[bufferSize];
            long lastSize = CurrentSizeProcessed;
            var blockedProcessNames = GetBlockedProcessNames();

            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (FileStream targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (_isStopped) return;
                    _pauseEvent.Wait();

                    // si process métier apparaît pendant la copie d'un fichier volumineux
                    CheckBlockedProcesses(blockedProcessNames);

                    targetStream.Write(buffer, 0, bytesRead);
                    CurrentSizeProcessed += bytesRead;

                    // Update UI every 200ms
                    if (updateStopwatch.ElapsedMilliseconds > 200)
                    {
                        double elapsedSeconds = updateStopwatch.ElapsedMilliseconds / 1000.0;
                        long bytesSinceLastUpdate = CurrentSizeProcessed - lastSize;
                        double speed = bytesSinceLastUpdate / elapsedSeconds;

                        ThroughputText = FormatSpeed(speed);
                        lastSize = CurrentSizeProcessed;

                        OnProgressUpdate?.Invoke(this, new BackupProgressEventArgs(
                            totalFiles,
                            processedCount,
                            TotalSize,
                            CurrentSizeProcessed,
                            Path.GetFileName(sourcePath),
                            sourcePath,
                            targetPath
                        ));
                        updateStopwatch.Restart();
                    }
                }
            }
        }

        private List<string> GetBlockedProcessNames()
        {
            return AppSettings.Instance.BlockedProcesses
                                      .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(p => p.Trim().ToLower())
                                      .ToList();
        }

        private void CheckBlockedProcesses(List<string> blockedProcessNames)
        {
            foreach (var processName in blockedProcessNames)
            {
                if (string.IsNullOrWhiteSpace(processName)) continue;

                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        process.Dispose();
                    }
                    
                    // On notifie l'UI (qui pourra afficher une MessageBox)
                    OnBlockedProcessDetected?.Invoke(this, processName);

                    // On passe en pause et on attend que l'utilisateur relance
                    State = BackupState.Paused;
                    _pauseEvent.Wait();
                    return;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}