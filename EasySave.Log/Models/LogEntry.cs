using System;

namespace EasySave.Log.Models
{
    public class LogEntry
    {
        public string Name { get; set; }
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public long FileSize { get; set; }
        public float TransferTime { get; set; }
        public float EncryptionTime { get; set; }
        public string Time { get; set; }

        public LogEntry()
        {
            Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public LogEntry(string name, string source, string target, long size, float time, float encryptionTime)
        {
            Name = name;
            SourceFile = source;
            TargetFile = target;
            FileSize = size;
            TransferTime = time;
            EncryptionTime = encryptionTime;
            Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}