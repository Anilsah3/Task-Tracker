// Presentation/ConsoleMenu.cs
#nullable enable
using System;
using Task_Tracker.Application;
using Task_Tracker.Task;


namespace Task_Tracker.Presentation
{
    // Console menu for the Task Tracker application
    public class ConsoleMenu
    {
        private readonly TaskManager _manager;  // TODO: replace with TaskManager
        private readonly object _reports; // TODO: replace with ReportService
        private bool _running = true;

        public ConsoleMenu(TaskManager manager, object reports)

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
                            AddTaskFlow();
                            break;

                        case "2":
                            UpdateStatusFlow();
                            break;

                        case "3":
                            SearchTasksFlow();
                            break;

                        case "4":
                            ListSortedFlow();
                            break;

                        case "5":
                            ShowOverdueFlow();
                            break;

                        case "6":
                            ExportOverdueCsvFlow();
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

        // ---------- Placeholder flows (implement later) ----------

      private void AddTaskFlow()
{
    Console.Write("Title: ");
    var title = Console.ReadLine() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Title is required.");
        return;
    }

    Console.Write("Description (optional): ");
    var desc = Console.ReadLine();

    Console.Write("Due date (yyyy-MM-dd): ");
    var dueRaw = Console.ReadLine();
    if (!DateTime.TryParse(dueRaw, out var due))
    {
        Console.WriteLine("Invalid date. Use format yyyy-MM-dd.");
        return;
    }

    Console.Write("Priority (Low, Medium, High, Critical): ");
    var prRaw = Console.ReadLine();
    if (!Enum.TryParse<Priority>(prRaw, true, out var prio))
    {
        Console.WriteLine("Invalid priority. Defaulting to Medium.");
        prio = Priority.Medium;
    }

    Console.Write("Assignee (optional): ");
    var asg = Console.ReadLine();

    try
    {
        var task = _manager.CreateTask(title, desc, due, prio, asg);
        Console.WriteLine($"✅ Task created. ID: {task.Id}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to create task: {ex.Message}");
    }
}


        private void UpdateStatusFlow()
        {
            Console.WriteLine("(Update Task Status) — feature coming soon.");
        }

        private void SearchTasksFlow()
        {
            Console.WriteLine("(Search Tasks) — feature coming soon.");
        }

        private void ListSortedFlow()
        {
            Console.WriteLine("(List Sorted by due/priority) — feature coming soon.");
        }

        private void ShowOverdueFlow()
        {
            Console.WriteLine("(Show Overdue) — feature coming soon.");
        }

        private void ExportOverdueCsvFlow()
        {
            Console.WriteLine("(Export Overdue CSV) — feature coming soon.");
        }
    }
}
