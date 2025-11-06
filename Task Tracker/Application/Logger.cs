
#nullable enable
using System;

namespace Task_Tracker.Application
{
    public class Logger
    {
        private static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public void Info(string msg)  => Log("INFO",  msg, ConsoleColor.Gray);
        public void Warn(string msg)  => Log("WARN",  msg, ConsoleColor.Yellow);
        public void Error(string msg) => Log("ERROR", msg, ConsoleColor.Red);

        private void Log(string level, string msg, ConsoleColor color)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{Now()}] [{level}] {msg}");
            Console.ForegroundColor = old;
        }
    }
}
