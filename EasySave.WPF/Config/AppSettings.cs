using EasySave.WPF.Enumerations;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.WPF.Config
{
    public class AppSettings
    {
        private static AppSettings _instance;
        private static readonly object _lock = new object();

        private readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");


        private Language _language;
        public Language Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    SaveSettings();
                }
            }
        }

        private string _logFormat;
        public string LogFormat
        {
            get => _logFormat;
            set
            {
                if (_logFormat != value)
                {
                    _logFormat = value;
                    SaveSettings();
                }
            }
        }

        public string LogDirectory { get; set; }
        public string StateDirectory { get; set; }

        private string _encryptedExtensions;
        public string EncryptedExtensions
        {
            get => _encryptedExtensions;
            set
            {
                if (_encryptedExtensions != value)
                {
                    _encryptedExtensions = value;
                    SaveSettings();
                }
            }
        }
        
        private bool _encryptAll;
        public bool EncryptAll
        {
            get => _encryptAll;
            set
            {
                if (_encryptAll != value)
                {
                    _encryptAll = value;
                    SaveSettings();
                }
            }
        }

        private string _blockedProcesses;
        public string BlockedProcesses
        {
            get => _blockedProcesses;
            set
            {
                if (_blockedProcesses != value)
                {
                    _blockedProcesses = value;
                    SaveSettings();
                }
            }
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

        private AppSettings()
        {
            _language = Language.English;
            _logFormat = "json";
            _encryptedExtensions = "";
            _encryptAll = false;
            _blockedProcesses = "notepad,mspaint";
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            StateDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LoadSettings();
        }


        private void LoadSettings()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_configFilePath);

                    var savedSettings = JsonSerializer.Deserialize<AppSettingsDto>(json);

                    if (savedSettings != null)
                    {
                        _language = savedSettings.Language;
                        _logFormat = savedSettings.LogFormat;
                        _encryptedExtensions = savedSettings.EncryptedExtensions;
                        _encryptAll = savedSettings.EncryptAll;
                        _blockedProcesses = savedSettings.BlockedProcesses ?? "notepad,mspaint";
                    }
                }
                catch
                {
                }
            }
        }

        private void SaveSettings()
        {
            var settingsToSave = new AppSettingsDto
            {
                Language = _language,
                LogFormat = _logFormat,
                EncryptedExtensions = _encryptedExtensions,
                EncryptAll = _encryptAll,
                BlockedProcesses = _blockedProcesses
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settingsToSave, options);
            File.WriteAllText(_configFilePath, json);
        }
    }

    public class AppSettingsDto
    {
        public Language Language { get; set; }
        public string LogFormat { get; set; }
        public string EncryptedExtensions { get; set; }
        public bool EncryptAll { get; set; }
        public string BlockedProcesses { get; set; }
    }
}