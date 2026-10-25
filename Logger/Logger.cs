using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Logger
{
    /// <summary>
    /// Logger helper
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Log entry array
        /// </summary>
        private LogEntry[] log = new LogEntry[100];
        /// <summary>
        /// Current log entry index
        /// </summary>
        private int logIndex = 0;
        /// <summary>
        /// Log stack size
        /// </summary>
        private int logStackSize = 100;

        /// <summary>
        /// Custom filter function
        /// </summary>
        private Func<LogEntry, bool> filterFnc;
        /// <summary>
        /// Custom format function
        /// </summary>
        private Func<LogEntry, string> formatFnc;
        /// <summary>
        /// Log level
        /// </summary>
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// Enables the console log
        /// </summary>
        public bool EnableConsole { get; set; } = false;
        /// <summary>
        /// Console log level
        /// </summary>
        public LogLevel ConsoleLogLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Constructor
        /// </summary>
        public Log()
        {
#if DEBUG
            LogLevel = LogLevel.Debug;
#else
            LogLevel = LogLevel.Information;
#endif
        }

        /// <summary>
        /// Sets the log stack size
        /// </summary>
        /// <param name="stackSize">Stack size</param>
        public void SetStackSize(int stackSize)
        {
            if (stackSize <= 0)
            {
                return;
            }

            if (stackSize == logStackSize)
            {
                return;
            }

            logStackSize = stackSize;
            Array.Resize(ref log, logStackSize);
            logIndex = Math.Min(logIndex, logStackSize);
        }

        /// <summary>
        /// Writes a trace entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public void WriteTrace(object caller, string text)
        {
            Write(LogLevel.Trace, caller, text);
        }
        /// <summary>
        /// Writes a trace entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public void WriteTrace(string callerTypeName, string text)
        {
            Write(LogLevel.Trace, callerTypeName, text);
        }
        /// <summary>
        /// Writes a debug entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public void WriteDebug(object caller, string text)
        {
            Write(LogLevel.Debug, caller, text);
        }
        /// <summary>
        /// Writes a debug entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public void WriteDebug(string callerTypeName, string text)
        {
            Write(LogLevel.Debug, callerTypeName, text);
        }
        /// <summary>
        /// Writes a information entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public void WriteInformation(object caller, string text)
        {
            Write(LogLevel.Information, caller, text);
        }
        /// <summary>
        /// Writes a information entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public void WriteInformation(string callerTypeName, string text)
        {
            Write(LogLevel.Information, callerTypeName, text);
        }
        /// <summary>
        /// Writes a warning entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        public void WriteWarning(object caller, string text)
        {
            Write(LogLevel.Warning, caller, text);
        }
        /// <summary>
        /// Writes a warning entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        public void WriteWarning(string callerTypeName, string text)
        {
            Write(LogLevel.Warning, callerTypeName, text);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public void WriteError(object caller, string text, Exception ex = null)
        {
            Write(LogLevel.Error, caller, text, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="text">Entry text</param>
        /// <param name="ex">Exception (optional)</param>
        public void WriteError(string callerTypeName, string text, Exception ex = null)
        {
            Write(LogLevel.Error, callerTypeName, text, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="caller">Caller</param>
        /// <param name="ex">Exception</param>
        public void WriteError(object caller, Exception ex)
        {
            Write(LogLevel.Error, caller, ex.Message, ex);
        }
        /// <summary>
        /// Writes a error entry
        /// </summary>
        /// <param name="callerTypeName">Caller type name</param>
        /// <param name="ex">Exception</param>
        public void WriteError(string callerTypeName, Exception ex)
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
        public void Write(LogLevel logLevel, object caller, string text, Exception ex = null)
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
        public void Write(LogLevel logLevel, string callerTypeName, string text, Exception ex = null)
        {
            if (logLevel < LogLevel)
            {
                // Discard log entry
                return;
            }

            var logEntry = log[logIndex] ?? new LogEntry();
            logEntry.EventDate = DateTime.Now;
            logEntry.CallerTypeName = callerTypeName;
            logEntry.LogLevel = logLevel;
            logEntry.Text = text;
            logEntry.Exception = ex;

            log[logIndex] = logEntry;

            if (logLevel >= ConsoleLogLevel)
            {
                // Console logger
                Console.Write(DefaultFormatter(logEntry));
            }

            logIndex++;
            logIndex %= logStackSize;
        }

        /// <summary>
        /// Gets whether the log has any errors or not
        /// </summary>
        public bool HasErrors()
        {
            return Array.Exists(log, l => l?.LogLevel == LogLevel.Error);
        }

        /// <summary>
        /// Reads a list of entries from the log
        /// </summary>
        /// <param name="count">Number of entries, from the last one</param>
        public IEnumerable<LogEntry> Read(int count = 0)
        {
            return Read(DefaultFilter, count);
        }
        /// <summary>
        /// Reads a list of entries from the log, of the specified log level or above
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="count">Number of entries, from the last one</param>
        public IEnumerable<LogEntry> Read(LogLevel logLevel, int count = 0)
        {
            return Read(l => l.LogLevel >= logLevel, count);
        }
        /// <summary>
        /// Reads a list of entries from the log, of the specified log level or above
        /// </summary>
        /// <param name="predicate">Filter predicate function</param>
        /// <param name="count">Number of entries, from the last one</param>
        public IEnumerable<LogEntry> Read(Func<LogEntry, bool> predicate, int count = 0)
        {
            var logCopy = log.ToArray();
            logCopy = logCopy
                .Skip(logIndex).Take(logStackSize - logIndex)
                .Concat(logCopy.Take(logIndex))
                .Where(l => l != null)
                .ToArray();

            var logEntries = predicate == null ? logCopy : logCopy.Where(predicate).ToArray();

            int take = count <= 0 ? logEntries.Length : Math.Min(count, logEntries.Length);
            int skip = Math.Max(0, logEntries.Length - take);

            return logEntries.Skip(skip).Take(take).ToArray();
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public string ReadText(int maxLines = 0, bool reverse = true)
        {
            return ReadText(DefaultFilter, DefaultFormatter, maxLines, reverse);
        }
        /// <summary>
        /// Gets the last maxLines lines of the log
        /// </summary>
        /// <param name="fncFormat">Log line format function</param>
        /// <param name="maxLines">Maximum number of lines</param>
        /// <param name="reverse">Reverse entry order</param>
        public string ReadText(Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
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
        public string ReadText(Func<LogEntry, bool> predicate, Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
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
        private string ReadText(IEnumerable<LogEntry> logEntries, Func<LogEntry, string> fncFormat, int maxLines = 0, bool reverse = true)
        {
            if (logEntries?.Any() != true)
            {
                return string.Empty;
            }

            if (reverse)
            {
                logEntries = logEntries.Reverse();
            }

            var fmt = fncFormat ?? DefaultFormatter;

            StringBuilder logText = new();
            int lineCount = logEntries.Count();
            lineCount = maxLines > 0 ? Math.Min(lineCount, maxLines) : lineCount;
            for (int i = 0; i < lineCount; i++)
            {
                logText.Append(fmt(logEntries.ElementAt(i)));
            }

            return logText.ToString();
        }

        /// <summary>
        /// Sets a custom log entry filter
        /// </summary>
        /// <param name="predicate">Filter predicate</param>
        public void SetCustomFilter(Func<LogEntry, bool> predicate)
        {
            filterFnc = predicate;
        }
        /// <summary>
        /// Sets a custom log entry formatter
        /// </summary>
        /// <param name="formatter">Formatter function</param>
        public void SetCustomFormatter(Func<LogEntry, string> formatter)
        {
            formatFnc = formatter;
        }
        /// <summary>
        /// Default log filter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private bool DefaultFilter(LogEntry logEntry)
        {
            return filterFnc != null ? filterFnc(logEntry) : logEntry != null;
        }
        /// <summary>
        /// Default log line formatter
        /// </summary>
        /// <param name="logEntry">Log entry</param>
        private string DefaultFormatter(LogEntry logEntry)
        {
            return formatFnc != null ? formatFnc(logEntry) : $"{logEntry.EventDate:HH:mm:ss.fff} [{logEntry.LogLevel}]> {logEntry.Text}{Environment.NewLine}";
        }
        /// <summary>
        /// Sets the built-in log entry filter
        /// </summary>
        public void SetDefaultFilter()
        {
            filterFnc = null;
        }
        /// <summary>
        /// Sets the built-in log entry formatter
        /// </summary>
        public void SetDefaultFormatter()
        {
            formatFnc = null;
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public void Clear()
        {
            logIndex = 0;
            for (int i = 0; i < log.Length; i++)
            {
                log[i] = null;
            }
        }

        /// <summary>
        /// Dumps the log to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Dump(string fileName, Func<LogEntry, bool> predicate = null, Func<LogEntry, string> fncFormat = null)
        {
            string dumpText = ReadText(predicate ?? DefaultFilter, fncFormat ?? DefaultFormatter, 0, false);

            File.WriteAllText(fileName, dumpText);
        }

        /// <summary>
        /// Gets the log level base on frame time
        /// </summary>
        /// <param name="frameTime">Frame time</param>
        /// <returns>Returns the log level</returns>
        public static LogLevel EvaluateTime(long frameTime)
        {
            if (frameTime > 500) return LogLevel.Warning;
            if (frameTime > 30) return LogLevel.Information;
            else return LogLevel.Debug;
        }
    }
}
