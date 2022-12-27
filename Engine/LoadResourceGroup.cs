using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Load resource group
    /// </summary>
    public class LoadResourceGroup
    {
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(Task task)
        {
            return new LoadResourceGroup
            {
                Tasks = new[] { task }
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(IEnumerable<Task> tasks)
        {
            return new LoadResourceGroup
            {
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(params Task[] tasks)
        {
            return new LoadResourceGroup
            {
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup FromTasks(string id, Task task)
        {
            return new LoadResourceGroup
            {
                Id = id,
                Tasks = new[] { task }
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(string id, IEnumerable<Task> tasks)
        {
            return new LoadResourceGroup
            {
                Id = id,
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup FromTasks(string id, params Task[] tasks)
        {
            return new LoadResourceGroup
            {
                Id = id,
                Tasks = tasks
            };
        }

        /// <summary>
        /// Group identifier
        /// </summary>
        public string Id { get; set; } = null;
        /// <summary>
        /// Task list
        /// </summary>
        public IEnumerable<Task> Tasks { get; set; } = Array.Empty<Task>();
    }

    /// <summary>
    /// Load resource group
    /// </summary>
    /// <typeparam name="T">Task result type</typeparam>
    public class LoadResourceGroup<T>
    {
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(Task<T> task)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = new[] { task }
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(IEnumerable<Task<T>> tasks)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(params Task<T>[] tasks)
        {
            return new LoadResourceGroup<T>
            {
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(string id, Task<T> task)
        {
            return new LoadResourceGroup<T>
            {
                Id = id,
                Tasks = new[] { task }
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(string id, IEnumerable<Task<T>> tasks)
        {
            return new LoadResourceGroup<T>
            {
                Id = id,
                Tasks = tasks
            };
        }
        /// <summary>
        /// Creates a load resource group from a task list
        /// </summary>
        public static LoadResourceGroup<T> FromTasks(string id, params Task<T>[] tasks)
        {
            return new LoadResourceGroup<T>
            {
                Id = id,
                Tasks = tasks
            };
        }

        /// <summary>
        /// Group identifier
        /// </summary>
        public string Id { get; set; } = null;
        /// <summary>
        /// Task list
        /// </summary>
        public IEnumerable<Task<T>> Tasks { get; set; } = Array.Empty<Task<T>>();
    }
}
