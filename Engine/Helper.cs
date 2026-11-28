using Engine.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading;

namespace Engine
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        #region Random

        /// <summary>
        /// Default random generator
        /// </summary>
        public static Random RandomGenerator { get; } = new();
        /// <summary>
        /// Gets a new random generator
        /// </summary>
        public static Random NewGenerator()
        {
            return new();
        }
        /// <summary>
        /// Gets a new random generator
        /// </summary>
        /// <param name="seed">Seed</param>
        public static Random NewGenerator(int seed)
        {
            return new(seed);
        }

        public static float NextSingle(this Random rand, float min, float max)
        {
            float v = rand.NextSingle();

            return float.Lerp(min, max, v);
        }
        public static Vector2 NextVector2(this Random rand, Vector2 min, Vector2 max)
        {
            Vector2 alpha = new(rand.NextSingle(), rand.NextSingle());

            return Vector2.Lerp(min, max, alpha);
        }
        public static Vector3 NextVector3(this Random rand, Vector3 min, Vector3 max)
        {
            Vector3 alpha = new(rand.NextSingle(), rand.NextSingle(), rand.NextSingle());

            return Vector3.Lerp(min, max, alpha);
        }
        public static Vector4 NextVector4(this Random rand, Vector4 min, Vector4 max)
        {
            Vector4 alpha = new(rand.NextSingle(), rand.NextSingle(), rand.NextSingle(), rand.NextSingle());

            return Vector4.Lerp(min, max, alpha);
        }

        #endregion

        #region Memory

        /// <summary>
        /// Swaps values
        /// </summary>
        /// <typeparam name="T">Type of values</typeparam>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        public static void Swap<T>(ref T left, ref T right)
        {
            (right, left) = (left, right);
        }
        /// <summary>
        /// Writes stream to memory
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream CopyToMemory(this Stream stream)
        {
            var ms = new MemoryStream();

            stream.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }
        /// <summary>
        /// Writes file to memory
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream CopyToMemory(this string fileName)
        {
            using var stream = File.OpenRead(fileName);
            return stream.CopyToMemory();
        }

        #endregion

        #region Security

        /// <summary>
        /// Create a deterministic, non-cryptographic xxHash64 string for the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the xxHash64 string of the specified buffer</returns>
        public static string GetMd5Sum(this byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            return XxHash64.HashToUInt64(buffer).ToString("X16");
        }
        /// <summary>
        /// Create the md5 sum string of the specified string
        /// </summary>
        /// <param name="content">String</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this string content)
        {
            byte[] tmp = new byte[content.Length * 2];
            Encoding.Unicode.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, tmp, 0, true);

            return tmp.GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified string list
        /// </summary>
        /// <param name="content">String list</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this IEnumerable<string> content)
        {
            string md5 = null;
            content
                .ToList()
                .ForEach(p => md5 += p.GetMd5Sum());

            return md5;
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the md5 sum string of the specified stream</returns>
        public static string GetMd5Sum(this MemoryStream stream)
        {
            return stream.ToArray().GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the md5 sum string of the specified stream list</returns>
        public static string GetMd5Sum(this IEnumerable<MemoryStream> streams)
        {
            string md5 = null;
            streams
                .ToList()
                .ForEach(p => md5 += p.GetMd5Sum());

            return md5;
        }

        #endregion

        #region Actions & Functions

        /// <summary>
        /// Executes the specified action a number of times
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="input">Input</param>
        /// <param name="retryCount">Retry count</param>
        public static void Retry<T>(Action<T> action, T input, int retryCount)
        {
            Retry(action, input, retryCount, TimeSpan.FromSeconds(0));
        }
        /// <summary>
        /// Executes the specified action a number of times
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="input">Input</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="delay">Delay between attempts</param>
        public static void Retry<T>(Action<T> action, T input, int retryCount, TimeSpan delay)
        {
            Exception lastEx = null;

            do
            {
                try
                {
                    action.Invoke(input);

                    break;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    retryCount--;
                }

                Thread.Sleep(delay);
            }
            while (retryCount > 0);

            if (lastEx != null)
            {
                throw lastEx;
            }
        }
        /// <summary>
        /// Executes the specified function a number of times
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="func">Function</param>
        /// <param name="input">Input</param>
        /// <param name="retryCount">Retry count</param>
        /// <returns>Returns the function execution result</returns>
        public static TResult Retry<T, TResult>(Func<T, TResult> func, T input, int retryCount)
        {
            return Retry(func, input, retryCount, TimeSpan.FromSeconds(0));
        }
        /// <summary>
        /// Executes the specified function a number of times
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="func">Function</param>
        /// <param name="input">Input</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="delay">Delay between attempts</param>
        /// <returns>Returns the function execution result</returns>
        public static TResult Retry<T, TResult>(Func<T, TResult> func, T input, int retryCount, TimeSpan delay)
        {
            Exception lastEx;

            do
            {
                try
                {
                    return func.Invoke(input);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    retryCount--;
                }

                Thread.Sleep(delay);
            }
            while (retryCount > 0);

            throw lastEx;
        }
        /// <summary>
        /// Executes the specified function a number of times
        /// </summary>
        /// <param name="func">Function</param>
        /// <param name="retryCount">Retry count</param>
        /// <returns>Returns the result of the function</returns>
        /// <remarks>The method exits on exceptions</remarks>
        public static bool Retry(Func<bool> func, int retryCount)
        {
            return Retry(func, retryCount, TimeSpan.FromSeconds(0));
        }
        /// <summary>
        /// Executes the specified function a number of times
        /// </summary>
        /// <param name="func">Function</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="delay">Delay between attempts</param>
        /// <returns>Returns the result of the function</returns>
        /// <remarks>The method exits on exceptions</remarks>
        public static bool Retry(Func<bool> func, int retryCount, TimeSpan delay)
        {
            int retry = retryCount;

            bool res;
            do
            {
                res = func();
                retry--;

                Thread.Sleep(delay);
            }
            while (retry > 0 && !res);

            return res;
        }

        #endregion

        #region Array Utils

        /// <summary>
        /// Creates an array initialized with sequential numbers
        /// </summary>
        /// <param name="n">Number of total values in the array</param>
        /// <param name="start">Start value</param>
        public static int[] CreateSequentialArray(int n, int start = 0)
        {
            int[] res = new int[n];
            for (int j = 0; j < n; ++j)
            {
                res[j] = start + j;
            }
            return res;
        }
        /// <summary>
        /// Generate an array initialized to defaultValue
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(uint length, T defaultValue = default) where T : unmanaged
        {
            T[] array = new T[length];

            InitializeArray(array, defaultValue);

            return array;
        }
        /// <summary>
        /// Generate an array initialized to defaultValue
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, T defaultValue = default) where T : unmanaged
        {
            T[] array = new T[length];

            InitializeArray(array, defaultValue);

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(uint length, Func<T> func)
        {
            T[] array = new T[length];

            InitializeArray(array, func);

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, Func<T> func)
        {
            T[] array = new T[length];

            InitializeArray(array, func);

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(uint length, Func<int, T> func)
        {
            T[] array = new T[length];

            InitializeArray(array, func);

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, Func<int, T> func)
        {
            T[] array = new T[length];

            InitializeArray(array, func);

            return array;
        }
        /// <summary>
        /// Initializes the specified array with value
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value to set to all array elements</param>
        public static void InitializeArray<T>(this T[] arr, T value) where T : unmanaged
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
        /// <summary>
        /// Initializes the specified array with function result
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="func">Function</param>
        public static void InitializeArray<T>(this T[] arr, Func<T> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = func.Invoke();
            }
        }
        /// <summary>
        /// Initializes the specified array with function result
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="func">Function</param>
        public static void InitializeArray<T>(this T[] arr, Func<int, T> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = func.Invoke(i);
            }
        }
        /// <summary>
        /// Gets the maximum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the maximum value of the collection</returns>
        public static T Max<T>(params T[] array) where T : IComparable<T>
        {
            T res = default;

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0 || res.CompareTo(array[i]) < 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the minimum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the minimum value of the collection</returns>
        public static T Min<T>(params T[] array) where T : IComparable<T>
        {
            T res = default;

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0 || res.CompareTo(array[i]) > 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
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
        /// Gets next pair of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next pair</returns>
        public static int NextPair(this int num)
        {
            return !MathUtils.NearEqual(num * 0.5f, (int)(num * 0.5f)) ? num + 1 : num;
        }
        /// <summary>
        /// Gets next odd of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next odd</returns>
        public static int NextOdd(this int num)
        {
            return !MathUtils.NearEqual(num * 0.5f, (int)(num * 0.5f)) ? num : num + 1;
        }
        /// <summary>
        /// Calculates the next highest power of two.
        /// </summary>
        /// <remarks>
        /// This is a minimal method meant to be fast. There is a known edge case where an input of 0 will output 0
        /// instead of the mathematically correct value of 1. It will not be fixed.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The next power of two after the value.</returns>
        public static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }
        /// <summary>
        /// Concatenates the members of a collection of type T, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="list">Collection</param>
        /// <param name="separator">The string to use as a separator</param>
        /// <returns>A string that consists of the members of values delimited by the separator string</returns>
        public static string Join<T>(this IEnumerable<T> list, string separator = "")
        {
            var res = list.Select(a => $"{a}");

            return string.Join(separator, res);
        }
        /// <summary>
        /// Removes the first item of the list
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="list">The list</param>
        /// <returns>Returns the removed item</returns>
        public static T PopFirst<T>(this List<T> list)
        {
            T value = list[0];
            list.RemoveAt(0);
            return value;
        }
        /// <summary>
        /// Removes the last item of the list
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="list">The list</param>
        /// <returns>Returns the removed item</returns>
        public static T PopLast<T>(this List<T> list)
        {
            int index = list.Count - 1;
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }
        /// <summary>
        /// Compares two lists, element by element
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <returns>Returns true if both lists contains the same elements</returns>
        public static bool CompareLists<T>(List<T> enum1, List<T> enum2)
        {
            if (ReferenceEquals(enum1, enum2)) return true;
            if (enum1 is null || enum2 is null) return false;
            if (enum1.Count != enum2.Count) return false;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < enum1.Count; i++)
            {
                if (!comparer.Equals(enum1[i], enum2[i]))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Compares two arrays, element by element
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <returns>Returns true if both arrays contains the same elements</returns>
        public static bool CompareArrays<T>(T[] enum1, T[] enum2)
        {
            if (ReferenceEquals(enum1, enum2)) return true;
            if (enum1 is null || enum2 is null) return false;
            if (enum1.Length != enum2.Length) return false;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < enum1.Length; i++)
            {
                if (!comparer.Equals(enum1[i], enum2[i]))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Truncates the spefied array
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array">Array</param>
        /// <param name="count">Number of items</param>
        public static T[] Truncate<T>(T[] array, int count)
        {
            if (array == null)
            {
                return [];
            }

            if (array.Length <= count)
            {
                //Copy array
                return [.. array];
            }

            return [.. array.Take(count)];
        }
        /// <summary>
        /// Returns a distinct array from the input array.
        /// </summary>
        /// <typeparam name="T">Array type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Returns a new array, with the distinct values</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T[] SortDistinct<T>(T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Length == 0) return [];

            if (array.Length == 1) return [array[0]];

            T[] cpy = new T[array.Length];
            Array.Copy(array, cpy, array.Length);
            Array.Sort(cpy);

            List<T> distinctElements = [cpy[0]];
            for (int i = 1; i < cpy.Length; i++)
            {
                if (!cpy[i].Equals(cpy[i - 1]))
                {
                    distinctElements.Add(cpy[i]);
                }
            }

            T[] result = new T[distinctElements.Count];
            distinctElements.CopyTo(result, 0);
            return result;
        }
        /// <summary>
        /// Returns a distinct array from the input array.
        /// </summary>
        /// <typeparam name="T">Array type</typeparam>
        /// <param name="array">Array</param>
        /// <returns>Returns a new array, with the distinct values</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T[] Distinct<T>(T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Length == 0) return [];

            if (array.Length == 1) return [array[0]];

            List<T> distinctElements = [];
            HashSet<T> seen = [];
            foreach (T item in array)
            {
                if (seen.Contains(item))
                {
                    continue;
                }

                seen.Add(item);
                distinctElements.Add(item);
            }

            T[] result = new T[distinctElements.Count];
            distinctElements.CopyTo(result, 0);
            return result;
        }
        /// <summary>
        /// Returns a new array that contains the specified number of elements from the start of the input array.
        /// </summary>
        /// <typeparam name="T">Array type</typeparam>
        /// <param name="array">Array</param>
        /// <param name="count">Count</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T[] Take<T>(T[] array, int count)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");

            if (count > array.Length) count = array.Length;

            T[] result = new T[count];
            Array.Copy(array, result, count);
            return result;
        }

        #endregion

        #region Stacks

        /// <summary>
        /// Pushes an array of items to the top of the stack
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="stack">Stack</param>
        /// <param name="values">List of items</param>
        public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> values)
        {
            if (values?.Any() != true)
            {
                return;
            }

            foreach (var item in values)
            {
                stack.Push(item);
            }
        }

        #endregion

        #region Concurrent Utils

        /// <summary>
        /// Clears a concurrent bag list
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Concurrent bag</param>
        public static void Clear<T>(this ConcurrentBag<T> source)
        {
            while (!source.IsEmpty)
            {
                source.TryTake(out T _);
            }
        }

        #endregion

        #region Conversion

        public static Vortice.RawRect ToRawRect(this RectI value)
        {
            return new(value.Left, value.Top, value.Right, value.Bottom);
        }
        public static Vortice.RawRect ToRawRect(this Rect value)
        {
            return new((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
        }

        public static RectI ToRectI(this Vortice.RawRect value)
        {
            return new(value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top);
        }
        public static Vortice.Mathematics.RectI ToRectI(this RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        public static Vortice.Mathematics.RectI ToRectI(this Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }

        public static Rect ToRect(this Vortice.RawRect value)
        {
            return new(value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top);
        }
        public static Vortice.Mathematics.Rect ToRect(this RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        public static Vortice.Mathematics.Rect ToRect(this Rect value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        public static Vortice.Mathematics.Int2 ToInt2(this Vector2I Value)
        {
            return new(Value.X, Value.Y);
        }

        public static Vortice.Mathematics.Int3 ToInt3(this Vector3I Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }

        public static Vector2I ToVector2I(this Vortice.Mathematics.SizeI Value)
        {
            return new(Value.Width, Value.Height);
        }
        public static Vector2I ToVector2I(this Vortice.Mathematics.Int2 Value)
        {
            return new(Value.X, Value.Y);
        }

        public static Vector3I ToVector3I(this Vortice.Mathematics.Size3 Value)
        {
            return new(Value.Width, Value.Height, Value.Depth);
        }
        public static Vector3I ToVector3I(this Vortice.Mathematics.Int3 Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }

        public static Color ToColor(this Vortice.Mathematics.Color4 value)
        {
            return new(value.R, value.G, value.B, value.A);
        }

        public static Color3 ToColor3(this Vortice.Mathematics.Color3 value)
        {
            return new(value.R, value.G, value.B);
        }
        public static Vortice.Mathematics.Color3 ToColor3(this Color3 value)
        {
            return new(value.Red, value.Green, value.Blue);
        }

        public static Color4 ToColor4(this Vortice.Mathematics.Color4 value)
        {
            return new(value.R, value.G, value.B, value.A);
        }
        public static Vortice.Mathematics.Color4 ToColor4(this Color value)
        {
            return new(
                value.R / 255f,
                value.G / 255f,
                value.B / 255f,
                value.A / 255f);
        }
        public static Vortice.Mathematics.Color4 ToColor4(this Color4 value)
        {
            return new(value.Red, value.Green, value.Blue, value.Alpha);
        }

        public static Vortice.Mathematics.Size ToSize(this Vector2 Value)
        {
            return new(Value.X, Value.Y);
        }

        public static Vortice.Mathematics.SizeI ToSizeI(this Vector2I Value)
        {
            return new(Value.X, Value.Y);
        }

        public static Vortice.Mathematics.Size3 ToSize3(this Vector3I Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }

        public static Vector2 ToVector2(this Vortice.Mathematics.Size Value)
        {
            return new(Value.Width, Value.Height);
        }

        #endregion
    }
}
