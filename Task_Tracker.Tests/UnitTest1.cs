using System;
using Task_Tracker.Application;
using Task_Tracker.Task;
using Xunit;
using TaskStatus = Task_Tracker.Task.TaskStatus;   // <-- fix the ambiguity

namespace Task_Tracker.Tests
{
    public class TaskManagerTests
    {
        private class TestLogger : Logger
        {
            public TestLogger() : base("test-logs.txt") { }
        }

        [Fact]
        public void Can_create_task()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");
            manager.LoadFromJson();

            var task = manager.CreateTask("Do work", "desc", DateTime.Today, Priority.Medium, "me");

            Assert.NotNull(task);
            Assert.Equal("Do work", task.Title);
            Assert.Single(manager.All());
        }

        [Fact]
        public void Can_update_status()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");

            var task = manager.CreateTask("Do work", null, DateTime.Today, Priority.Low, null);

            var ok = manager.UpdateStatus(task.Id, TaskStatus.Done);

            Assert.True(ok);
            Assert.Equal(TaskStatus.Done, manager.FindById(task.Id)!.Status);
        }

        [Fact]
        public void Search_by_title_returns_matching_tasks()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");

            manager.CreateTask("Fix login", null, DateTime.Today, Priority.High, null);
            manager.CreateTask("Write report", null, DateTime.Today, Priority.Medium, null);
            manager.CreateTask("Fix UI", null, DateTime.Today, Priority.Low, null);

            var result = manager.SearchByTitle("fix");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Sort_by_due_date_orders_earliest_first()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");

            manager.CreateTask("t1", null, new DateTime(2025, 10, 5), Priority.Medium, null);
            manager.CreateTask("t2", null, new DateTime(2025, 9, 30), Priority.Medium, null);
            manager.CreateTask("t3", null, new DateTime(2025, 10, 1), Priority.Medium, null);

            var sorted = manager.SortByDueDateManual(true);

            Assert.Equal("t2", sorted[0].Title);
            Assert.Equal("t3", sorted[1].Title);
            Assert.Equal("t1", sorted[2].Title);
        }

        [Fact]
        public void Overdue_returns_only_past_and_not_done()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");

            var o1 = manager.CreateTask("Old", null, DateTime.Today.AddDays(-2), Priority.High, null);
            var f1 = manager.CreateTask("Future", null, DateTime.Today.AddDays(3), Priority.Medium, null);
            var o2 = manager.CreateTask("Old but done", null, DateTime.Today.AddDays(-1), Priority.Low, null);
            manager.UpdateStatus(o2.Id, TaskStatus.Done);

            var overdue = manager.GetOverdue(DateTime.Today);

            Assert.Single(overdue);
            Assert.Equal(o1.Id, overdue[0].Id);
        }

        // extra one to catch the "overdue also returns done" bug
        [Fact]
        public void Overdue_should_not_return_done_tasks()
        {
            var logger = new TestLogger();
            var manager = new TaskManager(logger, "test-tasks.json");

            var open = manager.CreateTask("Old open", null, DateTime.Today.AddDays(-4), Priority.High, null);
            var done = manager.CreateTask("Old done", null, DateTime.Today.AddDays(-5), Priority.Medium, null);
            manager.UpdateStatus(done.Id, TaskStatus.Done);

            var overdue = manager.GetOverdue(DateTime.Today);

            Assert.Single(overdue);
            Assert.Equal(open.Id, overdue[0].Id);
        }
    }
}
