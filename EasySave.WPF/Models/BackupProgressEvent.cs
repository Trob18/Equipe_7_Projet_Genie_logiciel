using System;

namespace EasySave.WPF.Models
{
    public class BackupProgressEventArgs : EventArgs
    {
        public int TotalFiles { get; set; }
        public int FilesProcessed { get; set; }
        public long TotalSize { get; set; }
        public long SizeProcessed { get; set; }
        public string CurrentFileName { get; set; }
        public string CurrentSourcePath { get; set; }
        public string CurrentTargetPath { get; set; }
        public int Percentage => TotalFiles == 0 ? 0 : (FilesProcessed * 100 / TotalFiles);

        public BackupProgressEventArgs(int total, int processed, long totalSize, long sizeProcessed, string currentFile, string src, string dest)
        {
            TotalFiles = total;
            FilesProcessed = processed;
            TotalSize = totalSize;
            SizeProcessed = sizeProcessed;
            CurrentFileName = currentFile;
            CurrentSourcePath = src;
            CurrentTargetPath = dest;
        }
    }
}