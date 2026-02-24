using EasySave.WPF.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.WPF.State
{
    public static class StateSettings
    {
        private static readonly object _writeLock = new object();
        private static List<StateLog> _currentStateList = null;

        private static void EnsureLoaded()
        {
            if (_currentStateList != null) return;

            string stateDir = AppSettings.Instance.StateDirectory;
            string stateFile = Path.Combine(stateDir, "state.json");

            if (File.Exists(stateFile))
            {
                try
                {
                    string jsonContent = File.ReadAllText(stateFile);
                    _currentStateList = JsonSerializer.Deserialize<List<StateLog>>(jsonContent) ?? new List<StateLog>();
                }
                catch
                {
                    _currentStateList = new List<StateLog>();
                }
            }
            else
            {
                _currentStateList = new List<StateLog>();
            }
        }

        /// <summary>
        /// Updates or adds a backup state entry in the state tracking JSON file.
        /// </summary>
        public static void UpdateState(StateLog stateLog)
        {
            lock (_writeLock)
            {
                EnsureLoaded();

                var existingState = _currentStateList.Find(s => s.BackupName == stateLog.BackupName);
                if (existingState != null)
                {
                    _currentStateList.Remove(existingState);
                }

                _currentStateList.Add(stateLog);

                string stateDir = AppSettings.Instance.StateDirectory;
                string stateFile = Path.Combine(stateDir, "state.json");

                if (!Directory.Exists(stateDir))
                {
                    Directory.CreateDirectory(stateDir);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_currentStateList, options);

                File.WriteAllText(stateFile, jsonString);
            }
        }
    }
}