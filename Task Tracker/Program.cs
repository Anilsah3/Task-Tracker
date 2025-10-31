
using Task_Tracker.Application;
using Task_Tracker.Presentation;

namespace Task_Tracker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger("logs.txt");
            var manager = new TaskManager(logger, "tasks.json");

            //  load saved tasks (if file exists)
            manager.LoadFromJson();

            var menu = new ConsoleMenu(manager, new object());
            menu.Run();
        }
    }
}
