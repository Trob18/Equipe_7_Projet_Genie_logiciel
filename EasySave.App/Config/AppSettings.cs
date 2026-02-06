using EasySave.App.Enumerations;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.App.Config
{
    public sealed class AppSettings
    {
        private static AppSettings _instance;

        private static readonly object _lock = new object();
        private readonly string _configFilePath;

        private const string CURRENT_VERSION = "1.0"; // Version de référence

        private AppSettings()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configFolder = Path.Combine(appDataPath, "EasySave");

            _configFilePath = Path.Combine(configFolder, "config.json");

            Language = Language.English;
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            StateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state");

            LoadOrCreateConfiguration(configFolder);
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
        public string LogDirectory { get; set; }
        public string StateDirectory { get; set; }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var configToSave = new ConfigDto
                {
                    Version = CURRENT_VERSION,
                    Language = this.Language
                };

                string jsonString = JsonSerializer.Serialize(configToSave, options);
                File.WriteAllText(_configFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur critique : Impossible de générer la config : {ex.Message}");
            }
        }

        private void LoadOrCreateConfiguration(string configFolder)
        {
            if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);

            if (File.Exists(_configFilePath))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    string jsonContent = File.ReadAllText(_configFilePath);
                    var configData = JsonSerializer.Deserialize<ConfigDto>(jsonContent, options);

                    if (configData != null && configData.Version == CURRENT_VERSION)
                    {
                        this.Language = configData.Language;
                    }
                    else
                    {
                        Save();
                    }
                }
                catch
                {
                    Save();
                }
            }
            else
            {
                Save();
            }
        }

        private class ConfigDto
        {
            public string Version { get; set; }
            public Language Language { get; set; }
        }
    }
}