
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Task_Tracker.Task;

namespace Task_Tracker.Application
{
    public class TaskManager
    {
        private readonly List<TaskItem> _tasks = new();
        private readonly Logger _logger;
        private readonly string _dataFile;
        public TaskManager(Logger logger, string dataFile = "tasks.json")
        {
            _logger = logger;
            _dataFile = dataFile;
        }

        // load everything from json file if it exists
        public void LoadFromJson()
        {
            if (!File.Exists(_dataFile))
            {
                _logger.Info($"Data file '{_dataFile}' not found. Starting empty.");
                return;
            }

            try
            {
                var json = File.ReadAllText(_dataFile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.Info($"Data file '{_dataFile}' was empty. Starting empty.");
                    return;
                }

                var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loaded != null)
                {
                    foreach (var t in loaded)
                    {
                        if (t.StartDate == default)
                        {
                            // Prefer CreatedAt if present; else use DueDate or today
                            t.StartDate = (t.CreatedAt != default ? t.CreatedAt.Date
                                          : (t.DueDate != default ? t.DueDate.Date : DateTime.Today));
                        }
                    }

                    _tasks.Clear();
                    _tasks.AddRange(loaded);
                    _logger.Info($"Loaded {_tasks.Count} task(s) from '{_dataFile}'.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read '{_dataFile}': {ex.Message}");
            }
        }

        // save to json so we don't lose work
        public void SaveToJson()
        {
            try
            {
                var json = JsonSerializer.Serialize(_tasks,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(_dataFile, json);
                _logger.Info($"Saved {_tasks.Count} task(s) to '{_dataFile}'.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not write '{_dataFile}': {ex.Message}");
            }
        }

        // === CREATE TASK ===
        public TaskItem CreateTask(string title, string desc, DateTime startDate, DateTime due, Priority prio, string assignee)
        {
            InputRules.EnsureTitle(title);
            InputRules.EnsureDescription(desc);
            InputRules.EnsureAssignee(assignee);
            InputRules.EnsureDueDate(due);
            InputRules.EnsurePriority(prio);

            // Only logical check: start should not be after due (does not block past)
            if (startDate.Date > due.Date)
                throw new ArgumentException("Start date cannot be after due date.");

            var now = DateTime.UtcNow;

            var task = new TaskItem
            {
                Title       = InputRules.Clean(title),
                Description = InputRules.Clean(desc),
                StartDate   = startDate.Date,
                DueDate     = due.Date,
                Priority    = prio,
                Assignee    = InputRules.Clean(assignee),
                CreatedAt   = now,
                UpdatedAt   = now
            };

            _tasks.Add(task);
            _logger.Info($"Created task {task.Id} ({task.Title})");

            // Auto-save immediately after creation
            SaveToJson();

            return task;
        }

        public TaskItem CreateTask(string title, string desc, DateTime due, Priority prio, string assignee)
            => CreateTask(title, desc, DateTime.Today, due, prio, assignee);

        public TaskItem? FindById(Guid id) => _tasks.FirstOrDefault(t => t.Id == id);

        // search by title (needs at least 2 chars)
        public List<TaskItem> SearchByTitle(string term)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
            {
                _logger.Warn("Search term too short. Minimum 2 characters.");
                return new List<TaskItem>();
            }

            var key = term.ToLowerInvariant();
            var results = _tasks
                .Where(t => (t.Title ?? string.Empty).ToLowerInvariant().Contains(key))
                .OrderBy(t => t.DueDate)
                .ToList();

            _logger.Info($"Search '{term}' -> {results.Count} result(s).");
            return results;
        }

        public List<TaskItem> SortByDueDateManual(bool ascending = true)
        {
            var list = _tasks.ToList();

            for (int i = 1; i < list.Count; i++)
            {
                var current = list[i];
                int j = i - 1;

                while (j >= 0 && CompareDue(list[j], current, ascending) > 0)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                list[j + 1] = current;
            }

            _logger.Info("Sorted by due date using insertion sort.");
            return list;
        }

        private static int CompareDue(TaskItem a, TaskItem b, bool ascending)
        {
            var cmp = DateTime.Compare(a.DueDate, b.DueDate);
            return ascending ? cmp : -cmp;
        }

        public List<TaskItem> SortByPriority(bool ascending = false)
        {
            var list = ascending
                ? _tasks.OrderBy(t => t.Priority).ToList()
                : _tasks.OrderByDescending(t => t.Priority).ToList();

            _logger.Info("Sorted by priority.");
            return list;
        }

        public List<TaskItem> GetOverdue(DateTime today)
        {
            var list = _tasks
                .Where(t => t.DueDate.Date < today.Date)
                .OrderBy(t => t.DueDate)
                .ToList();

            _logger.Info($"Fetched {list.Count} overdue task(s).");
            return list;
        }

        public List<TaskItem> GetDueWithin(DateTime today, int days)
        {
            var end = today.Date.AddDays(days);
            var list = _tasks
                .Where(t => t.DueDate.Date >= today.Date && t.DueDate.Date <= end)
                .OrderBy(t => t.DueDate)
                .ToList();

            _logger.Info($"Fetched {list.Count} task(s) due within {days} day(s).");
            return list;
        }

        public void ExportOverdueToCsv(string filePath, DateTime today)
        {
            var overdue = GetOverdue(today);

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Id,Title,StartDate,DueDate,DaysLeft,Priority,Assignee");

            foreach (var t in overdue)
            {
                var daysLeft = (int)Math.Floor((t.DueDate.Date - today.Date).TotalDays);
                writer.WriteLine($"{t.Id},{Escape(t.Title)},{t.StartDate:yyyy-MM-dd},{t.DueDate:yyyy-MM-dd},{daysLeft},{t.Priority},{Escape(t.Assignee)}");
            }

            _logger.Info($"Exported {overdue.Count} overdue task(s) to {filePath}");
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return value.Replace(",", " ");
        }

        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
