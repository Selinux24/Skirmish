using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Load resource group
    /// </summary>
    public class LoadResourceGroup : ILoadResourceGroup
    {
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(Task task, Action<LoadResourcesResult> callback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                Tasks = [task],
                actionCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(IEnumerable<Task> tasks, Action<LoadResourcesResult> callback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                Tasks = [.. tasks],
                actionCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(Task task, Func<LoadResourcesResult, Task> callback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                Tasks = [task],
                funcCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(IEnumerable<Task> tasks, Func<LoadResourcesResult, Task> callback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                Tasks = [.. tasks],
                funcCallback = callback,
                Id = id,
            };
        }

        /// <summary>
        /// Task result
        /// </summary>
        private LoadResourcesResult taskResult;
        /// <summary>
        /// Callback action
        /// </summary>
        private Action<LoadResourcesResult> actionCallback;
        /// <summary>
        /// Callback function
        /// </summary>
        private Func<LoadResourcesResult, Task> funcCallback;

        /// <inheritdoc/>
        public string Id { get; set; } = null;
        /// <summary>
        /// Task list
        /// </summary>
        public IEnumerable<Task> Tasks { get; set; } = [];

        /// <inheritdoc/>
        public async Task Process(IProgress<LoadResourceProgress> progress)
        {
            List<TaskResult> loadResult = [];

            var taskList = Tasks.ToList();

            int totalTasks = taskList.Count;
            int currentTask = 0;
            while (taskList.Count != 0)
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                loadResult.Add(new()
                {
                    Completed = t.Status == TaskStatus.RanToCompletion,
                    Exception = t.Exception, // Store the excetion
                });

                progress?.Report(new() { Id = Id, Progress = ++currentTask / (float)totalTasks });
            }

            taskResult = new()
            {
                Results = loadResult,
            };
        }
        /// <inheritdoc/>
        public void End()
        {
            actionCallback?.Invoke(taskResult);
            funcCallback?.Invoke(taskResult);
        }
    }

    /// <summary>
    /// Load resource group
    /// </summary>
    /// <typeparam name="T">Task result type</typeparam>
    public class LoadResourceGroup<T> : ILoadResourceGroup
    {
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(Task<T> task, Action<LoadResourcesResult<T>> callback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = [task],
                actionCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(IEnumerable<Task<T>> tasks, Action<LoadResourcesResult<T>> callback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = [.. tasks],
                actionCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(Task<T> task, Func<LoadResourcesResult<T>, Task> callback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = [task],
                funcCallback = callback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(IEnumerable<Task<T>> tasks, Func<LoadResourcesResult<T>, Task> callback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = [.. tasks],
                funcCallback = callback,
                Id = id,
            };
        }

        /// <summary>
        /// Task result
        /// </summary>
        private LoadResourcesResult<T> taskResult;
        /// <summary>
        /// Callback action
        /// </summary>
        private Action<LoadResourcesResult<T>> actionCallback;
        /// <summary>
        /// Callback function
        /// </summary>
        private Func<LoadResourcesResult<T>, Task> funcCallback;

        /// <inheritdoc/>
        public string Id { get; set; } = null;
        /// <summary>
        /// Task list
        /// </summary>
        public IEnumerable<Task<T>> Tasks { get; set; } = [];

        /// <inheritdoc/>
        public async Task Process(IProgress<LoadResourceProgress> progress)
        {
            List<TaskResult<T>> loadResult = [];

            var taskList = Tasks.ToList();

            int totalTasks = taskList.Count;
            int currentTask = 0;
            while (taskList.Count != 0)
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                bool completedOk = t.Status == TaskStatus.RanToCompletion;

                loadResult.Add(new()
                {
                    Completed = completedOk,
                    Exception = t.Exception, // Store the excetion
                    Result = completedOk ? (await t) : default, // Avoid throwing the exception now
                });

                progress?.Report(new() { Id = Id, Progress = ++currentTask / (float)totalTasks });
            }

            taskResult = new()
            {
                Results = loadResult,
            };
        }
        /// <inheritdoc/>
        public void End()
        {
            actionCallback?.Invoke(taskResult);
            funcCallback?.Invoke(taskResult);
        }
    }
}
