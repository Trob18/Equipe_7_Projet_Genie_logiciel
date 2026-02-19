using EasySave.Log.Interfaces;
using EasySave.Log.Models;
using System.Collections.Generic;

namespace EasySave.Log.Loggers
{
    public class HybridLogger : ILogger
    {
        private readonly List<ILogger> _loggers;

        public HybridLogger(List<ILogger> loggers)
        {
            _loggers = loggers;
        }

        public void WriteLog(LogEntry logEntry)
        {
            foreach (var logger in _loggers)
            {
                logger.WriteLog(logEntry);
            }
        }
    }
}