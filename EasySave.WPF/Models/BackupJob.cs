using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySave.WPF.Config;
using System.Runtime.CompilerServices;

namespace EasySave.WPF.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType Type { get; set; }

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
                if (State == BackupState.Error) return "Error";
                if (Progress == 100 && State == BackupState.Inactive) return "Success";
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

        public void Execute()
        {
            State = BackupState.Active;
            Progress = 0;
            RemainingTimeText = "";
            Stopwatch overallStopwatch = Stopwatch.StartNew();

            var blockedProcessNames = AppSettings.Instance.BlockedProcesses
                                          .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(p => p.Trim().ToLower())
                                          .ToList();

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
                    throw new IOException($"The process '{processName}' is running and must be closed before starting a backup.");
                }
            }


            if (!Directory.Exists(SourceDirectory))
            {
                State = BackupState.Error;
                return;
            }
            var allFiles = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);

            List<string> encryptedExtensions = AppSettings.Instance.EncryptedExtensions
                                                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(ext => ext.ToLower().Trim())
                                                    .ToList();

            int totalFiles = allFiles.Length;
            int processedCount = 0;

            long totalSize = 0;
            foreach (var f in allFiles) totalSize += new FileInfo(f).Length;

            long currentSizeProcessed = 0;
            
            string cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "CryptoSoft", "bin", "Debug", "net8.0", "win-x64", "CryptoSoft.exe");
            string encryptionKey = "EasySaveEncryptionKey";

            foreach (var filePath in allFiles)
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

                    if (shouldEncrypt)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = cryptoSoftPath,
                            Arguments = $"\"{targetFilePath}\" \"{encryptionKey}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
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
                    RemainingTimeText = t.ToString(@"hh\:mm\:ss");
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

            overallStopwatch.Stop();
            State = BackupState.Inactive;
            RemainingTimeText = "";
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}