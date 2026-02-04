using System;

namespace EasySave.App.State
{
    public class StateLog
    {
        public string BackupName { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        public string State { get; set; }
        public int TotalFilesToCopy { get; set; }
        public long TotalFilesSize { get; set; }
        public int NbFilesLeftToDo { get; set; }
        public int Progression { get; set; }
        public DateTime Timestamp { get; set; }

        public StateLog()
        {
            Timestamp = DateTime.Now;
        }
    }
}