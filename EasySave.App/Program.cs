using EasySave.App.Controllers;
using EasySave.App.Views;
using EasySave.App.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasySave.App
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupController controller = new BackupController();
            ConsoleView view = new ConsoleView();

            controller.OnGlobalProgress += (jobName, percent) =>
            {
                Console.Write($"\rExecution [{jobName}]: {percent}%   ");
            };

            if (args.Length > 0)
            {
                string command = string.Join("", args);

                List<int> jobsToRun = ParseJobSelection(command, controller.GetJobs().Count);

                if (jobsToRun.Count > 0)
                {
                    Console.WriteLine($"\nLancement de {jobsToRun.Count} travaux détectés via commande...");
                    foreach (int index in jobsToRun)
                    {
                        Console.WriteLine($"\n--- Execution Job {index + 1} ---");
                        controller.ExecuteJob(index);
                    }
                    Console.WriteLine("\n\nTout est terminé !");
                }
                else
                {
                    Console.WriteLine("Aucun travail valide trouvé dans la commande.");
                }

                return;
            }
            bool running = true;
            Console.WriteLine(Config.ResourceSettings.GetString("Welcome"));

            while (running)
            {
                view.DisplayMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        view.DisplayJobs(controller.GetJobs());
                        break;

                    case "2":
                        try
                        {
                            Console.WriteLine("----------------------------------\n");
                            string name = view.AskForInput("EnterName"); Console.WriteLine();
                            string source = view.AskForInput("EnterSource"); Console.WriteLine();
                            string target = view.AskForInput("EnterTarget"); Console.WriteLine();
                            BackupType type = view.AskForBackupType();
                            controller.CreateJob(name, source, target, type);
                            view.DisplaySuccess();
                        }
                        catch (Exception ex) { view.DisplayError(ex.Message); }
                        break;

                    case "3":
                        var jobs = controller.GetJobs();
                        view.DisplayJobs(jobs);

                        Console.WriteLine("Indiquez les numéros à lancer (ex: 1-3;5) ou 'all' :");
                        string input = Console.ReadLine();

                        List<int> selection = ParseJobSelection(input, jobs.Count);

                        foreach (int i in selection)
                        {
                            Console.WriteLine($"\n--- Execution Job {jobs[i].Name} ---");
                            controller.ExecuteJob(i);
                        }

                        if (selection.Count > 0) view.DisplaySuccess();
                        else view.DisplayError("Aucune sélection valide.");
                        break;

                    case "4":
                        view.DisplayJobs(controller.GetJobs());
                        string delInput = view.AskForInput("EnterJobID");
                        if (int.TryParse(delInput, out int delIdx))
                        {
                            controller.DeleteJob(delIdx - 1);
                            Console.WriteLine(Config.ResourceSettings.GetString("JobDeleted"));
                        }
                        break;

                    case "5":
                        Console.WriteLine(Config.ResourceSettings.GetString("ConfigMenu"));
                        string conf = Console.ReadLine();

                        if (conf == "1")
                        {
                            string langChoice = view.AskForInput("ChangeLang");
                            if (langChoice.ToLower() == "fr") Config.AppSettings.Instance.Language = Language.Francais;
                            else if (langChoice.ToLower() == "en") Config.AppSettings.Instance.Language = Language.English;
                            Console.Clear();
                        }
                        else if (conf == "2")
                        {
                            Console.WriteLine("Format (json/xml) ?");
                            string fmt = Console.ReadLine();

                            if (fmt == "xml" || fmt == "json")
                            {
                                Config.AppSettings.Instance.LogFormat = fmt;

                                controller.UpdateLogger();

                                Console.WriteLine($"Format changed to {fmt} !");
                            }
                        }
                        break;

                    case "6":
                        running = false;
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nAppuyez sur Entrée...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }
        static List<int> ParseJobSelection(string input, int maxJobs)
        {
            var result = new List<int>();

            if (string.IsNullOrWhiteSpace(input)) return result;

            if (input.ToLower() == "all")
            {
                for (int i = 0; i < maxJobs; i++) result.Add(i);
                return result;
            }
            var parts = input.Split(';');

            foreach (var part in parts)
            {
                if (part.Contains("-"))
                {
                    var ranges = part.Split('-');
                    if (ranges.Length == 2 &&
                        int.TryParse(ranges[0], out int start) &&
                        int.TryParse(ranges[1], out int end))
                    {
                        int s = Math.Min(start, end) - 1;
                        int e = Math.Max(start, end) - 1;

                        for (int i = s; i <= e; i++) result.Add(i);
                    }
                }
                else
                {
                    if (int.TryParse(part, out int id))
                    {
                        result.Add(id - 1);
                    }
                }
            }
            return result.Where(id => id >= 0 && id < maxJobs).Distinct().OrderBy(id => id).ToList();
        }
    }
}