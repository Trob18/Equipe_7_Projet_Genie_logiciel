using EasySave.Log.Interfaces;
using EasySave.Log.Loggers;

namespace EasySave.Log
{
    public static class LoggerCrea
    {
        public static ILogger CreateLogger(string format, string logDirectory)
        {
            if (format?.ToLower() == "xml")
            {
                return new XmlLogger(logDirectory);
            }
            return new JsonLogger(logDirectory);
        }
    }
}