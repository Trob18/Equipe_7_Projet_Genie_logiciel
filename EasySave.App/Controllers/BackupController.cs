using EasySave.App.Config;
using EasySave.App.Enumerations;
using EasySave.App.Models;
using EasySave.App.State;
using EasySave.Log;
using EasySave.Log.Interfaces;
using EasySave.Log.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace EasySave.App.Controllers
{
    public class BackupController
    {
        private List<BackupJob> _backupJobs;
        private readonly ILogger _logger;

        private readonly string _jobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
        public BackupController()
        {
            _backupJobs = new List<BackupJob>();

            _logger = LoggerCrea.CreateLogger(
                AppSettings.Instance.LogFormat,
                AppSettings.Instance.LogDirectory
            );

            LoadJobs();
        }
        private void LoadJobs()
        {
            if (File.Exists(_jobsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_jobsFilePath);
                    _backupJobs = JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
                }
                catch
                {
                    _backupJobs = new List<BackupJob>();
                }
            }
            else
            {
                _backupJobs = new List<BackupJob>();
            }

            foreach (var job in _backupJobs)
            {
                job.OnProgressUpdate += OnJobProgressUpdate;
                job.OnFileCopied += OnJobFileCopied;
            }
        }
        public void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_backupJobs, options);
            File.WriteAllText(_jobsFilePath, json);
        }
        public List<BackupJob> GetJobs()
        {
            return _backupJobs;
        }


        public void CreateJob(string name, string source, string target, BackupType type)
        {
            var newJob = new BackupJob(name, source, target, type);

            newJob.OnProgressUpdate += OnJobProgressUpdate;

            newJob.OnFileCopied += OnJobFileCopied;

            _backupJobs.Add(newJob);
            SaveJobs();
        }

        public void ExecuteJob(int index)
        {
            if (index >= 0 && index < _backupJobs.Count)
            {
                var job = _backupJobs[index];

                UpdateState(job, "ACTIVE", 0, 0, "Initialisation...");

                job.Execute();

                UpdateState(job, "INACTIVE", 100, 0, "Terminé");
            }
        }

        private void OnJobFileCopied(object sender, (string Src, string Dest, long Size, float Time) data)
        {
            var job = (BackupJob)sender;

            var logEntry = new LogEntry
            {
                Name = job.Name,
                SourceFile = data.Src,
                TargetFile = data.Dest,
                FileSize = data.Size,
                TransferTime = data.Time,
                Timestamp = DateTime.Now
            };

            _logger.WriteLog(logEntry);
        }

        private void OnJobProgressUpdate(object sender, BackupProgressEventArgs e)
        {
            var job = (BackupJob)sender;
            UpdateState(job, "ACTIVE", e.Percentage, e.TotalFiles - e.FilesProcessed, e.CurrentFileName);
        }

        private void UpdateState(BackupJob job, string status, int progress, int left, string currentFile)
        {
            var stateLog = new StateLog
            {
                BackupName = job.Name,
                SourceFilePath = currentFile,
                TargetFilePath = job.TargetDirectory,
                State = status,
                TotalFilesToCopy = 0,
                NbFilesLeftToDo = left,
                Progression = progress,
                Timestamp = DateTime.Now
            };

            StateSettings.UpdateState(stateLog);
        }
    }
}