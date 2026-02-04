using EasySave.App.Enumerations;
using System;
using System.IO;

namespace EasySave.App.Config
{
    public sealed class AppSettings
    {
        private static AppSettings _instance;

        private static readonly object _lock = new object();

        private AppSettings()
        {
            Language = Language.English;
            LogFormat = "json";

            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            StateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state");
        }

        public static AppSettings Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppSettings();
                    }
                    return _instance;
                }
            }
        }


        public Language Language { get; set; }
        public string LogFormat { get; set; }
        public string LogDirectory { get; set; }
        public string StateDirectory { get; set; }
    }
}