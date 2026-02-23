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
        private static readonly object _lock = new object();
        private static List<LogEntry> _currentLogs = null;
        private static string _currentLogFile = null;

        public XmlLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        private void EnsureLoaded(string filePath)
        {
            if (_currentLogs != null && _currentLogFile == filePath) return;

            var serializer = new XmlSerializer(typeof(List<LogEntry>));
            if (File.Exists(filePath))
            {
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        if (stream.Length > 0)
                        {
                            _currentLogs = (List<LogEntry>)serializer.Deserialize(stream);
                        }
                    }
                }
                catch
                {
                    _currentLogs = new List<LogEntry>();
                }
            }
            else
            {
                _currentLogs = new List<LogEntry>();
            }
            _currentLogFile = filePath;
        }

        public void WriteLog(LogEntry logEntry)
        {
            string filePath = GetLogFilePath();
            
            lock (_lock)
            {
                EnsureLoaded(filePath);
                _currentLogs.Add(logEntry);

                var serializer = new XmlSerializer(typeof(List<LogEntry>));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, _currentLogs);
                }
            }
        }

        private string GetLogFilePath()
        {
            return Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
        }
    }
}