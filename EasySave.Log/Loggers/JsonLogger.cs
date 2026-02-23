using EasySave.Log.Interfaces;
using EasySave.Log.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Log.Loggers
{
    public class JsonLogger : ILogger
    {
        private readonly string _logDirectory;
        private static readonly object _lock = new object();
        private static List<LogEntry> _currentLogs = null;
        private static string _currentLogFile = null;

        public JsonLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        private void EnsureLoaded(string filePath)
        {
            if (_currentLogs != null && _currentLogFile == filePath) return;

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    _currentLogs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent) ?? new List<LogEntry>();
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
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string filePath = Path.Combine(_logDirectory, fileName);

            lock (_lock)
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                EnsureLoaded(filePath);
                _currentLogs.Add(logEntry);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_currentLogs, options);
                File.WriteAllText(filePath, jsonString);
            }
        }
    }
}