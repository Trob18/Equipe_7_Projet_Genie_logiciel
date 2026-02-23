using EasySave.WPF.Config;
using EasySave.WPF.Enumerations;
using EasySave.WPF.ViewModels;
using System;

namespace EasySave.WPF.Views
{
    public class ConsoleView
    {
        private MainViewModel _viewModel;

        public ConsoleView()
        {
            _viewModel = new MainViewModel();
        }

        public void Start()
        {
            bool isRunning = true;
            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("=====================================");
                Console.WriteLine($"      {ResourceSettings.GetString("ConsoleMenuTitle")}      ");
                Console.WriteLine("=====================================");
                Console.WriteLine(ResourceSettings.GetString("ConsoleMenuShow"));
                Console.WriteLine(ResourceSettings.GetString("ConsoleMenuExec"));
                Console.WriteLine(ResourceSettings.GetString("ConsoleMenuCreate"));
                Console.WriteLine(ResourceSettings.GetString("ConsoleMenuSet"));
                Console.WriteLine(ResourceSettings.GetString("ConsoleMenuQuit"));
                Console.WriteLine("=====================================");
                Console.Write(ResourceSettings.GetString("ConsoleChoice"));

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowJobs();
                        break;
                    case "2":
                        ExecuteJob();
                        break;
                    case "3":
                        CreateJob();
                        break;
                    case "4":
                        SettingsMenu();
                        break;
                    case "5":
                        isRunning = false;
                        break;
                }
            }
        }
        private void CreateJob()
        {
            Console.Clear();
            Console.WriteLine("=====================================");
            Console.WriteLine($"        {ResourceSettings.GetString("ConsoleCreateTitle")}        ");
            Console.WriteLine("=====================================");

            Console.Write(ResourceSettings.GetString("ConsoleAddName"));
            _viewModel.JobName = Console.ReadLine();

            Console.Write(ResourceSettings.GetString("ConsoleAddSource"));
            _viewModel.SourcePath = Console.ReadLine();

            Console.Write(ResourceSettings.GetString("ConsoleAddTarget"));
            _viewModel.TargetPath = Console.ReadLine();

            Console.Write(ResourceSettings.GetString("ConsoleAddType"));
            string typeInput = Console.ReadLine();
            if (int.TryParse(typeInput, out int type) && (type == 0 || type == 1))
            {
                _viewModel.SelectedType = (BackupType)type;
            }
            else
            {
                _viewModel.SelectedType = BackupType.Full;
            }

            _viewModel.CreateJobCommand.Execute(null);

            Console.WriteLine($"\n-> {_viewModel.StatusMessage}");

            Console.WriteLine(ResourceSettings.GetString("ConsolePressEnter"));
            Console.ReadLine();
        }
        private void ShowJobs()
        {
            Console.Clear();
            Console.WriteLine(ResourceSettings.GetString("ConsoleJobListTitle"));
            if (_viewModel.BackupJobs.Count == 0) Console.WriteLine(ResourceSettings.GetString("ConsoleNoJobs"));

            for (int i = 0; i < _viewModel.BackupJobs.Count; i++)
            {
                var job = _viewModel.BackupJobs[i];
                Console.WriteLine($"[{i}] {job.Name} | {job.Type} | {job.SourceDirectory} -> {job.TargetDirectory}");
            }
            Console.WriteLine(ResourceSettings.GetString("ConsolePressEnter"));
            Console.ReadLine();
        }

        private void ExecuteJob()
        {
            ShowJobs();
            Console.Write(ResourceSettings.GetString("ConsoleEnterJobNumber"));

            string input = Console.ReadLine();
            if (int.TryParse(input, out int index) && index >= 0 && index < _viewModel.BackupJobs.Count)
            {
                var jobToRun = _viewModel.BackupJobs[index];
                Console.WriteLine(string.Format(ResourceSettings.GetString("ConsoleLaunchingJob"), jobToRun.Name));

                jobToRun.Execute();

                Console.WriteLine(ResourceSettings.GetString("ConsoleJobFinished"));
                Console.ReadLine();
            }
        }

        private void SettingsMenu()
        {
            bool inSettings = true;
            while (inSettings)
            {
                Console.Clear();
                Console.WriteLine("=====================================");
                Console.WriteLine($"             {ResourceSettings.GetString("ConsoleSettingsTitle")}              ");
                Console.WriteLine("=====================================");
                Console.WriteLine($"{ResourceSettings.GetString("ConsoleSettingsLang")}{AppSettings.Instance.Language}");
                Console.WriteLine($"{ResourceSettings.GetString("ConsoleSettingsLogFormat")}{AppSettings.Instance.LogFormat}");
                Console.WriteLine($"{ResourceSettings.GetString("ConsoleSettingsIP")}{AppSettings.Instance.LogServerIP}");
                Console.WriteLine($"{ResourceSettings.GetString("ConsoleSettingsEncryptAll")}{(AppSettings.Instance.EncryptAll ? "OUI/YES" : "NON/NO")}");
                Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsBack"));
                Console.WriteLine("=====================================");
                Console.Write(ResourceSettings.GetString("ConsoleSettingsModify"));

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write(ResourceSettings.GetString("ConsoleSettingsChooseLang"));
                        if (int.TryParse(Console.ReadLine(), out int lang) && (lang == 0 || lang == 1))
                        {
                            _viewModel.SelectedLanguage = (Language)lang;
                            Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsLangChanged"));
                        }
                        break;
                    case "2":
                        Console.Write(ResourceSettings.GetString("ConsoleSettingsChooseFormat"));
                        string format = Console.ReadLine()?.ToLower().Trim();
                        if (_viewModel.LogFormats.Contains(format))
                        {
                            _viewModel.SelectedLogFormat = format;
                            Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsFormatChanged"));
                        }
                        break;
                    case "3":
                        Console.Write(ResourceSettings.GetString("ConsoleSettingsChooseIP"));
                        _viewModel.LogServerIP = Console.ReadLine()?.Trim();
                        Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsIPChanged"));
                        break;
                    case "4":
                        _viewModel.EncryptAll = !_viewModel.EncryptAll;
                        Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsEncryptToggled"));
                        break;
                    case "5":
                        inSettings = false;
                        continue;
                }

                if (inSettings)
                {
                    Console.WriteLine(ResourceSettings.GetString("ConsoleSettingsContinue"));
                    Console.ReadLine();
                }
            }
        }
    }
}