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

        public static void UpdateState(StateLog stateLog)
        {
            lock (_writeLock)
            {
                string stateDir = AppSettings.Instance.StateDirectory;
                string stateFile = Path.Combine(stateDir, "state.json");

                if (!Directory.Exists(stateDir))
                {
                    Directory.CreateDirectory(stateDir);
                }

                List<StateLog> currentStateList = new List<StateLog>();
                if (File.Exists(stateFile))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(stateFile);
                        currentStateList = JsonSerializer.Deserialize<List<StateLog>>(jsonContent) ?? new List<StateLog>();
                    }
                    catch
                    {
                        currentStateList = new List<StateLog>();
                    }
                }

                var existingState = currentStateList.Find(s => s.BackupName == stateLog.BackupName);
                if (existingState != null)
                {
                    currentStateList.Remove(existingState);
                }

                currentStateList.Add(stateLog);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(currentStateList, options);

                File.WriteAllText(stateFile, jsonString);
            }
        }
    }
}