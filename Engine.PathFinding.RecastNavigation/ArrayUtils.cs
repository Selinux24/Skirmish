using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Array utils
    /// </summary>
    static class ArrayUtils
    {
        /// <summary>
        /// Gets the next index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the next index</returns>
        public static int Next(int i, int length)
        {
            return i + 1 < length ? i + 1 : 0;
        }
        /// <summary>
        /// Gets the previous index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the previous index</returns>
        public static int Prev(int i, int length)
        {
            return i - 1 >= 0 ? i - 1 : length - 1;
        }
        /// <summary>
        /// Pushes the specified item in front of the array
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="v">Item</param>
        /// <param name="arr">Array</param>
        /// <param name="an">Array size</param>
        public static void PushFront<T>(T v, T[] arr, ref int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = v;
        }
        /// <summary>
        /// Pushes the specified item int the array's back position
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="v">Item</param>
        /// <param name="arr">Array</param>
        /// <param name="an">Array size</param>
        public static void PushBack<T>(T v, T[] arr, ref int an)
        {
            arr[an] = v;
            an++;
        }
        /// <summary>
        /// Removes the item at index position in the specified array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="index">Start position</param>
        /// <param name="n">Number of items in the array</param>
        /// <returns>Returns the resulting array</returns>
        public static void RemoveAt<T>(T[] arr, int index, int n)
        {
            for (int i = index; i < n; i++)
            {
                arr[i] = arr[i + 1];
            }
        }
        /// <summary>
        /// Resets the array values
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array">Array to reset</param>
        /// <param name="count">Number of items to reset in the array</param>
        /// <param name="value">Value to set</param>
        public static void ResetArray<T>(T[] array, int count, T value)
        {
            ResetArray(array, 0, count, value);
        }
        /// <summary>
        /// Resets the array values
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array">Array to reset</param>
        /// <param name="start">Start index</param>
        /// <param name="count">Number of items to reset in the array</param>
        /// <param name="value">Value to set</param>
        public static void ResetArray<T>(T[] array, int start, int count, T value)
        {
            if (count <= 0)
            {
                return;
            }

            if (array?.Any() != true)
            {
                return;
            }

            if (array.Length > start + count)
            {
                return;
            }

            for (int i = start; i < count; i++)
            {
                array[i] = value;
            }
        }
    }
}
