// Application/TaskManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Task_Tracker.Task;
using TaskStatus = Task_Tracker.Task.TaskStatus;  // alias to avoid clashing with System.Threading.Tasks.TaskStatus

namespace Task_Tracker.Application
{
    /// <summary>
    /// Simple in-memory task manager.
    /// Right now this just stores tasks in a List.
    /// You can later move this to file or DB without changing the menu calls.
    /// </summary>
    public class TaskManager
    {
        // We keep all tasks in memory for now.
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
        /// Change only the Status of a task. Returns true on success, false if not found.
        /// </summary>
        public bool UpdateStatus(Guid id, TaskStatus newStatus)
        {
            var task = FindById(id);
            if (task is null) return false;

            if (task.Status == newStatus)
                return true; // nothing to change

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
        /// This is your explicit DSA algorithm implementation.
        /// </summary>
        public List<TaskItem> SortByDueDateManual(bool ascending = true)
        {
            // Work on a copy so we don't mutate the original list
            var list = _tasks.ToList();

            // Insertion sort: walk forward, and insert each item into the "sorted" left side
            for (int i = 1; i < list.Count; i++)
            {
                var current = list[i];
                int j = i - 1;

                // Shift bigger (or smaller, depending on asc/desc) items to the right
                while (j >= 0 && CompareDue(list[j], current, ascending) > 0)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                // Drop the current item into the opening we just created
                list[j + 1] = current;
            }

            return list;
        }

        /// <summary>
        /// Helper to compare two tasks' due dates in the chosen direction.
        /// Returns:
        ///   >0 if 'a' should come AFTER 'b'
        ///    0 if equal
        ///   <0 if 'a' should come BEFORE 'b'
        /// </summary>
        private static int CompareDue(TaskItem a, TaskItem b, bool ascending)
        {
            int cmp = DateTime.Compare(a.DueDate, b.DueDate);
            return ascending ? cmp : -cmp;
        }

        /// <summary>
        /// Sort by priority.
        /// By default we show highest priority first (Critical > High > Medium > Low).
        /// This one uses LINQ since the assignment only requires at least one manual algorithm.
        /// </summary>
        public List<TaskItem> SortByPriority(bool ascending = false)
        {
            return ascending
                ? _tasks.OrderBy(t => t.Priority).ToList()
                : _tasks.OrderByDescending(t => t.Priority).ToList();
        }

        /// <summary>
        /// Simple read-only snapshot of all tasks. Useful if you want to dump everything.
        /// </summary>
        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
