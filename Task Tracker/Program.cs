// Program.cs
using Task_Tracker.Presentation;
using Task_Tracker.Application;

var manager = new TaskManager();      // real manager
var dummyReports = new object();      // placeholder for now

var menu = new ConsoleMenu(manager, dummyReports);
menu.Run();