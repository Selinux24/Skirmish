using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Logger helper
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Internal log events list
        /// </summary>
        private static readonly ConcurrentQueue<LogEntry> log = new ConcurrentQueue<LogEntry>();
        /// <summary>
        /// Log level
        /// </summary>
        public static LogLevel LogLevel { get; set; }
        /// <summary>
        /// Log stack size
        /// </summary>
        public static int LogStackSize { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        static Logger()
        {
#if DEBUG
            LogLevel = LogLevel.Debug;
#else
            LogLevel = LogLevel.Information;
#endif

            LogStackSize = 100;
        }

        /// <summary>
        /// Writes a debug entry
        /// </summary>
        /// <param name="text">Entry text</param>
        public static void WriteDebug(string text)
        {
            Write(LogLevel.Debug, text);
        }
        /// <summary>
        /// Writes a information entry
        /// </summary>
        /// <param name="text">Entry text</param>
        public static void WriteInformation(string text)
        {
            Write(LogLevel.Information, text);
        }
        /// <summary>
        /// Writes a warning entry
        /// </summary>
        /// <param name="text">Entry text</param>
        public static void WriteWarning(string text)
        {
            Write(LogLevel.Warning, text);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="text">Entry text</param>
        public static void WriteError(string text)
        {
            Write(LogLevel.Error, text);
        }
        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="text">Entry text</param>
        public static void Write(LogLevel logLevel, string text)
        {
            if (logLevel < LogLevel)
            {
                // Discard log entry
                return;
            }

            log.Enqueue(new LogEntry { EventDate = DateTime.Now, LogLevel = logLevel, Text = text });

            if (LogStackSize <= 0)
            {
                // The stack never empties
                return;
            }

            while (log.Count > LogStackSize)
            {
                log.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Gets whether the log has any errors or not
        /// </summary>
        public static bool HasErrors()
        {
            return log.Any(l => l.LogLevel == LogLevel.Error);
        }

        /// <summary>
        /// Reads a list of entries from the log
        /// </summary>
        /// <param name="count">Number of entries, from the last one</param>
        public static IEnumerable<LogEntry> Read(int count = 0)
        {
            int take = count <= 0 ? log.Count : Math.Min(count, log.Count);
            int skip = Math.Max(0, log.Count - take);

            return log.Skip(skip).Take(take).ToArray();
        }
        /// <summary>
        /// Reads a list of entries from the log, of the specified log level or above
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="count">Number of entries, from the last one</param>
        public static IEnumerable<LogEntry> Read(LogLevel logLevel, int count = 0)
        {
            var logByLevel = log.Where(l => l.LogLevel >= logLevel).ToList();

            int take = count <= 0 ? logByLevel.Count : Math.Min(count, logByLevel.Count);
            int skip = Math.Max(0, logByLevel.Count - take);

            return logByLevel.Skip(skip).Take(take).ToArray();
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public static string ReadText(int maxLines = 0, bool reverse = true)
        {
            var logEntries = Read();
            if (!logEntries.Any())
            {
                return string.Empty;
            }

            if (reverse)
            {
                logEntries = logEntries.Reverse();
            }

            string logText = new string(logEntries.SelectMany(l => $"{l.EventDate:HH:mm:ss.fff} [{l.LogLevel}]> {l.Text}" + Environment.NewLine).ToArray());
            var lines = logText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (maxLines > 0 && lines.Length > maxLines)
            {
                lines = lines.Take(maxLines).ToArray();
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public static void Clear()
        {
            while (log.Count > 0)
            {
                log.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Dumps the log to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static void Dump(string fileName)
        {
            string dumpText = ReadText(0, false);
            File.WriteAllText(fileName, dumpText);
        }
    }
}
