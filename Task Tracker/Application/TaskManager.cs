// Application/TaskManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Task_Tracker.Task;
using TaskStatus = Task_Tracker.Task.TaskStatus;  // avoid clash with System.Threading.Tasks.TaskStatus

namespace Task_Tracker.Application
{
    /// <summary>
    /// Simple in-memory task manager.
    /// Later we can swap this to a file or DB without changing the callers.
    /// </summary>
    public class TaskManager
    {
        // All tasks live here for now.
        private readonly List<TaskItem> _tasks = new();

        /// <summary>
        /// Create a new task and add it to the list.
        /// </summary>
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
            return task;
        }

        /// <summary>
        /// Return a task with the given Id, or null if it doesn't exist.
        /// </summary>
        public TaskItem? FindById(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Change only the status of a task. Returns true on success, false if not found.
        /// </summary>
        public bool UpdateStatus(Guid id, TaskStatus newStatus)
        {
            var task = FindById(id);
            if (task is null) return false;

            if (task.Status == newStatus)
                return true; // no change needed

            task.Status = newStatus;
            return true;
        }

        /// <summary>
        /// Case-insensitive title search.
        /// Returns all tasks whose Title contains the given text.
        /// Sorted by earliest due first.
        /// </summary>
        public List<TaskItem> SearchByTitle(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<TaskItem>();

            term = term.Trim().ToLowerInvariant();

            return _tasks
                .Where(t => (t.Title ?? string.Empty).ToLowerInvariant().Contains(term))
                .OrderBy(t => t.DueDate)
                .ToList();
        }

        /// <summary>
        /// Sort tasks by due date using a manual insertion sort.
        /// This is the DSA algorithm we can describe in the report.
        /// </summary>
        public List<TaskItem> SortByDueDateManual(bool ascending = true)
        {
            // Work on a copy so we don't change the original order accidentally
            var list = _tasks.ToList();

            for (int i = 1; i < list.Count; i++)
            {
                var current = list[i];
                int j = i - 1;

                // Move items that are "greater" to the right
                while (j >= 0 && CompareDue(list[j], current, ascending) > 0)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                // Drop current into the hole we created
                list[j + 1] = current;
            }

            return list;
        }

        // Helper for insertion sort
        private static int CompareDue(TaskItem a, TaskItem b, bool ascending)
        {
            int cmp = DateTime.Compare(a.DueDate, b.DueDate);
            return ascending ? cmp : -cmp;
        }

        /// <summary>
        /// Sort by priority. By default we show highest priority first.
        /// </summary>
        public List<TaskItem> SortByPriority(bool ascending = false)
        {
            return ascending
                ? _tasks.OrderBy(t => t.Priority).ToList()
                : _tasks.OrderByDescending(t => t.Priority).ToList();
        }

        /// <summary>
        /// Return only tasks that are past their due date and still active.
        /// We don't count Done or Archived as overdue.
        /// </summary>
        public List<TaskItem> GetOverdue(DateTime today)
        {
            return _tasks
                .Where(t =>
                    t.DueDate.Date < today.Date &&
                    t.Status != TaskStatus.Done &&
                    t.Status != TaskStatus.Archived)
                .OrderBy(t => t.DueDate)
                .ToList();
        }

        /// <summary>
        /// Export overdue tasks to a CSV file.
        /// File will have a simple header row.
        /// </summary>
        public void ExportOverdueToCsv(string filePath, DateTime today)
        {
            var overdue = GetOverdue(today);

            // make sure folder exists
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var writer = new StreamWriter(filePath);
            // header
            writer.WriteLine("Id,Title,DueDate,Priority,Status,Assignee");

            foreach (var t in overdue)
            {
                writer.WriteLine($"{t.Id},{Escape(t.Title)},{t.DueDate:yyyy-MM-dd},{t.Priority},{t.Status},{Escape(t.Assignee)}");
            }
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // very simple "escape": just remove commas so CSV doesn't break
            return value.Replace(",", " ");
        }

        /// <summary>
        /// Simple read-only snapshot of all tasks.
        /// </summary>
        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
