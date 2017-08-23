using System;

namespace Engine.Collections
{
    /// <summary>
    /// Fixed array
    /// </summary>
    /// <typeparam name="T">The type for the elements of the array</typeparam>
    public class FixedArray<T>
    {
        /// <summary>
        /// Internal array
        /// </summary>
        public T[] Array { get; private set; }
        /// <summary>
        /// Maximum items in the array
        /// </summary>
        public int Max { get; private set; }
        /// <summary>
        /// Current items in the array
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Gets the items in the array by index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Returns the item in the array by index</returns>
        public T this[int index]
        {
            get
            {
                return this.Array[index];
            }
            set
            {
                this.Array[index] = value;
            }
        }
        /// <summary>
        /// Gets the first item in the array
        /// </summary>
        public T First
        {
            get
            {
                return this.Array[0];
            }
        }
        /// <summary>
        /// Gets the last item in the array
        /// </summary>
        public T Last
        {
            get
            {
                return this.Array[this.Count - 1];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="max">Maximum items in the array</param>
        public FixedArray(int max)
        {
            this.Array = new T[max];
            this.Max = max;
            this.Count = 0;
        }

        /// <summary>
        /// Adds a new item to the array
        /// </summary>
        /// <param name="value">The new item</param>
        /// <returns>Returns true if the item is added</returns>
        public bool Add(T value)
        {
            if (this.Count < this.Max)
            {
                this.Array[this.Count++] = value;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Inserts a value in the specified position
        /// </summary>
        /// <param name="index">Item index position</param>
        /// <param name="value">Value to insert</param>
        public void InsertAt(int index, T value)
        {
            int tgt = index + 1;
            int n = Math.Min(this.Count - index, this.Max - tgt);

            if (n > 0)
            {
                for (int j = 0; j < n; j++)
                {
                    this[tgt + j] = this[index + j];
                }
            }

            this[index] = value;
        }
        /// <summary>
        /// Clears the array
        /// </summary>
        public void Clear()
        {
            this.Count = 0;
        }
    }
}
