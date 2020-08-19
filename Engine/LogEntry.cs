using System;

namespace Engine
{
    /// <summary>
    /// Log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Event date
        /// </summary>
        public DateTime EventDate { get; set; }
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Entry level
        /// </summary>
        public LogLevel LogLevel { get; set; }
    }
}
