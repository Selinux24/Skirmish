﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Load resource result
    /// </summary>
    public class LoadResourcesResult
    {
        /// <summary>
        /// Task result list
        /// </summary>
        public virtual IEnumerable<TaskResult> Results { get; set; }
        /// <summary>
        /// Gets whether all tasks completed correctly or not
        /// </summary>
        public virtual bool Completed
        {
            get
            {
                return Results?.Any(r => !r.Completed) != true;
            }
        }

        /// <summary>
        /// Throw exceptions, if any
        /// </summary>
        public virtual void ThrowExceptions()
        {
            var exList = GetExceptions();
            if (!exList.Any())
            {
                return;
            }

            var aggregate = new AggregateException($"A load resource task list results in error.", exList);

            throw aggregate.Flatten();
        }
        /// <summary>
        /// Gets a list of exception results, if any
        /// </summary>
        /// <returns>Returns a list of exceptions</returns>
        public IEnumerable<Exception> GetExceptions()
        {
            var exList = Results?
                .Where(r => r.Exception != null)
                .Select(r => r.Exception)
                .ToArray();

            return exList ?? new Exception[] { };
        }
    }

    /// <summary>
    /// Load resource result
    /// </summary>
    /// <typeparam name="T">Type of result</typeparam>
    public class LoadResourcesResult<T> : LoadResourcesResult
    {
        /// <summary>
        /// Task result list
        /// </summary>
        public new IEnumerable<TaskResult<T>> Results { get; set; }
    }
}
