// Application/TaskManager.cs
using Task_Tracker.Task;
using TaskStatus = Task_Tracker.Task.TaskStatus;

namespace Task_Tracker.Application
{
    /// <summary>
    /// Simple in-memory task manager.
    /// You can swap this to a file/database later without changing the menu code.
    /// </summary>
    public class TaskManager
    {
        // For now we keep tasks in a list.
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
        /// Find a task by its Id. Returns null if not found.
        /// </summary>
        public TaskItem? FindById(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Update only the status of a task.
        /// Returns true if the task existed and was updated; false if not found.
        /// </summary>
        public bool UpdateStatus(Guid id, TaskStatus newStatus)
        {
            var task = FindById(id);
            if (task is null) return false;

            // Nothing to change
            if (task.Status == newStatus) return true;

            task.Status = newStatus;
            return true;
        }

        /// <summary>
        /// Read-only view of all tasks (useful for listing/searching).
        /// </summary>
        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
