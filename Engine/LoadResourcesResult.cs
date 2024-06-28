using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public virtual IEnumerable<LoadTaskResult> Results { get; set; }
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
            var aggregate = Flatten();
            if (aggregate == null)
            {
                return;
            }

            throw aggregate;
        }
        /// <summary>
        /// Gets the aggregate exception, if any
        /// </summary>
        public virtual AggregateException Flatten()
        {
            var exList = GetExceptions();
            if (!exList.Any())
            {
                return null;
            }

            var aggregate = new AggregateException($"A load resource task list results in error.", exList);

            return aggregate.Flatten();
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

            return exList ?? [];
        }
        /// <summary>
        /// Gets a string with the listed exceptions
        /// </summary>
        public string GetErrorMessage()
        {
            var exList = GetExceptions();
            if (!exList.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var ex in exList)
            {
#if DEBUG
                sb.Append(ex.ToString());
#else
                sb.Append(ex.Message);
#endif
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Completed ? "Completed" : GetErrorMessage();
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
        public new IEnumerable<LoadTaskResult<T>> Results { get; set; }
    }
}
