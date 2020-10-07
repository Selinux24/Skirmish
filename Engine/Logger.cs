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
        /// Custom filter function
        /// </summary>
        private static Func<LogEntry, bool> filterFnc;
        /// <summary>
        /// Custom format function
        /// </summary>
        private static Func<LogEntry, string> formatFnc;
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
        /// Writes a trace entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public static void WriteTrace(object caller, string text)
        {
            Write(LogLevel.Trace, caller, text);
        }
        /// <summary>
        /// Writes a trace entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public static void WriteTrace(string callerTypeName, string text)
        {
            Write(LogLevel.Trace, callerTypeName, text);
        }
        /// <summary>
        /// Writes a debug entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public static void WriteDebug(object caller, string text)
        {
            Write(LogLevel.Debug, caller, text);
        }
        /// <summary>
        /// Writes a debug entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public static void WriteDebug(string callerTypeName, string text)
        {
            Write(LogLevel.Debug, callerTypeName, text);
        }
        /// <summary>
        /// Writes a information entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public static void WriteInformation(object caller, string text)
        {
            Write(LogLevel.Information, caller, text);
        }
        /// <summary>
        /// Writes a information entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public static void WriteInformation(string callerTypeName, string text)
        {
            Write(LogLevel.Information, callerTypeName, text);
        }
        /// <summary>
        /// Writes a warning entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public static void WriteWarning(object caller, string text)
        {
            Write(LogLevel.Warning, caller, text);
        }
        /// <summary>
        /// Writes a warning entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public static void WriteWarning(string callerTypeName, string text)
        {
            Write(LogLevel.Warning, callerTypeName, text);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public static void WriteError(object caller, string text, Exception ex = null)
        {
            Write(LogLevel.Error, caller, text, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public static void WriteError(string callerTypeName, string text, Exception ex = null)
        {
            Write(LogLevel.Error, callerTypeName, text, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="ex">Exception</param>
        public static void WriteError(object caller, Exception ex)
        {
            Write(LogLevel.Error, caller, ex.Message, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="ex">Exception</param>
        public static void WriteError(string callerTypeName, Exception ex)
        {
            Write(LogLevel.Error, callerTypeName, ex.Message, ex);
        }
        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="logLevel">Log level</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public static void Write(LogLevel logLevel, object caller, string text, Exception ex = null)
        {
            Write(logLevel, caller?.GetType().Name, text, ex);
        }
        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="logLevel">Log level</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public static void Write(LogLevel logLevel, string callerTypeName, string text, Exception ex = null)
        {
            if (logLevel < LogLevel)
            {
                // Discard log entry
                return;
            }

            log.Enqueue(new LogEntry { EventDate = DateTime.Now, CallerTypeName = callerTypeName, LogLevel = logLevel, Text = text, Exception = ex });

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
            return Read(DefaultFilter, count);
        }
        /// <summary>
        /// Reads a list of entries from the log, of the specified log level or above
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="count">Number of entries, from the last one</param>
        public static IEnumerable<LogEntry> Read(LogLevel logLevel, int count = 0)
        {
            return Read(l => l.LogLevel >= logLevel, count);
        }
        /// <summary>
        /// Reads a list of entries from the log, of the specified log level or above
        /// </summary>
        /// <param name="predicate">Filter predicate function</param>
        /// <param name="count">Number of entries, from the last one</param>
        public static IEnumerable<LogEntry> Read(Func<LogEntry, bool> predicate, int count = 0)
        {
            var logEntries = predicate == null ? log.ToList() : log.Where(predicate).ToList();

            int take = count <= 0 ? logEntries.Count : Math.Min(count, logEntries.Count);
            int skip = Math.Max(0, logEntries.Count - take);

            return logEntries.Skip(skip).Take(take).ToArray();
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public static string ReadText(int maxLines = 0, bool reverse = true)
        {
            return ReadText(DefaultFilter, DefaultFormatter, maxLines, reverse);
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="fncFormat">Log line format function</param>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public static string ReadText(Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
        {
            return ReadText(DefaultFilter, fncFormat ?? DefaultFormatter, maxLines, reverse);
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="predicate">Filter predicate function</param>
        /// <param name="fncFormat">Log line format function</param>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public static string ReadText(Func<LogEntry, bool> predicate, Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
        {
            return ReadText(Read(predicate ?? DefaultFilter), fncFormat ?? DefaultFormatter, maxLines, reverse);
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="logEntries">Log entry list</param>
        /// <param name="fncFormat">Log line format function</param>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        private static string ReadText(IEnumerable<LogEntry> logEntries, Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
        {
            if (logEntries?.Any() != true)
            {
                return string.Empty;
            }

            if (reverse)
            {
                logEntries = logEntries.Reverse();
            }

            string logText = new string(logEntries.SelectMany(fncFormat ?? DefaultFormatter).ToArray());
            var lines = logText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (maxLines > 0 && lines.Length > maxLines)
            {
                lines = lines.Take(maxLines).ToArray();
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Sets a custom log entry filter
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        public static void SetCustomFilter(Func<LogEntry, bool> predicate)
        {
            filterFnc = predicate;
        }
        /// <summary>
        /// Sets a custom log entry formatter
        /// </summary>
        /// <param name="formatter">Formatter function</param>
        public static void SetDefaultFormatter(Func<LogEntry, string> formatter)
        {
            formatFnc = formatter;
        }
        /// <summary>
        /// Default log filter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private static bool DefaultFilter(LogEntry logEntry)
        {
            return filterFnc != null ? filterFnc(logEntry) : logEntry != null;
        }
        /// <summary>
        /// Default log line formatter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private static string DefaultFormatter(LogEntry logEntry)
        {
            return formatFnc != null ? formatFnc(logEntry) : $"{logEntry.EventDate:HH:mm:ss.fff} [{logEntry.LogLevel}]> {logEntry.Text}{Environment.NewLine}";
        }
        /// <summary>
        /// Sets the built-in log entry filter
        /// </summary>
        public static void SetDefaultFilter()
        {
            filterFnc = null;
        }
        /// <summary>
        /// Sets the built-in log entry formatter
        /// </summary>
        public static void SetDefaultFormatter()
        {
            formatFnc = null;
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
        public static void Dump(string fileName, Func<LogEntry, bool> predicate = null, Func<LogEntry, string> fncFormat = null)
        {
            string dumpText = ReadText(predicate ?? DefaultFilter, fncFormat ?? DefaultFormatter, 0, false);

            File.WriteAllText(fileName, dumpText);
        }
    }
}
