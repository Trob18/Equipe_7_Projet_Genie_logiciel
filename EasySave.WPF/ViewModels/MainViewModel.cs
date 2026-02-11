using EasySave.Log;
using EasySave.Log.Interfaces;
using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using EasySave.WPF.Models;
using EasySave.WPF.State;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows; // Pour Visibility
using System.Windows.Input;

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
        public LanguageProxy Labels { get; } = new LanguageProxy();
        private string _jobName;
        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(); } }

        private string _sourcePath;
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(); } }

        private string _targetPath;
        public string TargetPath { get => _targetPath; set { _targetPath = value; OnPropertyChanged(); } }

        private BackupType _selectedType;
        public BackupType SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); } }

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

        public MainViewModel()
        {
            _startupLanguage = AppSettings.Instance.Language;
            RestartWarningVisibility = Visibility.Collapsed;
            SelectedType = BackupType.Full;

            _jobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");

            UpdateLogger(true);

            BackupJobs = new ObservableCollection<BackupJob>();
            LoadJobs();

            CreateJobCommand = new RelayCommand(param => CreateJob());
            DeleteJobCommand = new RelayCommand(param => DeleteJob(), param => SelectedJob != null);
            ExecuteJobCommand = new RelayCommand(param => ExecuteJob(), param => SelectedJob != null);

            StatusMessage = ResourceSettings.GetString("StatusReady");
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
                        foreach (var job in jobs) BackupJobs.Add(job);
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

        private void CreateJob()
        {
            if (BackupJobs.Count >= 5)
            {
                MessageBox.Show("Max 5 jobs !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(TargetPath))
            {
                StatusMessage = "Champs vides !";
                return;
            }

            var newJob = new BackupJob(JobName, SourcePath, TargetPath, SelectedType);
            BackupJobs.Add(newJob);
            SaveJobs();

            StatusMessage = $"{JobName} créé.";
            JobName = ""; SourcePath = ""; TargetPath = "";
        }

        private void DeleteJob()
        {
            if (SelectedJob != null)
            {
                BackupJobs.Remove(SelectedJob);
                SaveJobs();
                StatusMessage = "Travail supprimé.";
            }
        }

        private async void ExecuteJob()
        {
            if (SelectedJob == null) return;

            StatusMessage = $"Exécution : {SelectedJob.Name}...";
            ProgressValue = 0;

            SelectedJob.OnProgressUpdate += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressValue = args.Percentage;
                    StatusMessage = $"{args.CurrentFileName} ({args.Percentage}%)";

                    var stateLog = new StateLog
                    {
                        BackupName = SelectedJob.Name,
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

            SelectedJob.OnFileCopied += (sender, data) =>
            {
                var logEntry = new EasySave.Log.Models.LogEntry
                {
                    Name = SelectedJob.Name,
                    SourceFile = data.source,
                    TargetFile = data.target,
                    FileSize = data.size,
                    TransferTime = data.time,
                };
                _logger.WriteLog(logEntry);
            };

            await Task.Run(() =>
            {
                SelectedJob.Execute();
            });

            StatusMessage = $"{SelectedJob.Name} terminé !";
            ProgressValue = 100;

            var finalState = new StateLog
            {
                BackupName = SelectedJob.Name,
                Timestamp = DateTime.Now,
                State = "NON ACTIVE",
                Progression = 100
            };
            StateSettings.UpdateState(finalState);
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