using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Helpers;

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
        /// Maximum count of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public static int MaxInstancesPerFrame = 0;
        /// <summary>
        /// Sum of primitives drawn per frame
        /// </summary>
        public static int PrimitivesPerFrame = 0;
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
        /// Texture count
        /// </summary>
        public static int Textures = 0;
        /// <summary>
        /// Texture updates
        /// </summary>
        public static int TextureUpdates = 0;
        /// <summary>
        /// Picking test per frame
        /// </summary>
        public static int PicksPerFrame { get; private set; }
        /// <summary>
        /// Total picking time cost per frame
        /// </summary>
        public static float PickingTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average picking time cost
        /// </summary>
        public static float PickingAverageTime { get; private set; }
        /// <summary>
        /// Box volume tests per frame
        /// </summary>
        public static float VolumeBoxTestPerFrame { get; private set; }
        /// <summary>
        /// Total box volume tests time cost
        /// </summary>
        public static float VolumeBoxTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average box volume tests time cost
        /// </summary>
        public static float VolumeBoxTestAverageTime { get; private set; }
        /// <summary>
        /// Sphere volume tests per frame
        /// </summary>
        public static float VolumeSphereTestPerFrame { get; private set; }
        /// <summary>
        /// Total sphere volume tests time cost
        /// </summary>
        public static float VolumeSphereTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average sphere volume tests time cost
        /// </summary>
        public static float VolumeSphereTestAverageTime { get; private set; }
        /// <summary>
        /// Frustum volume tests per frame
        /// </summary>
        public static float VolumeFrustumTestPerFrame { get; private set; }
        /// <summary>
        /// Total frustum volume tests time cost
        /// </summary>
        public static float VolumeFrustumTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average frustum volume tests time cost
        /// </summary>
        public static float VolumeFrustumTestAverageTime { get; private set; }
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
        /// State changes count per frame (rasterizer, blend and depth-stencil states)
        /// </summary>
        public static int StateChanges
        {
            get
            {
                return RasterizerStateChanges + BlendStateChanges + DepthStencilStateChanges;
            }
        }
        /// <summary>
        /// Active buffers
        /// </summary>
        public static int Buffers = 0;
        /// <summary>
        /// Total buffer bytes
        /// </summary>
        public static long BufferBytes
        {
            get
            {
                long kBytes = 0;

                foreach (var item in gData.Values)
                {
                    var rs = item as Engine.Helpers.ResourceStatus;
                    if (rs != null)
                    {
                        kBytes += rs.Size;
                    }
                }

                return kBytes;
            }
        }
        /// <summary>
        /// Total buffer elements
        /// </summary>
        public static int BufferElements
        {
            get
            {
                int elements = 0;

                foreach (var item in gData.Values)
                {
                    var rs = item as Engine.Helpers.ResourceStatus;
                    if (rs != null)
                    {
                        elements += rs.Elements;
                    }
                }

                return elements;
            }
        }
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
        /// Stream output targets sets
        /// </summary>
        public static int SOTargetsSet = 0;
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
            MaxInstancesPerFrame = 0;
            PrimitivesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            Textures = 0;
            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingTotalTimePerFrame = 0f;
            PickingAverageTime = 0f;
            VolumeBoxTestPerFrame = 0;
            VolumeBoxTestTotalTimePerFrame = 0f;
            VolumeBoxTestAverageTime = 0f;
            VolumeSphereTestPerFrame = 0;
            VolumeSphereTestTotalTimePerFrame = 0f;
            VolumeSphereTestAverageTime = 0f;
            VolumeFrustumTestPerFrame = 0;
            VolumeFrustumTestTotalTimePerFrame = 0f;
            VolumeFrustumTestAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            Buffers = 0;
            BufferReads = 0;
            BufferWrites = 0;

            IAInputLayoutSets = 0;
            IAPrimitiveTopologySets = 0;
            IAVertexBuffersSets = 0;
            IAIndexBufferSets = 0;

            SOTargetsSet = 0;

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
            MaxInstancesPerFrame = 0;
            PrimitivesPerFrame = 0;

            UpdatesPerFrame = 0;
            UpdatesPerObject = 0;
            UpdatesPerInstance = 0;

            TextureUpdates = 0;

            PicksPerFrame = 0;
            PickingTotalTimePerFrame = 0f;
            PickingAverageTime = 0f;
            VolumeBoxTestPerFrame = 0;
            VolumeBoxTestTotalTimePerFrame = 0f;
            VolumeBoxTestAverageTime = 0f;
            VolumeSphereTestPerFrame = 0;
            VolumeSphereTestTotalTimePerFrame = 0f;
            VolumeSphereTestAverageTime = 0f;
            VolumeFrustumTestPerFrame = 0;
            VolumeFrustumTestTotalTimePerFrame = 0f;
            VolumeFrustumTestAverageTime = 0f;

            RasterizerStateChanges = 0;
            BlendStateChanges = 0;
            DepthStencilStateChanges = 0;

            BufferReads = 0;
            BufferWrites = 0;

            IAInputLayoutSets = 0;
            IAPrimitiveTopologySets = 0;
            IAVertexBuffersSets = 0;
            IAIndexBufferSets = 0;

            SOTargetsSet = 0;

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

        /// <summary>
        /// Buffer registration
        /// </summary>
        /// <param name="type">Type of buffer</param>
        /// <param name="name">Name</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding flags</param>
        /// <param name="sizeInBytes">Size in bytes</param>
        /// <param name="length">Number of elements</param>
        public static void RegBuffer(Type type, string name, int usage, int binding, long sizeInBytes, int length)
        {
            Buffers++;

            var key = string.Format("{0}.{1}", usage, type);

            var c = Counters.GetStatistics(key) as ResourceStatus;
            if (c == null)
            {
                c = new ResourceStatus();
                Counters.SetStatistics(key, c, true);
            }

            c.Add(name, usage, binding, sizeInBytes, length);
        }

        public static void AddPick(float delta)
        {
            PicksPerFrame++;
            PickingTotalTimePerFrame += delta;
            PickingAverageTime = PickingTotalTimePerFrame / PicksPerFrame;
        }

        public static void AddVolumeFrustumTest(float delta)
        {
            VolumeFrustumTestPerFrame++;
            VolumeFrustumTestTotalTimePerFrame += delta;
            VolumeFrustumTestAverageTime = VolumeFrustumTestTotalTimePerFrame / VolumeFrustumTestPerFrame;
        }

        public static void AddVolumeBoxTest(float delta)
        {
            VolumeBoxTestPerFrame++;
            VolumeBoxTestTotalTimePerFrame += delta;
            VolumeBoxTestAverageTime = VolumeBoxTestTotalTimePerFrame / VolumeBoxTestPerFrame;
        }

        public static void AddVolumeSphereTest(float delta)
        {
            VolumeSphereTestPerFrame++;
            VolumeSphereTestTotalTimePerFrame += delta;
            VolumeSphereTestAverageTime = VolumeSphereTestTotalTimePerFrame / VolumeSphereTestPerFrame;
        }
    }
}
