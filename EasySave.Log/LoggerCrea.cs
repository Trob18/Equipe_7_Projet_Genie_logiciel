using EasySave.Log.Interfaces;
using EasySave.Log.Loggers;

namespace EasySave.Log
{
    public static class LoggerCrea
    {
        public static ILogger CreateLogger(string logDirectory)
        {
            return new JsonLogger(logDirectory);
        }
    }
}