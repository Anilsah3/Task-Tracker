
#nullable enable
using Task_Tracker.Application;
using Task_Tracker.Presentation;

namespace Task_Tracker
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var logger = new Logger();
            var manager = new TaskManager(logger, dataFile: "tasks.json");
            manager.LoadFromJson();
            var reports = new object();

            var menu = new ConsoleMenu(manager, reports);
            menu.Run();
            manager.SaveToJson();
        }
    }
}
