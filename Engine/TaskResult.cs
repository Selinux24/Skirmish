using System;

namespace Engine
{
    /// <summary>
    /// Task result
    /// </summary>
    public class TaskResult
    {
        /// <summary>
        /// Gets whether the task completed correctly or not
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// Exception result
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Task result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    public class TaskResult<T> : TaskResult
    {
        /// <summary>
        /// Result
        /// </summary>
        public T Result { get; set; }
    }
}
