using System;

namespace Engine
{
    /// <summary>
    /// Log entry
    /// </summary>
    public record LogEntry
    {
        /// <summary>
        /// Event date
        /// </summary>
        public DateTime EventDate { get; set; }
        /// <summary>
        /// Caller type name
        /// </summary>
        public string CallerTypeName { get; set; }
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Entry level
        /// </summary>
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}
