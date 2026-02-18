using EasySave.WPF.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.WPF.State
{
    public static class StateSettings
    {
        private static readonly object _writeLock = new object();

        private static string NormalizeName(string name)
            => (name ?? string.Empty).Trim().ToLowerInvariant();

        public static void UpdateState(StateLog stateLog)
        {
            if (stateLog == null) return;

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
                string incomingKey = NormalizeName(stateLog.BackupName);
                stateLog.BackupName = stateLog.BackupName?.Trim(); // garde l'affichage propre

                currentStateList.RemoveAll(s => NormalizeName(s.BackupName) == incomingKey);

                currentStateList.Add(stateLog);

                currentStateList = currentStateList
                    .OrderBy(s => NormalizeName(s.BackupName))
                    .ToList();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(currentStateList, options);

                File.WriteAllText(stateFile, jsonString);
            }
        }
    }
}
