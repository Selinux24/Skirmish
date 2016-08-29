using System;
using System.Collections.Generic;

namespace Engine
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
        /// Global data keys list
        /// </summary>
        private static readonly List<string> gGlobalDataKeys = new List<string>();
        /// <summary>
        /// Per frame data keys list
        /// </summary>
        private static readonly List<string> gFrameDataKeys = new List<string>();

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
        /// Sum of primitives drawn per frame
        /// </summary>
        public static int TrianglesPerFrame = 0;
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
        /// Active buffers
        /// </summary>
        public static int Buffers = 0;
        /// <summary>
        /// Allocated memory in buffers
        /// </summary>
        public static long AllocatedMemoryInBuffers = 0;
        /// <summary>
        /// Buffer reads
        /// </summary>
        public static int BufferReads = 0;
        /// <summary>
        /// Buffer writes
        /// </summary>
        public static int BufferWrites = 0;
        /// <summary>
        /// Input assembler layout sets
        /// </summary>
        public static int IAInputLayoutSets = 0;
        /// <summary>
        /// Input assembler primitive topology sets
        /// </summary>
        public static int IAPrimitiveTopologySets = 0;
        /// <summary>
        /// Vertex buffer sets
        /// </summary>
        public static int IAVertexBuffersSets = 0;
        /// <summary>
        /// Index buffer sets
        /// </summary>
        public static int IAIndexBufferSets = 0;
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
        /// Clear counters
        /// </summary>
        public static void ClearAll()
        {
            FrameCount = 0;

            DrawCallsPerFrame = 0;

            InstancesPerFrame = 0;
            TrianglesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            Buffers = 0;
            AllocatedMemoryInBuffers = 0;
            BufferReads = 0;
            BufferWrites = 0;

            IAInputLayoutSets = 0;
            IAPrimitiveTopologySets = 0;
            IAVertexBuffersSets = 0;
            IAIndexBufferSets = 0;

            gData.Clear();
            gGlobalDataKeys.Clear();
            gFrameDataKeys.Clear();
        }
        /// <summary>
        /// Clear per frame counters
        /// </summary>
        public static void ClearFrame()
        {
            DrawCallsPerFrame = 0;

            InstancesPerFrame = 0;
            TrianglesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            BufferReads = 0;
            BufferWrites = 0;

            IAInputLayoutSets = 0;
            IAPrimitiveTopologySets = 0;
            IAVertexBuffersSets = 0;
            IAIndexBufferSets = 0;

            foreach (var key in gFrameDataKeys)
            {
                gData.Remove(key);
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
        public static void SetStatistics(string key, object value, bool global = false)
        {
            if (gData.ContainsKey(key))
            {
                gData[key] = value;
            }
            else
            {
                gData.Add(key, value);
            }

            RefreshDataKeys(key, global);
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
    }
}
