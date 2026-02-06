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

        public JsonLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        public void WriteLog(LogEntry logEntry)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string filePath = Path.Combine(_logDirectory, fileName);

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            var logs = new List<LogEntry>();

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent) ?? new List<LogEntry>();
                }
                catch
                {
                    logs = new List<LogEntry>();
                }
            }

            logs.Add(logEntry);

            var options = new JsonSerializerOptions { WriteIndented = true };

            string jsonString = JsonSerializer.Serialize(logs, options);

            File.WriteAllText(filePath, jsonString);
        }
    }
}