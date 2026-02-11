using System;

namespace EasySave.WPF.State
{
    public class StateLog
    {
        public string BackupName { get; set; }
        public DateTime Timestamp { get; set; }
        public string State { get; set; }
        public int TotalFilesToCopy { get; set; }
        public long TotalFilesSize { get; set; }
        public int Progression { get; set; }

        public int NbFilesLeftToDo { get; set; }
        public long NbFilesSizeLeftToDo { get; set; }

        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }

        public StateLog()
        {
            Timestamp = DateTime.Now;
        }
    }
}