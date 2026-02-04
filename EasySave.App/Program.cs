using EasySave.App.Controllers;
using EasySave.App.Views;
using EasySave.App.Enumerations;
using EasySave.App.Parsing;
using System;

namespace EasySave.App
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupController controller = new BackupController();
            ConsoleView view = new ConsoleView();

            // =======================
            // CLI MODE (no menu)
            // =======================
            if (args != null && args.Length > 0)
            {
                try
                {
                    var jobIds = CommandLineParser.ParseJobIds(args, minId: 1, maxId: 5);

                    bool anyError = false;

                    foreach (var id in jobIds)
                    {
                        try
                        {
                            Console.WriteLine($"[CLI] Executing job {id}...");
                            controller.ExecuteJob(id);
                            Console.WriteLine($"[CLI] Job {id} completed.");
                        }
                        catch (Exception ex)
                        {
                            anyError = true;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[CLI] Job {id} failed: {ex.Message}");
                            Console.ResetColor();
                        }
                    }

                    Console.ForegroundColor = anyError ? ConsoleColor.Yellow : ConsoleColor.Green;

                    // If you don't have "CompletedWithErrors" in resources yet,
                    // replace this line with: Console.WriteLine(anyError ? "Completed with errors." : "Operation successful!");
                    Console.WriteLine(
                        anyError
                            ? Config.ResourceSettings.GetString("CompletedWithErrors")
                            : Config.ResourceSettings.GetString("Success")
                    );

                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    view.DisplayError(ex.Message);
                }

                return;
            }
            // =======================

            bool running = true;

            Console.WriteLine(Config.ResourceSettings.GetString("Welcome"));

            while (running)
            {
                view.DisplayMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        var jobs = controller.GetJobs();
                        view.DisplayJobs(jobs);
                        break;

                    case "2":
                        try
                        {
                            string name = view.AskForInput("EnterName");
                            string source = view.AskForInput("EnterSource");
                            string target = view.AskForInput("EnterTarget");
                            BackupType type = view.AskForBackupType();

                            controller.CreateJob(name, source, target, type);
                            view.DisplaySuccess();
                        }
                        catch (Exception ex)
                        {
                            view.DisplayError(ex.Message);
                        }
                        break;

                    case "3":
                        view.DisplayJobs(controller.GetJobs());
                        string indexStr = view.AskForInput("EnterName"); // Recommended: "EnterJobId"

                        if (int.TryParse(indexStr, out int index))
                        {
                            try
                            {
                                controller.ExecuteJob(index);
                                view.DisplaySuccess();
                            }
                            catch (Exception ex)
                            {
                                view.DisplayError(ex.Message);
                            }
                        }
                        else
                        {
                            view.DisplayError("Invalid index.");
                        }
                        break;

                    case "4":
                        view.DisplayJobs(controller.GetJobs());
                        string deleteIndexStr = view.AskForInput("EnterName"); // Recommended: "EnterJobId"

                        if (int.TryParse(deleteIndexStr, out int deleteIndex))
                        {
                            try
                            {
                                controller.DeleteJob(deleteIndex);

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(Config.ResourceSettings.GetString("JobDeleted"));
                                Console.ResetColor();
                            }
                            catch (Exception ex)
                            {
                                view.DisplayError(ex.Message);
                            }
                        }
                        else
                        {
                            view.DisplayError("Invalid index.");
                        }
                        break;

                    case "5":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                Console.WriteLine();
            }
        }
    }
}