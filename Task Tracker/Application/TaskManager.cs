
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Task_Tracker.Task;
using TaskStatus = Task_Tracker.Task.TaskStatus;

namespace Task_Tracker.Application
{
    /// <summary>
    /// Simple in-memory task manager.
    /// Now also supports saving/loading tasks.json.
    /// </summary>
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

        // PERSISTENCE 

        public void LoadFromJson()
        {
            if (!File.Exists(_dataFile))
            {
                _logger.Info($"Data file '{_dataFile}' not found. Starting with empty list.");
                return;
            }

            try
            {
                var json = File.ReadAllText(_dataFile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.Info($"Data file '{_dataFile}' was empty. Starting with empty list.");
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


        public TaskItem CreateTask(string title, string? desc, DateTime due, Priority prio, string? assignee)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            var task = new TaskItem
            {
                Title       = title.Trim(),
                Description = string.IsNullOrWhiteSpace(desc) ? null : desc.Trim(),
                DueDate     = due,
                Priority    = prio,
                Assignee    = string.IsNullOrWhiteSpace(assignee) ? null : assignee.Trim(),
                Status      = TaskStatus.Todo
            };

            _tasks.Add(task);
            _logger.Info($"Created task {task.Id} ({task.Title})");

            return task;
        }

        public TaskItem? FindById(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        public bool UpdateStatus(Guid id, TaskStatus newStatus)
        {
            var task = FindById(id);
            if (task is null)
            {
                _logger.Warn($"Tried to update task {id} but it was not found.");
                return false;
            }

            if (task.Status == newStatus)
                return true;

            task.Status = newStatus;
            _logger.Info($"Updated task {id} to {newStatus}");
            return true;
        }

        public List<TaskItem> SearchByTitle(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<TaskItem>();

            term = term.Trim().ToLowerInvariant();

            var results = _tasks
                .Where(t => (t.Title ?? string.Empty).ToLowerInvariant().Contains(term))
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
                .Where(t =>
                    t.DueDate.Date < today.Date &&
                    t.Status != TaskStatus.Done &&
                    t.Status != TaskStatus.Archived)
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
            {
                Directory.CreateDirectory(dir);
            }

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
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return value.Replace(",", " ");
        }

        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
