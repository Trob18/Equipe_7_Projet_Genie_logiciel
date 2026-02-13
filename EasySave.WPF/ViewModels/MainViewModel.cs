using EasySave.Log;
using EasySave.Log.Interfaces;
using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using EasySave.WPF.Models;
using EasySave.WPF.State;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;               // (Select)
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            set
            {
                // ✅ si on change de job, on désabonne les handlers de l'ancien
                DetachJobHandlers(_selectedJob);

                _selectedJob = value;
                OnPropertyChanged();

                // ✅ et on ré-abonne sur le nouveau (optionnel : on peut abonner seulement au moment Run)
                // AttachJobHandlers(_selectedJob);

                CommandManager.InvalidateRequerySuggested();
            }
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

        public ObservableCollection<string> EncryptedExtensionsList { get; set; }

        private string _newExtensionInput;
        public string NewExtensionInput { get => _newExtensionInput; set { _newExtensionInput = value; OnPropertyChanged(); } }

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

                    RestartWarningVisibility = (value == _startupLanguage)
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }
        }

        private readonly string _jobsFilePath;
        private ILogger _logger;

        // état d’exécution (évite multi-run)
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // préparation Stop
        private CancellationTokenSource? _cts;

        // handlers stockés (pour pouvoir unsubscribe)
        private EventHandler<BackupProgressEventArgs>? _progressHandler;
        private EventHandler<(string source, string target, long size, float time)>? _fileCopiedHandler;

        public ICommand CreateJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ExecuteJobCommand { get; }
        public ICommand StopJobCommand { get; }   // (stop)
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

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

            CreateJobCommand = new RelayCommand(_ => CreateJob(), _ => !IsRunning);
            DeleteJobCommand = new RelayCommand(_ => DeleteJob(), _ => SelectedJob != null && !IsRunning);

            // async sans nouvelle classe : on utilise un async lambda (Action => async void OK)
            ExecuteJobCommand = new RelayCommand(async _ => await ExecuteJobAsync(), _ => SelectedJob != null && !IsRunning);

            // Stop “request”
            StopJobCommand = new RelayCommand(_ => RequestStop(), _ => IsRunning);

            AddExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !IsRunning);
            RemoveExtensionCommand = new RelayCommand(param => RemoveExtension(param as string), param => param is string && !IsRunning);

            StatusMessage = ResourceSettings.GetString("StatusReady");
        }

        private void UpdateLogger(bool isStartup)
        {
            _logger = LoggerCrea.CreateLogger(AppSettings.Instance.LogFormat, AppSettings.Instance.LogDirectory);

            if (!isStartup)
                StatusMessage = $"Logger : {AppSettings.Instance.LogFormat.ToUpper()} active.";
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
                        foreach (var job in jobs) BackupJobs.Add(job);
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
                DetachJobHandlers(SelectedJob); // ✅ clean
                BackupJobs.Remove(SelectedJob);
                SaveJobs();
                StatusMessage = "Travail supprimé.";
            }
        }

        // ASYNC PROPRE (pas de freeze + pas de double-events)
        private async Task ExecuteJobAsync()
        {
            if (SelectedJob == null) return;
            if (IsRunning) return;

            IsRunning = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var job = SelectedJob; // snapshot (évite surprises si sélection change)

            StatusMessage = $"Exécution : {job.Name}...";
            ProgressValue = 0;

            // on s’assure de ne pas empiler les handlers à chaque Run
            DetachJobHandlers(job);
            AttachJobHandlers(job);

            try
            {
                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();

                    // Stop réel nécessite un Execute(CancellationToken) dans BackupJob
                    job.Execute();

                }, token);

                StatusMessage = $"{job.Name} terminé !";
                ProgressValue = 100;

                StateSettings.UpdateState(new StateLog
                {
                    BackupName = job.Name,
                    Timestamp = DateTime.Now,
                    State = "NON ACTIVE",
                    Progression = 100,
                    SourceFilePath = "Terminé",
                    TargetFilePath = ""
                });
            }
            catch (OperationCanceledException)
            {
                StatusMessage = $"Arrêt demandé : {job.Name}";
                StateSettings.UpdateState(new StateLog
                {
                    BackupName = job.Name,
                    Timestamp = DateTime.Now,
                    State = "STOP_REQUESTED",
                    Progression = ProgressValue
                });
            }
            finally
            {
                // évite fuites/duplications
                DetachJobHandlers(job);

                IsRunning = false;
            }
        }

        private void RequestStop()
        {
            // stop "request" (sera stop réel quand BackupJob prendra un token)
            _cts?.Cancel();
        }

        private void AttachJobHandlers(BackupJob job)
        {
            if (job == null) return;

            _progressHandler = (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressValue = args.Percentage;
                    StatusMessage = $"{args.CurrentFileName} ({args.Percentage}%)";

                    StateSettings.UpdateState(new StateLog
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
                    });
                });
            };

            _fileCopiedHandler = (sender, data) =>
            {
                var logEntry = new EasySave.Log.Models.LogEntry
                {
                    Name = job.Name,
                    SourceFile = data.source,
                    TargetFile = data.target,
                    FileSize = data.size,
                    TransferTime = data.time,
                };
                _logger.WriteLog(logEntry);
            };

            job.OnProgressUpdate += _progressHandler;
            job.OnFileCopied += _fileCopiedHandler;
        }

        private void DetachJobHandlers(BackupJob job)
        {
            if (job == null) return;

            if (_progressHandler != null)
                job.OnProgressUpdate -= _progressHandler;

            if (_fileCopiedHandler != null)
                job.OnFileCopied -= _fileCopiedHandler;

            _progressHandler = null;
            _fileCopiedHandler = null;
        }

        public class LanguageProxy
        {
            public string this[string key] => ResourceSettings.GetString(key);
        }
    }
}