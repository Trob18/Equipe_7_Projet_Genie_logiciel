using EasySave.WPF.ViewModels;
using EasySave.WPF.Views;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace EasySave.WPF
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        const int SW_HIDE = 0;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            int executeIndex = Array.FindIndex(e.Args, a => a.Equals("-e", StringComparison.OrdinalIgnoreCase) || a.Equals("execute", StringComparison.OrdinalIgnoreCase));

            if (executeIndex >= 0 && executeIndex + 1 < e.Args.Length)
            {
                if (!AttachConsole(-1)) AllocConsole();

                string jobIdentifier = e.Args[executeIndex + 1];
                var viewModel = new MainViewModel();
                var jobToRun = viewModel.BackupJobs.FirstOrDefault(j => j.Name.Equals(jobIdentifier, StringComparison.OrdinalIgnoreCase));

                if (jobToRun == null && int.TryParse(jobIdentifier, out int index) && index >= 0 && index < viewModel.BackupJobs.Count)
                {
                    jobToRun = viewModel.BackupJobs[index];
                }

                if (jobToRun != null)
                {
                    Console.WriteLine($"\n[EasySave] Lancement automatique du travail : '{jobToRun.Name}'...");
                    jobToRun.Execute();
                    Console.WriteLine($"[EasySave] Travail '{jobToRun.Name}' terminé avec succès.\n");
                }
                else
                {
                    Console.WriteLine($"\n[EasySave] ERREUR : Travail '{jobIdentifier}' introuvable.\n");
                }

                Environment.Exit(0);
                return;
            }

            if (e.Args.Contains("console", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("-c"))
            {
                if (!AttachConsole(-1)) AllocConsole();

                var consoleView = new ConsoleView();
                consoleView.Start();

                Environment.Exit(0);
                return;
            }
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Environment.Exit(0);
        }
    }
}