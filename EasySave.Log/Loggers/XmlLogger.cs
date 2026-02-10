using EasySave.Log.Interfaces;
using EasySave.Log.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EasySave.Log.Loggers
{
    public class XmlLogger : ILogger
    {
        private readonly string _logDirectory;

        public XmlLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void WriteLog(LogEntry logEntry)
        {
            string filePath = GetLogFilePath();
            var logList = new List<LogEntry>();
            var serializer = new XmlSerializer(typeof(List<LogEntry>));

            if (File.Exists(filePath))
            {
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        if (stream.Length > 0)
                        {
                            logList = (List<LogEntry>)serializer.Deserialize(stream);
                        }
                    }
                }
                catch
                {
                    logList = new List<LogEntry>();
                }
            }
            logList.Add(logEntry);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, logList);
            }
        }

        private string GetLogFilePath()
        {
            return Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
        }
    }
}