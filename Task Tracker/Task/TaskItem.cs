
#nullable enable
using System;
using System.Text.Json.Serialization;

namespace Task_Tracker.Task
{
    public enum Priority { Low, Medium, High, Critical }

    public enum TaskStatus { InProgress, Done, Archived }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime DueDate { get; set; }


        public Priority Priority { get; set; } = Priority.Medium;

        public TaskStatus Status { get; set; } = TaskStatus.InProgress;

        
        public string Assignee { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    
        public int DaysLeft => (int)Math.Floor((DueDate.Date - DateTime.Today).TotalDays);
    }
}
