using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows; 

namespace EasySave.WPF.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
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

        private string GetShortPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            var parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 2) return path;
            return "...\\" + Path.Combine(parts[parts.Length - 2], parts[parts.Length - 1]);
        }

        private BackupState _state;
        public BackupState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressText)); }
        }

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

        public void Execute()
        {
            RunOnUI(() =>
            {
                State = BackupState.Active;
                Progress = 0;
                RemainingTimeText = "";
            });

            Stopwatch overallStopwatch = Stopwatch.StartNew();

            var blockedProcessNames = GetBlockedProcessNames();

            CheckBlockedProcesses(blockedProcessNames);

            if (!Directory.Exists(SourceDirectory))
            {
                RunOnUI(() => State = BackupState.Error);
                return;
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

            long totalSize = 0;
            foreach (var f in allFiles)
            {
                try
                {
                    totalSize += new FileInfo(f).Length;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting file size for {f}: {ex.Message}");
                }
            }

            long currentSizeProcessed = 0;

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
                        long copyTime = 0;
                        long encryptionTime = 0;

                        Stopwatch stopwatchCopy = Stopwatch.StartNew();
                        File.Copy(filePath, targetFilePath, true);
                        stopwatchCopy.Stop();
                        copyTime = stopwatchCopy.ElapsedMilliseconds;

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

                        if (shouldEncrypt && File.Exists(cryptoSoftPath))
                        {
                            try
                            {
                                ProcessStartInfo startInfo = new ProcessStartInfo
                                {
                                    FileName = cryptoSoftPath,
                                    Arguments = $"\"{targetFilePath}\" \"{encryptionKey}\"",
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                };

                                using (Process process = Process.Start(startInfo))
                                {
                                    process.WaitForExit();
                                    if (process.ExitCode >= 0)
                                    {
                                        encryptionTime = process.ExitCode;
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"CryptoSoft encryption failed for {targetFilePath} with exit code {process.ExitCode}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Encryption error for {targetFilePath}: {ex.Message}");
                            }
                        }

                        OnFileCopied?.Invoke(this, (filePath, targetFilePath, currentFileSize, copyTime, encryptionTime));
                    }

                    processedCount++;
                    currentSizeProcessed += currentFileSize;

                    long elapsedMs = overallStopwatch.ElapsedMilliseconds;
                    if (elapsedMs > 500 && currentSizeProcessed > 0)
                    {
                        double bytesPerMs = (double)currentSizeProcessed / elapsedMs;
                        long remainingBytes = totalSize - currentSizeProcessed;
                        double remainingMs = remainingBytes / bytesPerMs;
                        TimeSpan t = TimeSpan.FromMilliseconds(remainingMs);

                        RunOnUI(() => RemainingTimeText = t.ToString(@"hh\:mm\:ss"));
                    }

                    OnProgressUpdate?.Invoke(this, new BackupProgressEventArgs(
                        totalFiles,
                        processedCount,
                        totalSize,
                        currentSizeProcessed,
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

            RunOnUI(() =>
            {
                State = BackupState.Inactive;
                RemainingTimeText = "";
            });
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
                    throw new BlockedProcessException(processName);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}