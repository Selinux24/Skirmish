﻿using System;
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
        public static LoadResourceGroup FromTasks(Func<Task> task, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                tasks = [task],
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(Func<Task> task, Action<LoadResourcesResult> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                tasks = [task],
                actionCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(IEnumerable<Func<Task>> tasks, Action<LoadResourcesResult> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                tasks = tasks,
                actionCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(Func<Task> task, Func<LoadResourcesResult, Task> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                tasks = [task],
                funcCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(IEnumerable<Func<Task>> tasks, Func<LoadResourcesResult, Task> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup
            {
                tasks = tasks,
                funcCallback = callback,
                progressCallback = progressCallback,
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
        /// <summary>
        /// Progress callback
        /// </summary>
        private Action<LoadResourceProgress> progressCallback;
        /// <summary>
        /// Function list
        /// </summary>
        private IEnumerable<Func<Task>> tasks = [];

        /// <inheritdoc/>
        public string Id { get; set; } = null;

        /// <inheritdoc/>
        public async Task Process()
        {
            List<LoadTaskResult> loadResult = [];

            List<Task> taskList = new(tasks.Select(fnc => fnc.Invoke()));

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

                progressCallback?.Invoke(new() { Id = Id, Progress = ++currentTask / (float)totalTasks });
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
        public static LoadResourceGroup<T> FromTasks(Func<Task<T>> task, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                tasks = [task],
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(Func<Task<T>> task, Action<LoadResourcesResult<T>> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                tasks = [task],
                actionCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(IEnumerable<Func<Task<T>>> tasks, Action<LoadResourcesResult<T>> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                tasks = tasks,
                actionCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(Func<Task<T>> task, Func<LoadResourcesResult<T>, Task> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                tasks = [task],
                funcCallback = callback,
                progressCallback = progressCallback,
                Id = id,
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(IEnumerable<Func<Task<T>>> tasks, Func<LoadResourcesResult<T>, Task> callback, Action<LoadResourceProgress> progressCallback = null, string id = null)
        {
            return new LoadResourceGroup<T>
            {
                tasks = tasks,
                funcCallback = callback,
                progressCallback = progressCallback,
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
        /// <summary>
        /// Progress callback
        /// </summary>
        private Action<LoadResourceProgress> progressCallback;
        /// <summary>
        /// Task list
        /// </summary>
        private IEnumerable<Func<Task<T>>> tasks = [];

        /// <inheritdoc/>
        public string Id { get; set; } = null;

        /// <inheritdoc/>
        public async Task Process()
        {
            List<LoadTaskResult<T>> loadResult = [];

            List<Task<T>> taskList = new(tasks.Select(fnc => fnc.Invoke()));

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

                progressCallback?.Invoke(new() { Id = Id, Progress = ++currentTask / (float)totalTasks });
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
