using EasySave.Log;
using EasySave.Log.Interfaces;
using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using EasySave.WPF.Models;
using EasySave.WPF.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace EasySave.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Language _startupLanguage;
        public ObservableCollection<BackupJob> BackupJobs { get; set; }

        private BackupJob _selectedJob;
        public BackupJob SelectedJob
        {
            get => _selectedJob;
            set { _selectedJob = value; OnPropertyChanged(); }
        }

        public List<BackupJob> SelectedJobsList { get; set; } = new List<BackupJob>();

        public LanguageProxy Labels { get; } = new LanguageProxy();

        private string _jobName;
        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(); } }

        private string _sourcePath;
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(); } }

        private string _targetPath;
        public string TargetPath { get => _targetPath; set { _targetPath = value; OnPropertyChanged(); } }

        private BackupType _selectedType;
        public BackupType SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); } }

        public ObservableCollection<string> EncryptedExtensionsList { get; set; }

        private string _newExtensionInput;
        public string NewExtensionInput { get => _newExtensionInput; set { _newExtensionInput = value; OnPropertyChanged(); } }
        
        public ObservableCollection<string> BlockedProcessesList { get; set; }
        private string _newProcessInput;
        public string NewProcessInput { get => _newProcessInput; set { _newProcessInput = value; OnPropertyChanged(); } }

        public bool EncryptAll
        {
            get => AppSettings.Instance.EncryptAll;
            set
            {
                if (AppSettings.Instance.EncryptAll != value)
                {
                    AppSettings.Instance.EncryptAll = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EncryptAll));
                }
            }
        }

        private int _progressValue;
        public int ProgressValue { get => _progressValue; set { _progressValue = value; OnPropertyChanged(); } }

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        private Visibility _restartWarningVisibility;
        public Visibility RestartWarningVisibility
        {
            get => _restartWarningVisibility;
            set { _restartWarningVisibility = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> LogFormats { get; } = new ObservableCollection<string> { "json", "xml" };

        public string SelectedLogFormat
        {
            get => AppSettings.Instance.LogFormat;
            set
            {
                if (AppSettings.Instance.LogFormat != value)
                {
                    AppSettings.Instance.LogFormat = value;
                    OnPropertyChanged();
                    UpdateLogger(false);
                }
            }
        }
        public Language SelectedLanguage
        {
            get => AppSettings.Instance.Language;
            set
            {
                if (AppSettings.Instance.Language != value)
                {
                    AppSettings.Instance.Language = value;
                    OnPropertyChanged();

                    if (value == _startupLanguage)
                    {
                        RestartWarningVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        RestartWarningVisibility = Visibility.Visible;
                    }
                }
            }
        }

        private readonly string _jobsFilePath;
        private ILogger _logger;

        public ICommand CreateJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ExecuteJobCommand { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }
        public ICommand AddProcessCommand { get; }
        public ICommand RemoveProcessCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseTargetCommand { get; }

        public string this[string key] => ResourceSettings.GetString(key);

        public MainViewModel()
        {
            _startupLanguage = AppSettings.Instance.Language;
            RestartWarningVisibility = Visibility.Collapsed;
            SelectedType = BackupType.Full;

            _jobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");

            UpdateLogger(true);

            BackupJobs = new ObservableCollection<BackupJob>();
            LoadJobs();

            EncryptedExtensionsList = new ObservableCollection<string>(
                AppSettings.Instance.EncryptedExtensions
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.ToLower().Trim())
            );
            
            BlockedProcessesList = new ObservableCollection<string>(
                AppSettings.Instance.BlockedProcesses
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(proc => proc.ToLower().Trim())
            );

            CreateJobCommand = new RelayCommand(param => CreateJob());
            DeleteJobCommand = new RelayCommand(param => DeleteJob(), param => SelectedJob != null);
            ExecuteJobCommand = new RelayCommand(param => ExecuteJob(), param => SelectedJob != null || SelectedJobsList.Count > 0);
            AddExtensionCommand = new RelayCommand(param => AddExtension());
            RemoveExtensionCommand = new RelayCommand(param => RemoveExtension(param as string), param => param is string);
            AddProcessCommand = new RelayCommand(param => AddProcess());
            RemoveProcessCommand = new RelayCommand(param => RemoveProcess(param as string), param => param is string);
            BrowseSourceCommand = new RelayCommand(param => BrowseSource());
            BrowseTargetCommand = new RelayCommand(param => BrowseTarget());

            StatusMessage = ResourceSettings.GetString("StatusReady");
        }

        private void BrowseSource()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                SourcePath = dialog.FolderName;
            }
        }

        private void BrowseTarget()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                TargetPath = dialog.FolderName;
            }
        }

        private void UpdateLogger(bool isStartup)
        {
            _logger = LoggerCrea.CreateLogger(AppSettings.Instance.LogFormat, AppSettings.Instance.LogDirectory);

            if (!isStartup)
            {
                StatusMessage = $"Logger : {AppSettings.Instance.LogFormat.ToUpper()} active.";
            }
        }

        private void LoadJobs()
        {
            if (File.Exists(_jobsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_jobsFilePath);
                    var jobs = JsonSerializer.Deserialize<ObservableCollection<BackupJob>>(json);
                    if (jobs != null)
                    {
                        foreach (var job in jobs)
                        {
                            job.Progress = 0;
                            job.State = BackupState.Inactive;
                            BackupJobs.Add(job);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Erreur chargement : {ex.Message}";
                }
            }
        }

        private void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(BackupJobs, options);
            File.WriteAllText(_jobsFilePath, json);
        }

        private void AddExtension()
        {
            if (!string.IsNullOrWhiteSpace(NewExtensionInput))
            {
                string newExt = NewExtensionInput.ToLower().Trim();
                if (!newExt.StartsWith(".")) newExt = "." + newExt;

                if (!EncryptedExtensionsList.Contains(newExt))
                {
                    EncryptedExtensionsList.Add(newExt);
                    SaveEncryptedExtensions();
                    NewExtensionInput = "";
                }
            }
        }

        private void RemoveExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension))
            {
                EncryptedExtensionsList.Remove(extension);
                SaveEncryptedExtensions();
            }
        }

        private void SaveEncryptedExtensions()
        {
            AppSettings.Instance.EncryptedExtensions = string.Join(", ", EncryptedExtensionsList);
        }
        
        private void AddProcess()
        {
            if (!string.IsNullOrWhiteSpace(NewProcessInput))
            {
                string newProc = NewProcessInput.ToLower().Trim();
                if (newProc.EndsWith(".exe")) newProc = newProc[..^4];

                if (!BlockedProcessesList.Contains(newProc))
                {
                    BlockedProcessesList.Add(newProc);
                    SaveBlockedProcesses();
                    NewProcessInput = "";
                }
            }
        }

        private void RemoveProcess(string process)
        {
            if (!string.IsNullOrWhiteSpace(process))
            {
                BlockedProcessesList.Remove(process);
                SaveBlockedProcesses();
            }
        }

        private void SaveBlockedProcesses()
        {
            AppSettings.Instance.BlockedProcesses = string.Join(", ", BlockedProcessesList);
        }

        private void CreateJob()
        {
            if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(TargetPath))
            {
                StatusMessage = ResourceSettings.GetString("EmptyFields");
                return;
            }

            var newJob = new BackupJob(JobName, SourcePath, TargetPath, SelectedType);
            BackupJobs.Add(newJob);
            SaveJobs();

            StatusMessage = $"{JobName} {ResourceSettings.GetString("JobCreated")}";
            JobName = ""; SourcePath = ""; TargetPath = "";
        }

        private void DeleteJob()
        {
            if (SelectedJob != null)
            {
                BackupJobs.Remove(SelectedJob);
                SaveJobs();
                StatusMessage = ResourceSettings.GetString("JobDeleted");
            }
        }

        private async void ExecuteJob()
        {
            var jobsToRun = new List<BackupJob>();

            if (SelectedJobsList.Count > 0)
            {
                jobsToRun.AddRange(SelectedJobsList);
            }
            else if (SelectedJob != null)
            {
                jobsToRun.Add(SelectedJob);
            }

            if (jobsToRun.Count == 0) return;

            StatusMessage = string.Format(ResourceSettings.GetString("ExecutingJobs"), jobsToRun.Count);
            ProgressValue = 0;
            foreach (var job in jobsToRun)
            {
                EventHandler<BackupProgressEventArgs> progressHandler = (sender, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressValue = args.Percentage;
                        StatusMessage = $"{job.Name}: {args.Percentage}%";

                        job.Progress = args.Percentage;

                        var stateLog = new StateLog
                        {
                            BackupName = job.Name,
                            Timestamp = DateTime.Now,
                            State = "ACTIVE",
                            TotalFilesToCopy = args.TotalFiles,
                            TotalFilesSize = args.TotalSize,
                            Progression = args.Percentage,
                            NbFilesLeftToDo = args.TotalFiles - args.FilesProcessed,
                            NbFilesSizeLeftToDo = args.TotalSize - args.SizeProcessed,
                            SourceFilePath = args.CurrentSourcePath,
                            TargetFilePath = args.CurrentTargetPath
                        };
                        StateSettings.UpdateState(stateLog);
                    });
                };

                EventHandler<(string source, string target, long size, float time, float encryptionTime)> fileCopiedHandler = (sender, data) =>
                {
                    var logEntry = new Log.Models.LogEntry
                    {
                        Name = job.Name,
                        SourceFile = data.source,
                        TargetFile = data.target,
                        FileSize = data.size,
                        TransferTime = data.time,
                        EncryptionTime = data.encryptionTime,
                    };
                    _logger.WriteLog(logEntry);
                };

                job.OnProgressUpdate += progressHandler;
                job.OnFileCopied += fileCopiedHandler;

                await Task.Run(() =>
                {
                    try
                    {
                        job.Execute();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"{job.Name} {ResourceSettings.GetString("JobFinished")}";
                            ProgressValue = 100;

                            var finalState = new StateLog
                            {
                                BackupName = job.Name,
                                Timestamp = DateTime.Now,
                                State = "NON ACTIVE",
                                Progression = 100
                            };
                            StateSettings.UpdateState(finalState);
                        });
                    }
                    catch (BlockedProcessException bpex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            job.State = BackupState.Paused;
                            string message = string.Format(ResourceSettings.GetString("ProcessBlockedMessage"), bpex.ProcessName);
                            StatusMessage = $"{ResourceSettings.GetString("Error")} : {message}";
                            MessageBox.Show(
                                message, 
                                ResourceSettings.GetString("Error"), 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning
                            );
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"{ResourceSettings.GetString("Error")} : {ex.Message}";
                            job.State = BackupState.Error;
                        });
                    }
                    finally
                    {
                        job.OnProgressUpdate -= progressHandler;
                        job.OnFileCopied -= fileCopiedHandler;
                    }
                });
            }
        }


        public class LanguageProxy
        {
            public string this[string key]
            {
                get => ResourceSettings.GetString(key);
            }
        }
    }
}