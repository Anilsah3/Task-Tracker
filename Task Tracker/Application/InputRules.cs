
#nullable enable
using System;
using Task_Tracker.Task;
// add an alias to avoid clash with System.Threading.Tasks.TaskStatus
using DomainTaskStatus = Task_Tracker.Task.TaskStatus;

namespace Task_Tracker.Application
{
    public static class InputRules
    {
        public static string Clean(string s) => (s ?? "").Trim();

        public static void EnsureTitle(string title)
        {
            title = Clean(title);
            if (title.Length < 3)  throw new ArgumentException("Title must be at least 3 characters.");
            if (title.Length > 80) throw new ArgumentException("Title must be 80 characters or less.");
        }

        public static void EnsureDescription(string desc)
        {
            desc = Clean(desc);
            if (string.IsNullOrWhiteSpace(desc)) throw new ArgumentException("Description is required.");
            if (desc.Length < 5)   throw new ArgumentException("Description must be at least 5 characters.");
            if (desc.Length > 5000) throw new ArgumentException("Description is too long (max 5000).");
        }

        public static void EnsureAssignee(string assignee)
        {
            assignee = Clean(assignee);
            if (string.IsNullOrWhiteSpace(assignee)) throw new ArgumentException("Assignee is required.");
            if (assignee.Length < 2)  throw new ArgumentException("Assignee must be at least 2 characters.");
            if (assignee.Length > 50) throw new ArgumentException("Assignee must be 50 characters or less.");
        }

        public static void EnsureDueDate(DateTime due)
        {
    
            if (due.Date < DateTime.Today) throw new ArgumentException("Due date cannot be in the past.");
        }

        public static void EnsurePriority(Priority p)
        {
            if (!Enum.IsDefined(typeof(Priority), p))
                throw new ArgumentException("Priority is not valid.");
        }

        public static void EnsureStatus(DomainTaskStatus s)
        {
            if (!Enum.IsDefined(typeof(DomainTaskStatus), s))
                throw new ArgumentException("Status is not valid.");
        }
    }
}
