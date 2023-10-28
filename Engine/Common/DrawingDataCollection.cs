using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Drawing data collection
    /// </summary>
    public class DrawingDataCollection<T> : IEnumerable<(string Name, T Value)>
    {
        /// <summary>
        /// Internal list
        /// </summary>
        private readonly List<(string Name, T Value)> list = new();

        /// <summary>
        /// Gets the value by name
        /// </summary>
        /// <param name="name">Name</param>
        public T this[string name]
        {
            get
            {
                return GetValue(name);
            }
            set
            {
                SetValue(name, value);
            }
        }
        /// <summary>
        /// Gets the item count
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Adds or updates a value to the collection
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public void SetValue(string name, T value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var current = list.FindIndex(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (current < 0)
            {
                list.Add((name, value));

                return;
            }

            list[current] = (name, value);
        }
        /// <summary>
        /// Gets the value with the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public T GetValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return default;
            }

            int index = list.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return default;
            }

            return list[index].Value;
        }
        /// <summary>
        /// Gets the internal name list
        /// </summary>
        public string[] GetNames()
        {
            return list.Select(m => m.Name).ToArray();
        }
        /// <summary>
        /// Gets the internal value list
        /// </summary>
        public IEnumerable<T> GetValues()
        {
            return list.Select(m => m.Value).ToArray();
        }
        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <inheritdoc/>
        public IEnumerator<(string Name, T Value)> GetEnumerator()
        {
            return list.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{GetNames().Join("|")}";
        }
    }
}
