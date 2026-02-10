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
        private ILogger _logger;

        private readonly string _jobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
        public event Action<string, int> OnGlobalProgress;
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
            if (_backupJobs.Count >= 5)
            {
                throw new Exception(ResourceSettings.GetString("MaxJobsReached"));
            }
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

                UpdateState(job, "ACTIVE", 0, 0, 0, 0, 0, "Lancement...", "");

                job.Execute();

                UpdateState(job, "NON ACTIVE", 100, 0, 0, 0, 0, "Terminé", "");
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
                TransferTime = data.Time
            };

            _logger.WriteLog(logEntry);
        }

        private void OnJobProgressUpdate(object sender, BackupProgressEventArgs e)
        {
            var job = (BackupJob)sender;

            int filesLeft = e.TotalFiles - e.FilesProcessed;
            long sizeLeft = e.TotalSize - e.SizeProcessed;

            UpdateState(
                job,
                "ACTIVE",
                e.Percentage,
                e.TotalFiles,
                e.TotalSize,
                filesLeft,
                sizeLeft,
                e.CurrentSourcePath,
                e.CurrentTargetPath
            );

            OnGlobalProgress?.Invoke(job.Name, e.Percentage);
        }

        private void UpdateState(BackupJob job, string status, int progress, int totalFiles, long totalSize, int filesLeft, long sizeLeft, string currentSrc, string currentDest)
        {
            var stateLog = new StateLog
            {
                BackupName = job.Name,
                Timestamp = DateTime.Now,
                State = status,

                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalSize,
                Progression = progress,

                NbFilesLeftToDo = filesLeft,
                NbFilesSizeLeftToDo = sizeLeft,

                SourceFilePath = currentSrc,
                TargetFilePath = currentDest
            };

            StateSettings.UpdateState(stateLog);
        }


        public void DeleteJob(int index)
        {
            if (index >= 0 && index < _backupJobs.Count)
            {
                _backupJobs.RemoveAt(index);

                SaveJobs();
            }
        }
        public void UpdateLogger()
        {
            _logger = LoggerCrea.CreateLogger(
                AppSettings.Instance.LogFormat,
                AppSettings.Instance.LogDirectory
            );
        }
    }
}