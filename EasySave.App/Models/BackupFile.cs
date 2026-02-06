using System;
using System.IO;

namespace EasySave.App.Models
{
    public class BackupFile
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string Directory { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }

        public BackupFile(string path)
        {
            if (File.Exists(path))
            {
                var info = new FileInfo(path);
                FullPath = path;
                Name = info.Name;
                Directory = info.DirectoryName;
                Size = info.Length;
                LastWriteTime = info.LastWriteTime;
            }
        }
    }
}