
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Task_Tracker.Application;
using Task_Tracker.Task;
using DomainTaskStatus = Task_Tracker.Task.TaskStatus;

namespace Task_Tracker.Presentation
{
    public class ConsoleMenu
    {
        private readonly TaskManager _manager;
        private readonly object _reports;
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
                        case "1": AddTaskFlow(); break;
                        case "2": UpdateStatusFlow(); break;
                        case "3": SearchTasksFlow(); break;
                        case "4": ListSortedFlow(); break;
                        case "5": ShowOverdueFlow(); break;
                        case "6": ExportOverdueCsvFlow(); break;
                        case "0": SaveAndExit(); continue;
                        default:  Console.WriteLine("Invalid option. Please try again."); break;
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
            _manager.SaveToJson();
            Console.WriteLine("Data saved. Goodbye!");
            _running = false;
        }

        // --------- helpers for user input with validation loops ----------

        private static string ReadNonEmpty(string label, int minLen = 1, int maxLen = int.MaxValue)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                var s = (Console.ReadLine() ?? "").Trim();
                if (s.Length < minLen)
                {
                    Console.WriteLine($"Must be at least {minLen} character(s).");
                    continue;
                }
                if (s.Length > maxLen)
                {
                    Console.WriteLine($"Must be {maxLen} characters or less.");
                    continue;
                }
                return s;
            }
        }

        private static DateTime ReadDueDate()
        {
            while (true)
            {
                Console.Write("Due date (yyyy-MM-dd): ");
                var raw = Console.ReadLine();
                if (!DateTime.TryParse(raw, out var due))
                {
                    Console.WriteLine("Invalid date. Use format yyyy-MM-dd.");
                    continue;
                }
                try
                {
                    InputRules.EnsureDueDate(due);
                    return due;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static Priority ReadPriority()
        {
            while (true)
            {
                Console.Write("Priority (Low, Medium, High, Critical): ");
                var raw = (Console.ReadLine() ?? "").Trim();
                if (Enum.TryParse<Priority>(raw, true, out var pr))
                    return pr;

                Console.WriteLine("Invalid priority. Try: Low, Medium, High, Critical.");
            }
        }

        // --------------------- flows -----------------------

        private void AddTaskFlow()
        {
            var title    = ReadNonEmpty("Title", minLen: 3, maxLen: 80);
            var desc     = ReadNonEmpty("Description", minLen: 5, maxLen: 5000);
            var due      = ReadDueDate();
            var priority = ReadPriority();
            var assignee = ReadNonEmpty("Assignee", minLen: 2, maxLen: 50);

            try
            {
                var task = _manager.CreateTask(title, desc, due, priority, assignee);
                Console.WriteLine($" Task created. ID: {task.Id}");
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

            var task = _manager.FindById(id);
            if (task is null)
            {
                Console.WriteLine("No task found with that ID.");
                return;
            }

            Console.Write("New status (Todo, InProgress, Done, Archived): ");
            var statusRaw = Console.ReadLine();

            if (!Enum.TryParse<DomainTaskStatus>(statusRaw, true, out var newStatus))
            {
                Console.WriteLine("Unknown status. Try: Todo, InProgress, Done, Archived.");
                return;
            }

            try
            {
                var ok = _manager.UpdateStatus(id, newStatus);
                Console.WriteLine(ok ? "Status updated." : "Could not update status.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
            }
        }

        private void SearchTasksFlow()
        {
            Console.Write("Enter a Task ID (GUID) or part of the Title: ");
            var input = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Please type something to search.");
                return;
            }

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

            if (input.Length < 2)
            {
                Console.WriteLine("Please enter at least 2 characters for title search.");
                return;
            }

            var matches = _manager.SearchByTitle(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("No tasks matched.");
                return;
            }

            PrintTasks(matches);
        }

        private void ListSortedFlow()
        {
            Console.Write("Sort by (due | priority): ");
            var sortKey = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            Console.Write("Direction (asc | desc): ");
            var dir = (Console.ReadLine() ?? "asc").Trim().ToLowerInvariant();
            bool ascending = dir != "desc";

            if (sortKey == "due")
            {
                var sorted = _manager.SortByDueDateManual(ascending);
                PrintTasks(sorted);
                return;
            }

            if (sortKey == "priority")
            {
                var sorted = _manager.SortByPriority(ascending);
                PrintTasks(sorted);
                return;
            }

            Console.WriteLine("I didn't understand that. Try 'due' or 'priority'.");
        }

        private void ShowOverdueFlow()
        {
            var today = DateTime.Now;
            var overdue = _manager.GetOverdue(today);

            if (overdue.Count == 0)
            {
                Console.WriteLine("No overdue tasks");
                return;
            }

            Console.WriteLine("Overdue tasks:");
            PrintTasks(overdue);
        }

        private void ExportOverdueCsvFlow()
        {
            Console.Write("Enter file path (leave empty for ./overdue.csv): ");
            var path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(Environment.CurrentDirectory, "overdue.csv");

            var today = DateTime.Now;

            try
            {
                _manager.ExportOverdueToCsv(path, today);
                Console.WriteLine($"Overdue tasks exported to: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not export overdue tasks: {ex.Message}");
            }
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
                var assignee = string.IsNullOrWhiteSpace(t.Assignee) ? "-" : t.Assignee;

                Console.WriteLine($"{t.Id} | {title} | {due} | {t.Priority,-8} | {t.Status,-10} | {assignee}");
            }
        }
    }
}
