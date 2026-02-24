using EasySave.Log;
using EasySave.Log.Interfaces;
using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using EasySave.WPF.Models;
using EasySave.WPF.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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
            set { _selectedJob = value; OnPropertyChanged(); }
        }

        private List<BackupJob> _selectedJobsList = new List<BackupJob>();
        public List<BackupJob> SelectedJobsList
        {
            get => _selectedJobsList;
            set { _selectedJobsList = value; OnPropertyChanged(); }
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

        // --- DEBUT : VARIABLES POUR LES EXTENSIONS PRIORITAIRES ---
        public ObservableCollection<string> PriorityExtensionsList { get; set; }

        private string _newPriorityExtensionInput;
        public string NewPriorityExtensionInput
        {
            get => _newPriorityExtensionInput;
            set { _newPriorityExtensionInput = value; OnPropertyChanged(); }
        }
        // --- FIN ---

        public ObservableCollection<string> BlockedProcessesList { get; set; }
        private string _newProcessInput;
        public string NewProcessInput { get => _newProcessInput; set { _newProcessInput = value; OnPropertyChanged(); } }

        private string _settingsSearchText;
        public string SettingsSearchText
        {
            get => _settingsSearchText;
            set { _settingsSearchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredExtensions)); OnPropertyChanged(nameof(FilteredProcesses)); }
        }

        private string _extensionsSearchText;
        public string ExtensionsSearchText
        {
            get => _extensionsSearchText;
            set { _extensionsSearchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredExtensions)); }
        }

        private string _processesSearchText;
        public string ProcessesSearchText
        {
            get => _processesSearchText;
            set { _processesSearchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredProcesses)); }
        }

        public IEnumerable<string> FilteredExtensions
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ExtensionsSearchText)) return EncryptedExtensionsList;
                return EncryptedExtensionsList.Where(e => e.Contains(ExtensionsSearchText.ToLower()));
            }
        }

        public IEnumerable<string> FilteredProcesses
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProcessesSearchText)) return BlockedProcessesList;
                return BlockedProcessesList.Where(p => p.Contains(ProcessesSearchText.ToLower()));
            }
        }

        private string _prioritySearchText;
        public string PrioritySearchText
        {
            get => _prioritySearchText;
            set { _prioritySearchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredPriorityExtensions)); }
        }

        public IEnumerable<string> FilteredPriorityExtensions
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PrioritySearchText)) return PriorityExtensionsList;
                return PriorityExtensionsList.Where(e => e.Contains(PrioritySearchText.ToLower()));
            }
        }

        public bool EncryptAll
        {
            get => AppSettings.Instance.EncryptAll;
            set
            {
                if (AppSettings.Instance.EncryptAll != value)
                {
                    AppSettings.Instance.EncryptAll = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredExtensions));
                }
            }
        }

        private bool _isCreateJobVisible;
        public bool IsCreateJobVisible
        {
            get => _isCreateJobVisible;
            set { _isCreateJobVisible = value; OnPropertyChanged(); }
        }

        private bool _isEditJobVisible;
        public bool IsEditJobVisible
        {
            get => _isEditJobVisible;
            set { _isEditJobVisible = value; OnPropertyChanged(); }
        }

        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
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

        public ObservableCollection<string> LogFormats { get; } = new ObservableCollection<string>
        {
            "json",
            "xml",
            "network",
            "json+network",
            "xml+network"
        };

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
                    OnPropertyChanged(nameof(Labels));
                    StatusMessage = ResourceSettings.GetString("StatusReady");

                    foreach (var job in BackupJobs)
                    {
                        job.OnPropertyChanged(nameof(job.TranslatedType));
                        job.OnPropertyChanged(nameof(job.ProgressText));
                    }
                }
            }
        }

        public string LogServerIP
        {
            get => AppSettings.Instance.LogServerIP;
            set
            {
                if (AppSettings.Instance.LogServerIP != value)
                {
                    AppSettings.Instance.LogServerIP = value;
                    OnPropertyChanged();
                    UpdateLogger(false);
                }
            }
        }

        public long MaxLargeFileSizeMO
        {
            get => AppSettings.Instance.MaxLargeFileSizeMO;
            set
            {
                if (AppSettings.Instance.MaxLargeFileSizeMO != value)
                {
                    AppSettings.Instance.MaxLargeFileSizeMO = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly string _jobsFilePath;
        private ILogger _logger;

        public ICommand CreateJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ExecuteJobCommand { get; }
        public ICommand ExecuteOrResumeJobCommand { get; }
        public ICommand PauseJobCommand { get; }
        public ICommand ResumeJobCommand { get; }
        public ICommand StopJobCommand { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }
        public ICommand AddProcessCommand { get; }
        public ICommand RemoveProcessCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseTargetCommand { get; }
        public ICommand OpenCreateJobCommand { get; }
        public ICommand CloseCreateJobCommand { get; }
        public ICommand OpenEditJobCommand { get; }
        public ICommand UpdateJobCommand { get; }
        public ICommand CloseEditJobCommand { get; }

        // --- COMMANDES POUR LES EXTENSIONS PRIORITAIRES ---
        public ICommand AddPriorityExtensionCommand { get; }
        public ICommand RemovePriorityExtensionCommand { get; }
        public ICommand MovePriorityExtensionUpCommand { get; }
        public ICommand MovePriorityExtensionDownCommand { get; }
        // --------------------------------------------------

        public string this[string key] => ResourceSettings.GetString(key);

        public MainViewModel()
        {
            _startupLanguage = AppSettings.Instance.Language;
            RestartWarningVisibility = Visibility.Collapsed;
            SelectedType = BackupType.Full;
            IsCreateJobVisible = false;

            _jobsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");

            UpdateLogger(true);

            BackupJobs = new ObservableCollection<BackupJob>();
            LoadJobs();

            EncryptedExtensionsList = new ObservableCollection<string>(
                (AppSettings.Instance.EncryptedExtensions ?? "")
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.ToLower().Trim())
            );

            BlockedProcessesList = new ObservableCollection<string>(
                (AppSettings.Instance.BlockedProcesses ?? "")
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(proc => proc.ToLower().Trim())
            );

            // --- INITIALISATION LISTE EXTENSIONS PRIORITAIRES ---
            PriorityExtensionsList = new ObservableCollection<string>(
                (AppSettings.Instance.PriorityExtensions ?? "")
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.ToLower().Trim())
            );

            CreateJobCommand = new RelayCommand(param => CreateJob());
            DeleteJobCommand = new RelayCommand(param => DeleteJob(), param => SelectedJob != null);
            ExecuteJobCommand = new RelayCommand(param => ExecuteJob(), param => (SelectedJob != null || SelectedJobsList.Count > 0));
            ExecuteOrResumeJobCommand = new RelayCommand(param => ExecuteOrResumeJob(), param => CanExecuteOrResumeJob());
            PauseJobCommand = new RelayCommand(param => PauseJob(), param => CanPauseJob());
            ResumeJobCommand = new RelayCommand(param => ResumeJob(), param => CanResumeJob());
            StopJobCommand = new RelayCommand(param => StopJob(), param => CanStopJob());
            AddExtensionCommand = new RelayCommand(param => AddExtension());
            RemoveExtensionCommand = new RelayCommand(param => RemoveExtension(param as string), param => param is string);

            AddProcessCommand = new RelayCommand(param => AddProcess());
            RemoveProcessCommand = new RelayCommand(param => RemoveProcess(param as string), param => param is string);

            BrowseSourceCommand = new RelayCommand(param => BrowseSource());
            BrowseTargetCommand = new RelayCommand(param => BrowseTarget());
            OpenCreateJobCommand = new RelayCommand(param => IsCreateJobVisible = true);
            CloseCreateJobCommand = new RelayCommand(param => IsCreateJobVisible = false);
            OpenEditJobCommand = new RelayCommand(param => OpenEditJob(), param => SelectedJobsList != null && SelectedJobsList.Count == 1);
            UpdateJobCommand = new RelayCommand(param => UpdateJob());
            CloseEditJobCommand = new RelayCommand(param => IsEditJobVisible = false);

            // --- INITIALISATION COMMANDES EXTENSIONS PRIORITAIRES ---
            AddPriorityExtensionCommand = new RelayCommand(param => AddPriorityExtension());
            RemovePriorityExtensionCommand = new RelayCommand(param => RemovePriorityExtension(param as string), param => param is string);
            MovePriorityExtensionUpCommand = new RelayCommand(param => MovePriorityExtensionUp(param as string), param => param is string);
            MovePriorityExtensionDownCommand = new RelayCommand(param => MovePriorityExtensionDown(param as string), param => param is string);

            StatusMessage = ResourceSettings.GetString("StatusReady");
        }

        private bool CanPauseJob() => (SelectedJobsList.Any(j => j.State == BackupState.Active) || (SelectedJob?.State == BackupState.Active));
        private bool CanResumeJob() => (SelectedJobsList.Any(j => j.State == BackupState.Paused) || (SelectedJob?.State == BackupState.Paused));
        private bool CanStopJob() => (SelectedJobsList.Any(j => j.State == BackupState.Active || j.State == BackupState.Paused) || (SelectedJob != null && (SelectedJob.State == BackupState.Active || SelectedJob.State == BackupState.Paused)));
        private bool CanExecuteOrResumeJob()
        {
            var jobs = SelectedJobsList.Count > 0 ? SelectedJobsList : (SelectedJob != null ? new List<BackupJob> { SelectedJob } : new List<BackupJob>());
            return jobs.Any(j => j.State == BackupState.Inactive || j.State == BackupState.Paused || j.State == BackupState.Error);
        }

        private void ExecuteOrResumeJob()
        {
            var jobs = SelectedJobsList.Count > 0 ? SelectedJobsList : (SelectedJob != null ? new List<BackupJob> { SelectedJob } : new List<BackupJob>());
            
            var jobsToExecute = jobs.Where(j => j.State == BackupState.Inactive || j.State == BackupState.Error).ToList();
            var jobsToResume = jobs.Where(j => j.State == BackupState.Paused).ToList();

            if (jobsToResume.Count > 0)
            {
                foreach (var job in jobsToResume) job.Resume();
                StatusMessage = ResourceSettings.GetString("JobsResumed");
            }

            if (jobsToExecute.Count > 0)
            {
                // We reuse ExecuteJob but we need to pass the specific list
                // For simplicity, if we have a mix, we might want to handle it better, 
                // but usually users execute OR resume.
                ExecuteJob();
            }
        }

        private void PauseJob()
        {
            if (SelectedJobsList.Count > 0) foreach (var job in SelectedJobsList) job.Pause();
            else SelectedJob?.Pause();
            StatusMessage = ResourceSettings.GetString("JobsPaused") ?? "Travaux mis en pause";
            CommandManager.InvalidateRequerySuggested();
        }

        private void ResumeJob()
        {
            if (SelectedJobsList.Count > 0) foreach (var job in SelectedJobsList) job.Resume();
            else SelectedJob?.Resume();
            StatusMessage = ResourceSettings.GetString("JobsResumed") ?? "Travaux repris";
            CommandManager.InvalidateRequerySuggested();
        }

        private void StopJob()
        {
            if (SelectedJobsList.Count > 0) foreach (var job in SelectedJobsList) job.Stop();
            else SelectedJob?.Stop();
            StatusMessage = ResourceSettings.GetString("JobsStopped") ?? "Travaux arrêtés";
            CommandManager.InvalidateRequerySuggested();
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
            _logger = LoggerCrea.CreateLogger(AppSettings.Instance.LogFormat, AppSettings.Instance.LogDirectory, AppSettings.Instance.LogServerIP);

            if (!isStartup)
            {
                StatusMessage = $"Logger : {AppSettings.Instance.LogFormat.ToUpper()} actif sur {AppSettings.Instance.LogServerIP}.";
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
                            job.State = BackupState.Inactive;
                            job.InitializeJobData();
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
                var extensions = NewExtensionInput.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ext in extensions)
                {
                    string cleanExt = ext.ToLower().Trim();
                    if (!cleanExt.StartsWith(".")) cleanExt = "." + cleanExt;

                    if (!EncryptedExtensionsList.Contains(cleanExt))
                    {
                        EncryptedExtensionsList.Add(cleanExt);
                    }
                }
                SaveEncryptedExtensions();
                NewExtensionInput = "";
                OnPropertyChanged(nameof(FilteredExtensions));
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

        // --- METHODES POUR LES EXTENSIONS PRIORITAIRES ---
        private void AddPriorityExtension()
        {
            if (!string.IsNullOrWhiteSpace(NewPriorityExtensionInput))
            {
                string newExt = NewPriorityExtensionInput.ToLower().Trim();
                if (!newExt.StartsWith(".")) newExt = "." + newExt;

                if (!PriorityExtensionsList.Contains(newExt))
                {
                    PriorityExtensionsList.Add(newExt);
                    SavePriorityExtensions();
                    NewPriorityExtensionInput = "";
                }
            }
        }

        private void RemovePriorityExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension))
            {
                PriorityExtensionsList.Remove(extension);
                SavePriorityExtensions();
                OnPropertyChanged(nameof(FilteredPriorityExtensions));
            }
        }

        private void MovePriorityExtensionUp(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return;
            int index = PriorityExtensionsList.IndexOf(extension);
            if (index > 0)
            {
                PriorityExtensionsList.Move(index, index - 1);
                SavePriorityExtensions();
                OnPropertyChanged(nameof(FilteredPriorityExtensions));
            }
        }

        private void MovePriorityExtensionDown(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return;
            int index = PriorityExtensionsList.IndexOf(extension);
            if (index >= 0 && index < PriorityExtensionsList.Count - 1)
            {
                PriorityExtensionsList.Move(index, index + 1);
                SavePriorityExtensions();
                OnPropertyChanged(nameof(FilteredPriorityExtensions));
            }
        }

        private void SavePriorityExtensions()
        {
            AppSettings.Instance.PriorityExtensions = string.Join(", ", PriorityExtensionsList);
        }
        // --------------------------------------------------

        private void AddProcess()
        {
            if (!string.IsNullOrWhiteSpace(NewProcessInput))
            {
                var processes = NewProcessInput.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var proc in processes)
                {
                    string cleanProc = proc.ToLower().Trim();
                    if (cleanProc.EndsWith(".exe")) cleanProc = cleanProc[..^4];

                    if (!BlockedProcessesList.Contains(cleanProc))
                    {
                        BlockedProcessesList.Add(cleanProc);
                    }
                }
                SaveBlockedProcesses();
                NewProcessInput = "";
                OnPropertyChanged(nameof(FilteredProcesses));
            }
        }

        private void RemoveProcess(string process)
        {
            if (!string.IsNullOrWhiteSpace(process))
            {
                BlockedProcessesList.Remove(process);
                SaveBlockedProcesses();
                OnPropertyChanged(nameof(FilteredProcesses));
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
            newJob.InitializeJobData();
            BackupJobs.Add(newJob);
            SaveJobs();

            StatusMessage = $"{JobName} {ResourceSettings.GetString("JobCreated")}";
            JobName = ""; SourcePath = ""; TargetPath = "";
            IsCreateJobVisible = false;
        }

        private void OpenEditJob()
        {
            if (SelectedJob == null) return;

            JobName = SelectedJob.Name;
            SourcePath = SelectedJob.SourceDirectory;
            TargetPath = SelectedJob.TargetDirectory;
            SelectedType = SelectedJob.Type;
            IsEditJobVisible = true;
        }

        private void UpdateJob()
        {
            if (SelectedJob == null) return;

            if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(TargetPath))
            {
                StatusMessage = ResourceSettings.GetString("EmptyFields");
                return;
            }

            SelectedJob.Name = JobName;
            SelectedJob.SourceDirectory = SourcePath;
            SelectedJob.TargetDirectory = TargetPath;
            SelectedJob.Type = SelectedType;

            SelectedJob.OnPropertyChanged(nameof(SelectedJob.Name));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.SourceDirectory));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.TargetDirectory));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.Type));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.TranslatedType));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.ShortSourceDirectory));
            SelectedJob.OnPropertyChanged(nameof(SelectedJob.ShortTargetDirectory));

            SelectedJob.InitializeJobData();
            SaveJobs();

            StatusMessage = $"{JobName} mis à jour.";
            JobName = ""; SourcePath = ""; TargetPath = "";
            IsEditJobVisible = false;
        }

        private void DeleteJob()
        {
            var jobsToDelete = new List<BackupJob>();
            if (SelectedJobsList.Count > 0) jobsToDelete.AddRange(SelectedJobsList);
            else if (SelectedJob != null) jobsToDelete.Add(SelectedJob);

            if (jobsToDelete.Count == 0) return;

            string message;
            if (jobsToDelete.Count == 1)
            {
                message = string.Format(ResourceSettings.GetString("ConfirmDeleteSingle"), jobsToDelete[0].Name);
            }
            else
            {
                message = string.Format(ResourceSettings.GetString("ConfirmDeleteMultiple"), jobsToDelete.Count);
            }

            var result = MessageBox.Show(
                message,
                ResourceSettings.GetString("Delete"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                foreach (var job in jobsToDelete)
                {
                    BackupJobs.Remove(job);
                }
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
            CommandManager.InvalidateRequerySuggested();

            var tasks = new List<Task>();

            foreach (var job in jobsToRun)
            {
                EventHandler<BackupProgressEventArgs> progressHandler = (sender, args) =>
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
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
                    Task.Run(() =>
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
                    });
                };

                EventHandler<string> blockedProcessHandler = (sender, processName) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string message = string.Format(ResourceSettings.GetString("ProcessBlockedMessage"), processName);
                        StatusMessage = $"{ResourceSettings.GetString("Error")} : {message}";
                        CommandManager.InvalidateRequerySuggested();

                        MessageBox.Show(
                            message,
                            ResourceSettings.GetString("Error"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    });
                };

                job.OnProgressUpdate += progressHandler;
                job.OnFileCopied += fileCopiedHandler;
                job.OnBlockedProcessDetected += blockedProcessHandler;

                var task = Task.Run(() =>
                {
                    try
                    {
                        job.Execute();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            job.Progress = 100;

                            var finalState = new StateLog
                            {
                                BackupName = job.Name,
                                Timestamp = DateTime.Now,
                                State = "NON ACTIVE",
                                Progression = 100,
                                SourceFilePath = "Terminé",
                                TargetFilePath = ""
                            };
                            StateSettings.UpdateState(finalState);
                            CommandManager.InvalidateRequerySuggested();
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"{ResourceSettings.GetString("Error")} : {ex.Message}";
                            job.State = BackupState.Error;
                            CommandManager.InvalidateRequerySuggested();
                        });
                    }
                    finally
                    {
                        job.OnProgressUpdate -= progressHandler;
                        job.OnFileCopied -= fileCopiedHandler;
                        job.OnBlockedProcessDetected -= blockedProcessHandler;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = ResourceSettings.GetString("AllJobsFinished") ?? "Tous les travaux sont terminés !";
                ProgressValue = 100;
            });

            await Task.Delay(2000);
            Application.Current.Dispatcher.Invoke(() => ProgressValue = 0);
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