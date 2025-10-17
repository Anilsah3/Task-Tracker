// Presentation/ConsoleMenu.cs
#nullable enable
using System;

namespace Task_Tracker.Presentation
{
    // This is the console menu to show the task tracker presentation
    public class ConsoleMenu
    {
        private readonly object _manager; // TODO: replace with TaskManager
        private readonly object _reports; // TODO: replace with ReportService
        private bool _running = true;

        public ConsoleMenu(object manager, object reports)
        {
            _manager = manager;
            _reports = reports;
        }

        public void Run()
        {
            while (_running)
            {
                Console.Clear();
                ShowMain();
                Console.Write("Choose: ");
                var choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            // Add Task
                            Console.WriteLine("Adding the task");
                            break;

                        case "2":
                            // Update Task Status
                            Console.WriteLine("(Updating the task");
                            break;

                        case "3":
                            // Search Tasks
                            Console.WriteLine("(Search the task");
                            break;

                        case "4":
                            // List Sorted (by due/priority)
                            Console.WriteLine("(Listing the sorted task");
                            break;

                        case "5":
                            // Show Overdue
                            Console.WriteLine("(Showing the overdue task");
                            break;

                        case "6":
                            // Export Overdue CSV
                            Console.WriteLine("(showing the overdue csv");
                            break;

                        case "0":
                            SaveAndExit();
                            continue;

                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Pause();
            }
        }

        private static void ShowMain()
        {
            Console.WriteLine("=== Task Tracker ===");
            Console.WriteLine("1) Add Task");
            Console.WriteLine("2) Update Task Status");
            Console.WriteLine("3) Search Tasks");
            Console.WriteLine("4) List Tasks (Sort by due/priority)");
            Console.WriteLine("5) Show Overdue");
            Console.WriteLine("6) Export Overdue CSV");
            Console.WriteLine("0) Save & Exit");
            Console.WriteLine();
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to continue...");
            Console.ReadLine();
        }

        private void SaveAndExit()
        {
            // In the future you can trigger a repo SaveChanges here if needed.
            Console.WriteLine("Goodbye!");
            _running = false;
        }
    }
}
