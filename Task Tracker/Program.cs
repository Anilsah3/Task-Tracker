// Program.cs
using Task_Tracker.Presentation;

// Placeholder instances so the menu compiles/runs before you build the real services.
var dummyManager = new object();
var dummyReports = new object();

var menu = new ConsoleMenu(dummyManager, dummyReports);
menu.Run();
