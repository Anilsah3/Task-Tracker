
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Task_Tracker.Task;
using TaskStatusDomain = Task_Tracker.Task.TaskStatus;

namespace Task_Tracker.Application
{
    public class TaskManager
    {
        private readonly List<TaskItem> _tasks = new();
        private readonly Logger _logger;
        private readonly string _dataFile;

        // Change the default to "task.json" if you prefer singular
        public TaskManager(Logger logger, string dataFile = "tasks.json")
        {
            _logger = logger;
            _dataFile = dataFile;
        }


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

        // create a new task (assignee & description are required here)
        public TaskItem CreateTask(string title, string desc, DateTime due, Priority prio, string assignee)
        {
            // validate inputs
            InputRules.EnsureTitle(title);
            InputRules.EnsureDescription(desc);
            InputRules.EnsureAssignee(assignee);
            InputRules.EnsureDueDate(due);
            InputRules.EnsurePriority(prio);

            var now = DateTime.UtcNow;

            var task = new TaskItem
            {
                Title       = InputRules.Clean(title),
                Description = InputRules.Clean(desc),
                DueDate     = due.Date,
                Priority    = prio,
                Assignee    = InputRules.Clean(assignee),
                Status      = TaskStatusDomain.Todo,
                CreatedAt   = now,
                UpdatedAt   = now
            };

            _tasks.Add(task);
            _logger.Info($"Created task {task.Id} ({task.Title})");
            SaveToJson();

            return task;
        }

        public TaskItem? FindById(Guid id) => _tasks.FirstOrDefault(t => t.Id == id);

        // update only the status (with some basic rules)
        public bool UpdateStatus(Guid id, TaskStatusDomain newStatus)
        {
            var task = FindById(id);
            if (task is null)
            {
                _logger.Warn($"Tried to update task {id} but it was not found.");
                return false;
            }

            InputRules.EnsureStatus(newStatus);

            // simple rule: can't update an archived task
            if (task.Status == TaskStatusDomain.Archived)
                throw new InvalidOperationException("Archived tasks cannot be updated.");

            // optional: only allow Archive if Done
            if (newStatus == TaskStatusDomain.Archived && task.Status != TaskStatusDomain.Done)
                throw new InvalidOperationException("Only 'Done' tasks can be archived.");

            if (task.Status == newStatus) return true;

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;
            _logger.Info($"Updated task {id} to {newStatus}");

            // Persist immediately on status change as well
            SaveToJson();

            return true;
        }

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

        // sort by due date using simple insertion sort (as you had)
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
                .Where(t =>
                    t.DueDate.Date < today.Date &&
                    t.Status != TaskStatusDomain.Done &&
                    t.Status != TaskStatusDomain.Archived)
                .OrderBy(t => t.DueDate)
                .ToList();

            _logger.Info($"Fetched {list.Count} overdue task(s).");
            return list;
        }

        public void ExportOverdueToCsv(string filePath, DateTime today)
        {
            var overdue = GetOverdue(today);

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Id,Title,DueDate,Priority,Status,Assignee");

            foreach (var t in overdue)
            {
                writer.WriteLine($"{t.Id},{Escape(t.Title)},{t.DueDate:yyyy-MM-dd},{t.Priority},{t.Status},{Escape(t.Assignee)}");
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
