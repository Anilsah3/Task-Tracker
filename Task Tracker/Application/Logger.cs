
using System;

namespace Task_Tracker.Application
{
    /// <summary>
    /// Very small file logger. Appends lines to a txt file.
    /// If writing fails, we just ignore it so the app doesn't crash.
    /// </summary>
    public class Logger
    {
        private readonly string _filePath;
        private readonly object _lock = new();

        public Logger(string filePath)
        {
            _filePath = filePath;
        }

        public void Info(string message)  => Write("INFO", message);
        public void Warn(string message)  => Write("WARN", message);
        public void Error(string message) => Write("ERROR", message);

        private void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            try
            {
                lock (_lock)
                {
                    File.AppendAllLines(_filePath, new[] { line });
                }
            }
            catch
            {
                // don't crash the program because of logging
            }
        }
    }
}
