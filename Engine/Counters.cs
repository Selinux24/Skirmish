using System;
using System.Collections.Concurrent;
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
        /// Maximum count of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public static int MaxInstancesPerFrame { get; set; } = 0;
        /// <summary>
        /// Sum of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public static int InstancesPerFrame { get; set; } = 0;
        /// <summary>
        /// Sum of primitives drawn per frame
        /// </summary>
        public static int PrimitivesPerFrame { get; set; } = 0;
        /// <summary>
        /// Updates per frame
        /// </summary>
        public static int TransformUpdatesPerFrame { get; set; } = 0;
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
        /// Context state clear calls per frame (ClearState)
        /// </summary>
        public static int ContextClears { get; set; } = 0;

        /// <summary>
        /// Viewport set calls count per frame (SetViewports)
        /// </summary>
        public static int ViewportsSets { get; set; } = 0;
        /// <summary>
        /// Render target set calls count per frame (SetTargets)
        /// </summary>
        public static int RenderTargetSets { get; set; } = 0;
        /// <summary>
        /// Render target clear calls count per frame (ClearRenderTargetView)
        /// </summary>
        public static int RenderTargetClears { get; set; } = 0;
        /// <summary>
        /// Depth-Stencil clear calls count per frame (ClearDepthStencilView)
        /// </summary>
        public static int DepthStencilClears { get; set; } = 0;
        /// <summary>
        /// Depth-Stencil state changes count per frame (SetDepthStencilState)
        /// </summary>
        public static int DepthStencilStateChanges { get; set; } = 0;

        /// <summary>
        /// Rasterizer state changes count per frame (Rasterizer setter)
        /// </summary>
        public static int RasterizerStateChanges { get; set; } = 0;
        /// <summary>
        /// Blend state changes count per frame (OM.SetBlendState)
        /// </summary>
        public static int OMBlendStateChanges { get; set; } = 0;

        /// <summary>
        /// Input assembler layout sets
        /// </summary>
        public static int IAInputLayoutSets { get; set; } = 0;
        /// <summary>
        /// Input assembler primitive topology sets
        /// </summary>
        public static int IAPrimitiveTopologySets { get; set; } = 0;
        /// <summary>
        /// Vertex buffer sets
        /// </summary>
        public static int IAVertexBuffersSets { get; set; } = 0;
        /// <summary>
        /// Index buffer sets
        /// </summary>
        public static int IAIndexBufferSets { get; set; } = 0;

        /// <summary>
        /// Constant buffer update calls per frame (SetConstantBuffers)
        /// </summary>
        public static int ConstantBufferSets { get; set; } = 0;
        /// <summary>
        /// Constant buffer clear calls per frame (SetConstantBuffers)
        /// </summary>
        public static int ConstantBufferClears { get; set; } = 0;
        /// <summary>
        /// Shader resource update calls per frame (SetShaderResources)
        /// </summary>
        public static int ShaderResourceSets { get; set; } = 0;
        /// <summary>
        /// Shader resource clear calls per frame (SetShaderResources)
        /// </summary>
        public static int ShaderResourceClears { get; set; } = 0;
        /// <summary>
        /// Sampler update calls per frame (SetSamplers)
        /// </summary>
        public static int SamplerSets { get; set; } = 0;
        /// <summary>
        /// Sampler clear calls per frame (SetSamplers)
        /// </summary>
        public static int SamplerClears { get; set; } = 0;

        /// <summary>
        /// Vertex shader sets per frame (VertexShader setter)
        /// </summary>
        public static int VertexShadersSets { get; set; } = 0;
        /// <summary>
        /// Hull shader sets per frame (HullShader setter)
        /// </summary>
        public static int HullShadersSets { get; set; } = 0;
        /// <summary>
        /// Domain shader sets per frame (DomainShader setter)
        /// </summary>
        public static int DomainShadersSets { get; set; } = 0;
        /// <summary>
        /// Geometry shader sets per frame (GeometryShader setter)
        /// </summary>
        public static int GeometryShadersSets { get; set; } = 0;
        /// <summary>
        /// Pixel shader sets per frame (PixelShader setter)
        /// </summary>
        public static int PixelShadersSets { get; set; } = 0;
        /// <summary>
        /// Compute shader sets per frame (ComputeShader setter)
        /// </summary>
        public static int ComputeShadersSets { get; set; } = 0;

        /// <summary>
        /// Technique passes per frame (Apply)
        /// </summary>
        public static int TechniquePasses { get; set; } = 0;

        /// <summary>
        /// Subresource updates per frame (UpdateSubresource)
        /// </summary>
        public static int SubresourceUpdates { get; set; } = 0;
        /// <summary>
        /// Subresource maps per frame (MapSubresource)
        /// </summary>
        public static int SubresourceMaps { get; set; } = 0;
        /// <summary>
        /// Subresource unmaps per frame (UnmapSubresource)
        /// </summary>
        public static int SubresourceUnmaps { get; set; } = 0;

        /// <summary>
        /// Complete texture writes per frame
        /// </summary>
        public static int TextureWrites { get; set; } = 0;
        /// <summary>
        /// Complete buffer writes per frame
        /// </summary>
        public static int BufferWrites { get; set; } = 0;
        /// <summary>
        /// Complete buffer reads per frame
        /// </summary>
        public static int BufferReads { get; set; } = 0;

        /// <summary>
        /// Stream output targets sets per frame (SO.SetTargets)
        /// </summary>
        public static int SOTargetsSets { get; set; } = 0;

        /// <summary>
        /// Draw calls per frame
        /// </summary>
        public static int DrawCallsPerFrame { get; set; } = 0;

        /// <summary>
        /// Finish command list calls per frame (FinishCommandList)
        /// </summary>
        public static int FinishCommandLists { get; set; } = 0;
        /// <summary>
        /// Execute command list calls per frame (ExecuteCommandList)
        /// </summary>
        public static int ExecuteCommandLists { get; set; } = 0;

        /// <summary>
        /// State changes count per frame (rasterizer, blend and depth-stencil states)
        /// </summary>
        public static int StateChanges
        {
            get
            {
                return RasterizerStateChanges + OMBlendStateChanges + DepthStencilStateChanges;
            }
        }

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
                    if (item is ResourceStatus rs)
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
                    if (item is ResourceStatus rs)
                    {
                        elements += rs.Elements;
                    }
                }

                return elements;
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
        /// Clear counters
        /// </summary>
        public static void ClearAll()
        {
            Buffers = 0;
            Textures = 0;

            ResetCounters();

            gData.Clear();
            gGlobalDataKeys.Clear();
            gFrameDataKeys.Clear();
        }
        /// <summary>
        /// Clear per frame counters
        /// </summary>
        public static void ClearFrame()
        {
            ResetCounters();

            foreach (var key in gFrameDataKeys)
            {
                gData.TryRemove(key, out _);
            }
            gFrameDataKeys.Clear();
        }
        /// <summary>
        /// Reset all counters to zero.
        /// </summary>
        private static void ResetCounters()
        {
            ContextClears = 0;

            ViewportsSets = 0;
            RenderTargetSets = 0;
            RenderTargetClears = 0;
            DepthStencilClears = 0;
            DepthStencilStateChanges = 0;

            RasterizerStateChanges = 0;
            OMBlendStateChanges = 0;

            IAInputLayoutSets = 0;
            IAPrimitiveTopologySets = 0;
            IAVertexBuffersSets = 0;
            IAIndexBufferSets = 0;

            ConstantBufferSets = 0;
            ConstantBufferClears = 0;
            ShaderResourceSets = 0;
            ShaderResourceClears = 0;
            SamplerSets = 0;
            SamplerClears = 0;

            VertexShadersSets = 0;
            HullShadersSets = 0;
            DomainShadersSets = 0;
            GeometryShadersSets = 0;
            SOTargetsSets = 0;
            PixelShadersSets = 0;
            ComputeShadersSets = 0;

            TechniquePasses = 0;
            SubresourceUpdates = 0;
            SubresourceMaps = 0;
            SubresourceUnmaps = 0;

            TextureWrites = 0;
            BufferWrites = 0;
            BufferReads = 0;

            DrawCallsPerFrame = 0;

            FinishCommandLists = 0;
            ExecuteCommandLists = 0;


            InstancesPerFrame = 0;
            MaxInstancesPerFrame = 0;
            PrimitivesPerFrame = 0;

            TransformUpdatesPerFrame = 0;

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

            if (GetStatistics(key) is not ResourceStatus c)
            {
                c = new ResourceStatus();
                SetStatistics(key, c, true);
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
