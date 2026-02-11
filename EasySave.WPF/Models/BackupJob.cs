using EasySave.WPF.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.WPF.Models
{
    public class BackupJob
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType Type { get; set; }
        public BackupState State { get; set; }

        public event EventHandler<BackupProgressEventArgs> OnProgressUpdate;

        public event EventHandler<(string source, string target, long size, float time)> OnFileCopied;

        public BackupJob(string name, string source, string target, BackupType type)
        {
            Name = name;
            SourceDirectory = source;
            TargetDirectory = target;
            Type = type;
            State = BackupState.Inactive;
        }
        public BackupJob()
        {
        }

        public void Execute()
        {
            State = BackupState.Active;

            if (!Directory.Exists(SourceDirectory))
            {
                State = BackupState.Error;
                return;
            }

            var allFiles = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);
            int totalFiles = allFiles.Length;
            int processedCount = 0;

            long totalSize = 0;
            foreach (var f in allFiles) totalSize += new FileInfo(f).Length;

            long currentSizeProcessed = 0;

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
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    File.Copy(filePath, targetFilePath, true);
                    stopwatch.Stop();
                    OnFileCopied?.Invoke(this, (filePath, targetFilePath, currentFileSize, stopwatch.ElapsedMilliseconds));
                }
                processedCount++;
                currentSizeProcessed += currentFileSize;

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

            State = BackupState.Inactive;
        }
    }
}