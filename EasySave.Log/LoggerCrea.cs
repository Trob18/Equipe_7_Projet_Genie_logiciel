using EasySave.Log.Interfaces;
using EasySave.Log.Loggers;
using System.Collections.Generic;

namespace EasySave.Log
{
    public static class LoggerCrea
    {
        public static ILogger CreateLogger(string format, string logDirectory, string serverIp = "127.0.0.1")
        {
            string f = format?.ToLower().Trim() ?? "json";

            if (f == "json+network")
            {
                return new HybridLogger(new List<ILogger> {
            new JsonLogger(logDirectory),
            new NetworkLogger("json", serverIp)
        });
            }

            if (f == "xml+network")
            {
                return new HybridLogger(new List<ILogger> {
            new XmlLogger(logDirectory),
            new NetworkLogger("xml", serverIp)
        });
            }

            if (f == "network" || f == "tcp")
            {
                return new NetworkLogger("json", serverIp);
            }

            if (f == "xml") return new XmlLogger(logDirectory);

            return new JsonLogger(logDirectory);
        }
    }
}