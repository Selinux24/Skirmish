using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Game counters
    /// </summary>
    public static class FrameCounters
    {
        /// <summary>
        /// Data dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> gData = new();
        /// <summary>
        /// Global data keys list
        /// </summary>
        private static readonly List<string> gGlobalDataKeys = new();
        /// <summary>
        /// Per frame data keys list
        /// </summary>
        private static readonly List<string> gFrameDataKeys = new();
        /// <summary>
        /// Frame counters dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<int, PassCounters> counters = new();
        /// <summary>
        /// Summary string builder
        /// </summary>
        private static readonly StringBuilder sb = new();

        /// <summary>
        /// Counters summary
        /// </summary>
        public static string Summary { get; private set; }

        /// <summary>
        /// Pick counters
        /// </summary>
        public static PickCounters PickCounters { get; private set; } = new PickCounters();

        /// <summary>
        /// Frame count
        /// </summary>
        public static int FrameCount { get; set; }
        /// <summary>
        /// Frame per second
        /// </summary>
        public static long FramesPerSecond { get; set; } = 0;
        /// <summary>
        /// Frame time
        /// </summary>
        public static float FrameTime { get; set; } = 0f;

        /// <summary>
        /// Texture count
        /// </summary>
        public static int Textures { get; set; } = 0;
        /// <summary>
        /// Active buffers
        /// </summary>
        public static int Buffers { get; set; } = 0;

        /// <summary>
        /// Total buffer bytes
        /// </summary>
        public static long BufferBytes
        {
            get
            {
                return gData.Values.OfType<ResourceStatus>().Sum(item => item.Size);
            }
        }
        /// <summary>
        /// Total buffer elements
        /// </summary>
        public static int BufferElements
        {
            get
            {
                return gData.Values.OfType<ResourceStatus>().Sum(item => item.Elements);
            }
        }
        /// <summary>
        /// Statistics keys
        /// </summary>
        /// <remarks>
        /// The dictionary is complete at the end of the frame
        /// </remarks>
        public static string[] Statistics
        {
            get
            {
                string[] res = new string[gGlobalDataKeys.Count + gFrameDataKeys.Count];

                Array.Copy(gGlobalDataKeys.ToArray(), 0, res, 0, gGlobalDataKeys.Count);
                Array.Copy(gFrameDataKeys.ToArray(), 0, res, gGlobalDataKeys.Count, gFrameDataKeys.Count);

                return res;
            }
        }
        /// <summary>
        /// Statistics keys count
        /// </summary>
        /// <remarks>
        /// The dictionary is complete at the end of the frame
        /// </remarks>
        public static int StatisticsCount
        {
            get
            {
                return gGlobalDataKeys.Count + gFrameDataKeys.Count;
            }
        }

        /// <summary>
        /// Create pass counters
        /// </summary>
        /// <param name="name">Pass name</param>
        /// <param name="passIndex">Pass index</param>
        public static PassCounters CreatePassCounters(string name, int passIndex)
        {
            return counters.AddOrUpdate(passIndex, new PassCounters(name, passIndex), (k, v) => v);
        }
        /// <summary>
        /// Gets the frame counters by index
        /// </summary>
        /// <param name="index">Pass index</param>
        public static PassCounters GetFrameCounters(int passIndex)
        {
            if (!counters.TryGetValue(passIndex, out var c))
            {
                return null;
            }

            return c;
        }

        /// <summary>
        /// Clear counters
        /// </summary>
        public static void ClearAll()
        {
            Buffers = 0;
            Textures = 0;

            counters.ToList().ForEach(v => v.Value.Reset());

            PickCounters.Reset();

            gData.Clear();
            gGlobalDataKeys.Clear();
            gFrameDataKeys.Clear();
        }
        /// <summary>
        /// Clear per frame counters
        /// </summary>
        public static void ClearFrame()
        {
            SetSummary();

            counters.ToList().ForEach(v => v.Value.Reset());

            PickCounters.Reset();

            foreach (var key in gFrameDataKeys)
            {
                gData.TryRemove(key, out _);
            }
            gFrameDataKeys.Clear();
        }

        /// <summary>
        /// Gets statistic value by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Return statistic value by key</returns>
        public static object GetStatistics(string key)
        {
            if (gData.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }
        /// <summary>
        /// Gets statistic value by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Return statistic value by index</returns>
        public static object GetStatistics(int index)
        {
            if (index >= 0 && index < StatisticsCount)
            {
                return gData[Statistics[index]];
            }

            return null;
        }
        /// <summary>
        /// Sets statistic value by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Key value</param>
        public static void SetStatistics(string key, object value, bool global = false)
        {
            gData.AddOrUpdate(key, value, (k, o) => o);

            RefreshDataKeys(key, global);
        }
        /// <summary>
        /// Refreshing of data keys
        /// </summary>
        private static void RefreshDataKeys(string key, bool global)
        {
            if (global)
            {
                gGlobalDataKeys.Add(key);
            }
            else
            {
                gFrameDataKeys.Add(key);
            }
        }

        /// <summary>
        /// Buffer registration
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding flags</param>
        /// <param name="sizeInBytes">Size in bytes</param>
        /// <param name="length">Number of elements</param>
        public static void RegBuffer(string name, int usage, int binding, long sizeInBytes, int length)
        {
            Buffers++;

            var key = $"{usage}.var";

            if (GetStatistics(key) is not ResourceStatus c)
            {
                c = new ResourceStatus();
                SetStatistics(key, c, true);
            }

            c.Add(name, usage, binding, sizeInBytes, length);
        }
        /// <summary>
        /// Buffer registration
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding flags</param>
        /// <param name="sizeInBytes">Size in bytes</param>
        /// <param name="length">Number of elements</param>
        public static void RegBuffer<T>(string name, int usage, int binding, long sizeInBytes, int length)
        {
            Buffers++;

            var key = $"{usage}.{typeof(T)}";

            if (GetStatistics(key) is not ResourceStatus c)
            {
                c = new ResourceStatus();
                SetStatistics(key, c, true);
            }

            c.Add(name, usage, binding, sizeInBytes, length);
        }

        /// <summary>
        /// Gets the counters summary text
        /// </summary>
        /// <returns></returns>
        private static void SetSummary()
        {
            var counterList = counters.Select(c => c.Value).OrderBy(c => c.PassIndex).ToList();

            sb.Clear();
            counterList.ForEach(c => sb.AppendLine(c.ToString()));
            Summary = sb.ToString();
        }
    }
}
