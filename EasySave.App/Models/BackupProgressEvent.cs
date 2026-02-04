using System;

namespace EasySave.App.Models
{
    public class BackupProgressEventArgs : EventArgs
    {
        public int TotalFiles { get; set; }
        public int FilesProcessed { get; set; }
        public string CurrentFileName { get; set; }
        public int Percentage => TotalFiles == 0 ? 0 : (FilesProcessed * 100 / TotalFiles);

        public BackupProgressEventArgs(int total, int processed, string currentFile)
        {
            TotalFiles = total;
            FilesProcessed = processed;
            CurrentFileName = currentFile;
        }
    }
}