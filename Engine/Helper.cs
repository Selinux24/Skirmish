using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// One radian
        /// </summary>
        public const float Radian = 0.0174532924f;
        /// <summary>
        /// Zero tolerance value
        /// </summary>
        public const float ZeroTolerance = Vortice.Mathematics.MathHelper.ZeroTolerance;
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector2 ZeroToleranceVector2 = new(ZeroTolerance);
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector3 ZeroToleranceVector3 = new(ZeroTolerance);
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector4 ZeroToleranceVector4 = new(ZeroTolerance);

        public const float Pi = Vortice.Mathematics.MathHelper.Pi;
        public const float TwoPi = Vortice.Mathematics.MathHelper.TwoPi;
        public const float PiOver2 = Vortice.Mathematics.MathHelper.PiOver2;
        public const float PiOver4 = Vortice.Mathematics.MathHelper.PiOver4;

        #region Maths

        public static bool IsZero(float a, float epsilon = ZeroTolerance)
        {
            return Math.Abs(a) < epsilon;
        }
        public static bool IsOne(float a)
        {
            return IsZero(a - 1.0f);
        }
        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon"/> method). See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in 
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>. 
        /// </remarks>
        public unsafe static bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if ((aInt < 0) != (bInt < 0))
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return (ulp <= maxUlp);
        }
        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns><c>true</c> if a almost equal to b within a float epsilon, <c>false</c> otherwise</returns>
        public static bool WithinEpsilon(float a, float b, float epsilon)
        {
            float num = a - b;
            return ((-epsilon <= num) && (num <= epsilon));
        }

        public static float Mod(float value, float mod)
        {
            if (mod == 0f) return value;

            return value % mod;
        }

        public static float DegreesToRadians(float d)
        {
            return Vortice.Mathematics.MathHelper.ToRadians(d);
        }
        public static float RadiansToDegrees(float d)
        {
            return Vortice.Mathematics.MathHelper.ToDegrees(d);
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmoothStep(float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * (3 - (2 * amount));
        }

        #endregion

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
        /// Create the md5 sum string of the specified buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the md5 sum string of the specified buffer</returns>
        public static string GetMd5Sum(this byte[] buffer)
        {
            byte[] result = null;
            result = MD5.HashData(buffer);

            var sb = new StringBuilder();
            Array.ForEach(result, r => sb.Append(r.ToString("X2")));
            return sb.ToString();
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
            return !NearEqual(num * 0.5f, (int)(num * 0.5f)) ? num + 1 : num;
        }
        /// <summary>
        /// Gets next odd of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next odd</returns>
        public static int NextOdd(this int num)
        {
            return !NearEqual(num * 0.5f, (int)(num * 0.5f)) ? num : num + 1;
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
        /// Compares two enumerable lists, element by element
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <returns>Returns true if both enumerables contains the same elements</returns>
        public static bool CompareEnumerables<T>(this IEnumerable<T> enum1, IEnumerable<T> enum2)
        {
            if (enum1 == null && enum2 == null)
            {
                return true;
            }

            if (!(enum1 != null && enum2 != null))
            {
                return false;
            }

            if (enum1.Count() != enum2.Count())
            {
                return false;
            }

            if (!CompareEnumerableElements(enum1, enum2))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Compares two enumerables item by item
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First enumerable list</param>
        /// <param name="enum2">Second enumerable list</param>
        /// <returns>Returns true if both enumerables contains the same elements</returns>
        private static bool CompareEnumerableElements<T>(IEnumerable<T> enum1, IEnumerable<T> enum2)
        {
            for (int i = 0; i < enum1.Count(); i++)
            {
                var item1 = enum1.ElementAt(i);
                var item2 = enum2.ElementAt(i);

                if (Equals(item1, default(T)) && Equals(item2, default(T)))
                {
                    continue;
                }

                if (item1?.Equals(item2) != true)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Gets the minimum and maximum values
        /// </summary>
        /// <param name="value1">Value 1</param>
        /// <param name="value2">Value 2</param>
        /// <param name="min">Returns the minimum value</param>
        /// <param name="max">Returns the maximum value</param>
        public static void MinMax(float value1, float value2, out float min, out float max)
        {
            if (value1 < value2)
            {
                min = value1;
                max = value2;
            }
            else
            {
                min = value2;
                max = value1;
            }
        }
        /// <summary>
        /// Gets the minimum and maximum values
        /// </summary>
        /// <param name="value1">Value 1</param>
        /// <param name="value2">Value 2</param>
        /// <param name="value3">Value 3</param>
        /// <param name="min">Returns the minimum value</param>
        /// <param name="max">Returns the maximum value</param>
        public static void MinMax(float value1, float value2, float value3, out float min, out float max)
        {
            min = value1;
            if (value2 < min) min = value2;
            if (value3 < min) min = value3;

            max = value1;
            if (value2 > max) max = value2;
            if (value3 > max) max = value3;
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

        #region Transforms

        /// <summary>
        /// Gets the screen coordinates from the specified 3D point
        /// </summary>
        /// <param name="point">3D point</param>
        /// <param name="viewPort">View port</param>
        /// <param name="wvp">World * View * Projection</param>
        /// <param name="isInsideScreen">Returns true if the resulting point is inside the screen</param>
        /// <returns>Returns the resulting screen coordinates</returns>
        public static Vector2 UnprojectToScreen(Vector3 point, Vortice.Mathematics.Viewport viewPort, Matrix4x4 wvp, out bool isInsideScreen)
        {
            isInsideScreen = true;

            // Go to projection space
            var projected = Vector3.Transform(point, wvp);

            // Clip
            // 
            //  -Wp < Xp <= Wp 
            //  -Wp < Yp <= Wp 
            //  0 < Zp <= Wp 
            // 
            if (projected.X < -projected.W)
            {
                projected.X = -projected.W;
                isInsideScreen = false;
            }
            if (projected.X > projected.W)
            {
                projected.X = projected.W;
                isInsideScreen = false;
            }
            if (projected.Y < -projected.W)
            {
                projected.Y = -projected.W;
                isInsideScreen = false;
            }
            if (projected.Y > projected.W)
            {
                projected.Y = projected.W;
                isInsideScreen = false;
            }
            if (projected.Z < 0)
            {
                projected.Z = 0;
                isInsideScreen = false;
            }
            if (projected.Z > projected.W)
            {
                projected.Z = projected.W;
                isInsideScreen = false;
            }

            // Divide by w, to move from homogeneous coordinates to 3D coordinates again 
            projected /= projected.W;

            // Perform the viewport scaling, to get the appropiate coordinates inside the viewport 
            projected.X = ((float)(((projected.X + 1.0) * 0.5) * viewPort.Width)) + viewPort.X;
            projected.Y = ((float)(((1.0 - projected.Y) * 0.5) * viewPort.Height)) + viewPort.Y;
            projected.Z = (projected.Z * (viewPort.MaxDepth - viewPort.MinDepth)) + viewPort.MinDepth;

            return projected.XY;
        }

        #endregion

        #region Rect & RectI

        public static Vector2I TopLeft(this RectI rectangle)
        {
            return new Vector2I(rectangle.Top, rectangle.Left);
        }
        public static Vector2I TopRight(this RectI rectangle)
        {
            return new Vector2I(rectangle.Top, rectangle.Right);
        }
        public static Vector2I BottomLeft(this RectI rectangle)
        {
            return new Vector2I(rectangle.Bottom, rectangle.Left);
        }
        public static Vector2I BottomRight(this RectI rectangle)
        {
            return new Vector2I(rectangle.Bottom, rectangle.Right);
        }
        public static Vector2I Center(this RectI rectangle)
        {
            return new Vector2I(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2));
        }
        public static Vector2I[] GetVertices(this RectI rectangle)
        {
            return
            [
                new (rectangle.Left, rectangle.Top),
                new (rectangle.Right, rectangle.Top),
                new (rectangle.Right, rectangle.Bottom),
                new (rectangle.Left, rectangle.Bottom),
            ];
        }
        public static Vector2[] GetVertices(this Rect rectangle)
        {
            return
            [
                new (rectangle.Left, rectangle.Top),
                new (rectangle.Right, rectangle.Top),
                new (rectangle.Right, rectangle.Bottom),
                new (rectangle.Left, rectangle.Bottom),
            ];
        }
        public static Rect Scale(this Rect rectangle, float scale)
        {
            float width = rectangle.Width * scale;
            float height = rectangle.Height * scale;
            float dWidth = width - rectangle.Width;
            float dHeight = height - rectangle.Height;

            float left = rectangle.Left - (dWidth * 0.5f);
            float top = rectangle.Top - (dHeight * 0.5f);

            return new(left, top, width, height);
        }

        #endregion
    }
}
