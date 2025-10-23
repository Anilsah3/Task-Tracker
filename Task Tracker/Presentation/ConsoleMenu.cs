// Presentation/ConsoleMenu.cs
#nullable enable
using System;
using System.Collections.Generic;
using Task_Tracker.Application;
using Task_Tracker.Task;
using DomainTaskStatus = Task_Tracker.Task.TaskStatus; // avoid clash with System.Threading.Tasks.TaskStatus

namespace Task_Tracker.Presentation
{
    // Console menu for the Task Tracker application
    public class ConsoleMenu
    {
        private readonly TaskManager _manager;   // main logic
        private readonly object _reports;        // placeholder for later
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

        // ===== UI Helpers =====

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
            Console.WriteLine("Goodbye!");
            _running = false;
        }

        // ===== Flows =====

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
            Console.Write("Task ID: ");
            var idRaw = Console.ReadLine();

            if (!Guid.TryParse(idRaw, out var id))
            {
                Console.WriteLine("That doesn’t look like a valid ID.");
                return;
            }

            Console.Write("New status (Todo, InProgress, Done, Archived): ");
            var statusRaw = Console.ReadLine();

            // Use the alias 'DomainTaskStatus' to avoid ambiguity with System.Threading.Tasks.TaskStatus
            if (!Enum.TryParse<DomainTaskStatus>(statusRaw, true, out var newStatus))
            {
                Console.WriteLine("Unknown status. Try: Todo, InProgress, Done, or Archived.");
                return;
            }

            var ok = _manager.UpdateStatus(id, newStatus);
            Console.WriteLine(ok ? "Status updated." : "No task found with that ID.");
        }

        // Friendlier search: paste an ID or type any text; it auto-detects
        private void SearchTasksFlow()
        {
            Console.Write("Enter a Task ID (GUID) or part of the Title: ");
            var input = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Please type something to search.");
                return;
            }

            // If it parses as a GUID, search by ID
            if (Guid.TryParse(input, out var id))
            {
                var hit = _manager.FindById(id);
                if (hit is null)
                {
                    Console.WriteLine("No task found with that ID.");
                    return;
                }

                PrintTasks(new[] { hit });
                return;
            }

            // Otherwise do a title search (case-insensitive)
            var matches = _manager.SearchByTitle(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("No tasks matched.");
                return;
            }

            PrintTasks(matches);
        }

        private static void PrintTasks(IEnumerable<TaskItem> items)
        {
            Console.WriteLine();
            Console.WriteLine("Id                                   | Title                | Due        | Priority | Status      | Assignee");
            Console.WriteLine(new string('-', 100));

            foreach (var t in items)
            {
                var title = (t.Title ?? "").PadRight(20);
                if (title.Length > 20) title = title[..20];

                var due = t.DueDate.ToString("yyyy-MM-dd");
                var assignee = string.IsNullOrWhiteSpace(t.Assignee) ? "-" : t.Assignee!;

                Console.WriteLine($"{t.Id} | {title} | {due} | {t.Priority,-8} | {t.Status,-10} | {assignee}");
            }
        }

        // Placeholders for upcoming features
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
