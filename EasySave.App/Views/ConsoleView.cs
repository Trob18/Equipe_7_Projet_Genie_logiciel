using EasySave.App.Config;
using EasySave.App.Enumerations;
using EasySave.App.Models;
using System;
using System.Collections.Generic;

namespace EasySave.App.Views
{
    public class ConsoleView
    {
        public void DisplayMenu()
        {
            Console.WriteLine("\n----------------------------------");
            Console.WriteLine(ResourceSettings.GetString("MenuTitle"));
            Console.WriteLine("----------------------------------");
            Console.WriteLine(ResourceSettings.GetString("Option1"));
            Console.WriteLine(ResourceSettings.GetString("Option2"));
            Console.WriteLine(ResourceSettings.GetString("Option3"));
            Console.WriteLine(ResourceSettings.GetString("OptionDelete"));
            Console.WriteLine(ResourceSettings.GetString("OptionConfig"));
            Console.WriteLine(ResourceSettings.GetString("Option4"));
            Console.WriteLine("----------------------------------");
            Console.Write("Choice: ");
        }

        public void DisplayJobs(List<BackupJob> jobs)
        {
            Console.WriteLine("\n--- LIST ---");
            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs available.");
            }
            else
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    Console.WriteLine($"{i}. {jobs[i].Name} [{jobs[i].Type}] -> {jobs[i].SourceDirectory}");
                }
            }
        }

        public string AskForInput(string resourceKey)
        {
            Console.WriteLine(ResourceSettings.GetString(resourceKey));
            return Console.ReadLine();
        }

        public BackupType AskForBackupType()
        {
            Console.WriteLine(ResourceSettings.GetString("EnterType"));
            string input = Console.ReadLine();

            if (input == "2")
                return BackupType.Differential;

            return BackupType.Full;
        }

        public void DisplaySuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(ResourceSettings.GetString("Success"));
            Console.ResetColor();
        }

        public void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ResourceSettings.GetString("Error") + message);
            Console.ResetColor();
        }
    }
}