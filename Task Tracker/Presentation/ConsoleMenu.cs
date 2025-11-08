#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Task_Tracker.Application;
using Task_Tracker.Task;

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
                        case "2": SearchTasksFlow(); break;
                        case "3": ListSortedFlow(); break;
                        case "4": ShowOverdueFlow(); break;
                        case "5": ExportOverdueCsvFlow(); break;
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
            Console.WriteLine("2) Search Tasks");
            Console.WriteLine("3) List Tasks (Sort by due/priority)");
            Console.WriteLine("4) Show Overdue");
            Console.WriteLine("5) Export Overdue CSV");
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

        private static DateTime ReadDate(string label, bool allowEmpty = false, DateTime? defaultValue = null)
        {
            while (true)
            {
                Console.Write($"{label}{(allowEmpty ? " (Enter for default)" : "")}: ");
                var raw = Console.ReadLine();

                if (allowEmpty && string.IsNullOrWhiteSpace(raw))
                    return (defaultValue ?? DateTime.Today).Date;

                if (!DateTime.TryParse(raw, out var dt))
                {
                    Console.WriteLine("Invalid date. Use format yyyy-MM-dd.");
                    continue;
                }
                return dt.Date;
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
            Console.WriteLine();
            Console.WriteLine("=== Add a New Task ===");

            var title     = ReadNonEmpty("Title", minLen: 3, maxLen: 80);
            var desc      = ReadNonEmpty("Description", minLen: 5, maxLen: 5000);
            var start     = ReadDate("Start date (yyyy-MM-dd)", allowEmpty: true, defaultValue: DateTime.Today);
            var due       = ReadDate("Due date (yyyy-MM-dd)");
            var priority  = ReadPriority();
            var assignee  = ReadNonEmpty("Assignee", minLen: 2, maxLen: 50);

            try
            {
                var task = _manager.CreateTask(title, desc, start, due, priority, assignee);

                Console.WriteLine();
                Console.WriteLine("Task created and saved:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"ID        : {task.Id}");
                Console.WriteLine($"Title     : {task.Title}");
                Console.WriteLine($"Desc      : {TrimForLine(task.Description, 80)}");
                Console.WriteLine($"StartDate : {task.StartDate:yyyy-MM-dd}");
                Console.WriteLine($"DueDate   : {task.DueDate:yyyy-MM-dd}");
                Console.WriteLine($"Days Left : {DaysLeft(task.DueDate)}");
                Console.WriteLine($"Priority  : {task.Priority}");
                Console.WriteLine($"Assignee  : {task.Assignee}");
                Console.WriteLine(new string('-', 60));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create task: {ex.Message}");
            }
        }

        private static string TrimForLine(string? s, int max)
        {
            s ??= "";
            return s.Length <= max ? s : s.Substring(0, max - 3) + "...";
        }

        private static int DaysLeft(DateTime due) =>
            (int)Math.Floor((due.Date - DateTime.Today).TotalDays);

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

            Console.WriteLine("Unknown sort key.");
        }

        private void ShowOverdueFlow()
        {
            var today = DateTime.Today;
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
            Console.Write("Enter file path: ");
            var path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(Environment.CurrentDirectory, "overdue.csv");

            var today = DateTime.Today;

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
            Console.WriteLine("Id                                   | Title                | Start      | Due        | DLeft | Priority | Assignee");
            Console.WriteLine(new string('-', 120));

            foreach (var t in items)
            {
                var title = (t.Title ?? "").PadRight(20);
                if (title.Length > 20) title = title[..20];

                var start = t.StartDate.ToString("yyyy-MM-dd");
                var due   = t.DueDate.ToString("yyyy-MM-dd");
                var dleft = DaysLeft(t.DueDate).ToString().PadLeft(5);
                var assignee = string.IsNullOrWhiteSpace(t.Assignee) ? "-" : t.Assignee;

                Console.WriteLine($"{t.Id} | {title} | {start} | {due} | {dleft} | {t.Priority,-8} | {assignee}");
            }
        }
    }
}
