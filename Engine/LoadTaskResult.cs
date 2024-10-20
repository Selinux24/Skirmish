﻿using System;

namespace Engine
{
    /// <summary>
    /// Task result
    /// </summary>
    public class LoadTaskResult
    {
        /// <summary>
        /// Gets whether the task completed correctly or not
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// Exception result
        /// </summary>
        public Exception Exception { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Completed ? "Completed" : Exception?.Message ?? "Error";
        }
    }

    /// <summary>
    /// Task result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    public class LoadTaskResult<T> : LoadTaskResult
    {
        /// <summary>
        /// Result
        /// </summary>
        public T Result { get; set; }
    }
}
