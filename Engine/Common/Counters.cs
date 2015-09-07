using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Game counters
    /// </summary>
    public static class Counters
    {
        /// <summary>
        /// Data dictionary
        /// </summary>
        private static readonly Dictionary<string, object> gData = new Dictionary<string, object>();
        /// <summary>
        /// Data keys list
        /// </summary>
        private static readonly List<string> gDataKeys = new List<string>();

        /// <summary>
        /// Frame count
        /// </summary>
        public static long FrameCount = 0;
        /// <summary>
        /// Frame time
        /// </summary>
        public static float FrameTime = 0f;
        /// <summary>
        /// Draw calls per frame
        /// </summary>
        public static int DrawCallsPerFrame = 0;
        /// <summary>
        /// Sum of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public static int InstancesPerFrame = 0;
        /// <summary>
        /// Updates per frame
        /// </summary>
        public static int UpdatesPerFrame = 0;
        /// <summary>
        /// Updates per object
        /// </summary>
        public static int UpdatesPerObject = 0;
        /// <summary>
        /// Updates per instance
        /// </summary>
        public static int UpdatesPerInstance = 0;
        /// <summary>
        /// Texture updates
        /// </summary>
        public static int TextureUpdates = 0;
        /// <summary>
        /// Picking test per frame
        /// </summary>
        public static int PicksPerFrame = 0;
        /// <summary>
        /// Average picking time cost
        /// </summary>
        public static float PickingAverageTime = 0f;
        /// <summary>
        /// Rasterizer state changes count per frame
        /// </summary>
        public static int RasterizerStateChanges = 0;
        /// <summary>
        /// Blend state changes count per frame
        /// </summary>
        public static int BlendStateChanges = 0;
        /// <summary>
        /// Depth-Stencil state changes count per frame
        /// </summary>
        public static int DepthStencilStateChanges = 0;
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
                return gDataKeys.ToArray();
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
                return gDataKeys.Count;
            }
        }

        /// <summary>
        /// Clear counters
        /// </summary>
        public static void ClearAll()
        {
            FrameCount = 0;

            DrawCallsPerFrame = 0;

            InstancesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            gData.Clear();
            gDataKeys.Clear();
        }
        /// <summary>
        /// Clear per frame counters
        /// </summary>
        public static void ClearFrame()
        {
            DrawCallsPerFrame = 0;

            InstancesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            gData.Clear();
            gDataKeys.Clear();
        }

        /// <summary>
        /// Gets statistic value by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Return statistic value by key</returns>
        public static object GetStatistics(string key)
        {
            if (gData.ContainsKey(key))
            {
                return gData[key];
            }

            return null;
        }
        /// <summary>
        /// Sets statistic value by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Key value</param>
        public static void SetStatistics(string key, object value)
        {
            if (gData.ContainsKey(key))
            {
                gData[key] = value;
            }
            else
            {
                gData.Add(key, value);
            }

            RefreshDataKeys();
        }
        /// <summary>
        /// Gets statistic value by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Return statistic value by index</returns>
        public static object GetStatistics(int index)
        {
            if (index >= 0 && index < gDataKeys.Count)
            {
                return gData[gDataKeys[index]];
            }

            return null;
        }
        /// <summary>
        /// Refreshing of data keys
        /// </summary>
        private static void RefreshDataKeys()
        {
            gDataKeys.Clear();

            foreach (var key in gData.Keys)
            {
                gDataKeys.Add(key);
            }
        }
    }
}
