
namespace Task_Tracker.Task
{
    public enum Priority { Low, Medium, High, Critical }
    public enum TaskStatus { Todo, InProgress, Done, Archived }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public string? Assignee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
