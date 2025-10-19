// Application/TaskManager.cs
using Task_Tracker.Task;
using TaskStatus = Task_Tracker.Task.TaskStatus; 

namespace Task_Tracker.Application
{
    public class TaskManager
    {
        // Temporary in-memory list. We'll replace with a repository later.
        private readonly List<TaskItem> _tasks = new();

        public TaskItem CreateTask(string title, string? desc, DateTime due, Priority prio, string? assignee)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.");

            var task = new TaskItem
            {
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(desc) ? null : desc.Trim(),
                DueDate = due,
                Priority = prio,
                Assignee = string.IsNullOrWhiteSpace(assignee) ? null : assignee.Trim(),
                Status = TaskStatus.Todo
            };

            _tasks.Add(task);
            return task;
        }

        // (Optional helper for later) expose the list read-only
        public IReadOnlyList<TaskItem> All() => _tasks.AsReadOnly();
    }
}
