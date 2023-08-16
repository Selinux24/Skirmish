using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// Frame counters dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrameCounters> counters = new();

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
        /// Create pass counters
        /// </summary>
        /// <param name="name">Pass name</param>
        /// <param name="passIndex">Pass index</param>
        public static FrameCounters CreatePassCounters(string name, int passIndex)
        {
            return counters.AddOrUpdate(passIndex, new FrameCounters(name, passIndex), (k, v) => v);
        }
        /// <summary>
        /// Gets the frame counters by index
        /// </summary>
        /// <param name="index">Pass index</param>
        public static FrameCounters GetFrameCounters(int passIndex)
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

        /// <summary>
        /// Gets the counters summary text
        /// </summary>
        /// <returns></returns>
        private static void SetSummary()
        {
            var counterList = counters.Select(c => c.Value).OrderBy(c => c.PassIndex).ToList();

            StringBuilder sb = new();

            counterList.ForEach(c => sb.AppendLine(c.ToString()));

            Summary = sb.ToString();
        }
    }

    /// <summary>
    /// Frame counters
    /// </summary>
    public class FrameCounters
    {
        /// <summary>
        /// Pass name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Pass index
        /// </summary>
        public int PassIndex { get; private set; }

        /// <summary>
        /// Context state clear calls per frame (ClearState)
        /// </summary>
        public int ContextClears { get; set; }

        /// <summary>
        /// Viewport set calls count per frame (SetViewports)
        /// </summary>
        public int ViewportsSets { get; set; }
        /// <summary>
        /// Render target set calls count per frame (SetTargets)
        /// </summary>
        public int RenderTargetSets { get; set; }
        /// <summary>
        /// Render target clear calls count per frame (ClearRenderTargetView)
        /// </summary>
        public int RenderTargetClears { get; set; }
        /// <summary>
        /// Depth-Stencil clear calls count per frame (ClearDepthStencilView)
        /// </summary>
        public int DepthStencilClears { get; set; }
        /// <summary>
        /// Depth-Stencil state changes count per frame (SetDepthStencilState)
        /// </summary>
        public int DepthStencilStateChanges { get; set; }

        /// <summary>
        /// Rasterizer state changes count per frame (Rasterizer setter)
        /// </summary>
        public int RasterizerStateChanges { get; set; }
        /// <summary>
        /// Blend state changes count per frame (OM.SetBlendState)
        /// </summary>
        public int OMBlendStateChanges { get; set; }

        /// <summary>
        /// Input assembler layout sets
        /// </summary>
        public int IAInputLayoutSets { get; set; }
        /// <summary>
        /// Input assembler primitive topology sets
        /// </summary>
        public int IAPrimitiveTopologySets { get; set; }
        /// <summary>
        /// Vertex buffer sets
        /// </summary>
        public int IAVertexBuffersSets { get; set; }
        /// <summary>
        /// Index buffer sets
        /// </summary>
        public int IAIndexBufferSets { get; set; }

        /// <summary>
        /// Constant buffer update calls per frame (SetConstantBuffers)
        /// </summary>
        public int ConstantBufferSets { get; set; }
        /// <summary>
        /// Constant buffer clear calls per frame (SetConstantBuffers)
        /// </summary>
        public int ConstantBufferClears { get; set; }
        /// <summary>
        /// Shader resource update calls per frame (SetShaderResources)
        /// </summary>
        public int ShaderResourceSets { get; set; }
        /// <summary>
        /// Shader resource clear calls per frame (SetShaderResources)
        /// </summary>
        public int ShaderResourceClears { get; set; }
        /// <summary>
        /// Sampler update calls per frame (SetSamplers)
        /// </summary>
        public int SamplerSets { get; set; }
        /// <summary>
        /// Sampler clear calls per frame (SetSamplers)
        /// </summary>
        public int SamplerClears { get; set; }

        /// <summary>
        /// Vertex shader sets per frame (VertexShader setter)
        /// </summary>
        public int VertexShadersSets { get; set; }
        /// <summary>
        /// Hull shader sets per frame (HullShader setter)
        /// </summary>
        public int HullShadersSets { get; set; }
        /// <summary>
        /// Domain shader sets per frame (DomainShader setter)
        /// </summary>
        public int DomainShadersSets { get; set; }
        /// <summary>
        /// Geometry shader sets per frame (GeometryShader setter)
        /// </summary>
        public int GeometryShadersSets { get; set; }
        /// <summary>
        /// Pixel shader sets per frame (PixelShader setter)
        /// </summary>
        public int PixelShadersSets { get; set; }
        /// <summary>
        /// Compute shader sets per frame (ComputeShader setter)
        /// </summary>
        public int ComputeShadersSets { get; set; }

        /// <summary>
        /// Technique passes per frame (Apply)
        /// </summary>
        public int TechniquePasses { get; set; }

        /// <summary>
        /// Subresource updates per frame (UpdateSubresource)
        /// </summary>
        public int SubresourceUpdates { get; set; }
        /// <summary>
        /// Subresource maps per frame (MapSubresource)
        /// </summary>
        public int SubresourceMaps { get; set; }
        /// <summary>
        /// Subresource unmaps per frame (UnmapSubresource)
        /// </summary>
        public int SubresourceUnmaps { get; set; }

        /// <summary>
        /// Complete texture writes per frame
        /// </summary>
        public int TextureWrites { get; set; }
        /// <summary>
        /// Complete buffer writes per frame
        /// </summary>
        public int BufferWrites { get; set; }
        /// <summary>
        /// Complete buffer reads per frame
        /// </summary>
        public int BufferReads { get; set; }

        /// <summary>
        /// Stream output targets sets per frame (SO.SetTargets)
        /// </summary>
        public int SOTargetsSets { get; set; }

        /// <summary>
        /// Draw calls per frame
        /// </summary>
        public int DrawCallsPerFrame { get; set; }

        /// <summary>
        /// Finish command list calls per frame (FinishCommandList)
        /// </summary>
        public int FinishCommandLists { get; set; }
        /// <summary>
        /// Execute command list calls per frame (ExecuteCommandList)
        /// </summary>
        public int ExecuteCommandLists { get; set; }

        /// <summary>
        /// Sum of single draw calls and instance draw calls * instance count of this call
        /// </summary>
        public int InstancesPerFrame { get; set; }
        /// <summary>
        /// Sum of primitives drawn per frame
        /// </summary>
        public int PrimitivesPerFrame { get; set; }

        /// <summary>
        /// State changes count per frame (rasterizer, blend and depth-stencil states)
        /// </summary>
        public int StateChanges
        {
            get
            {
                return RasterizerStateChanges + OMBlendStateChanges + DepthStencilStateChanges;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Pass name</param>
        /// <param name="passIndex">Pass index</param>
        public FrameCounters(string name, int passIndex)
        {
            Name = name;
            PassIndex = passIndex;
        }

        /// <summary>
        /// Reset all counters to zero.
        /// </summary>
        public void Reset()
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
            PrimitivesPerFrame = 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (PassIndex < 0)
            {
                return $"ExecuteCommandLists => {ExecuteCommandLists:00} Viewport/RenderTarget/DepthStencil => {ViewportsSets:00}/{RenderTargetSets + RenderTargetClears:00}/{DepthStencilClears + DepthStencilStateChanges:00} ### {Name}";
            }
            else
            {
                return $"DrawCalls/Instances/Triangles => {DrawCallsPerFrame:000}/{InstancesPerFrame:0000}/{PrimitivesPerFrame:000000000} Rasterizer/DepthStencil/Blend => {RasterizerStateChanges:00}/{DepthStencilClears + DepthStencilStateChanges:00}/{OMBlendStateChanges:00} ### {Name}";
            }
        }
    }

    /// <summary>
    /// Pick counters
    /// </summary>
    public class PickCounters
    {
        /// <summary>
        /// Updates per frame
        /// </summary>
        public int TransformUpdatesPerFrame { get; set; }
        /// <summary>
        /// Picking test per frame
        /// </summary>
        public int PicksPerFrame { get; private set; }
        /// <summary>
        /// Total picking time cost per frame
        /// </summary>
        public float PickingTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average picking time cost
        /// </summary>
        public float PickingAverageTime { get; private set; }
        /// <summary>
        /// Box volume tests per frame
        /// </summary>
        public float VolumeBoxTestPerFrame { get; private set; }
        /// <summary>
        /// Total box volume tests time cost
        /// </summary>
        public float VolumeBoxTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average box volume tests time cost
        /// </summary>
        public float VolumeBoxTestAverageTime { get; private set; }
        /// <summary>
        /// Sphere volume tests per frame
        /// </summary>
        public float VolumeSphereTestPerFrame { get; private set; }
        /// <summary>
        /// Total sphere volume tests time cost
        /// </summary>
        public float VolumeSphereTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average sphere volume tests time cost
        /// </summary>
        public float VolumeSphereTestAverageTime { get; private set; }
        /// <summary>
        /// Frustum volume tests per frame
        /// </summary>
        public float VolumeFrustumTestPerFrame { get; private set; }
        /// <summary>
        /// Total frustum volume tests time cost
        /// </summary>
        public float VolumeFrustumTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average frustum volume tests time cost
        /// </summary>
        public float VolumeFrustumTestAverageTime { get; private set; }

        /// <summary>
        /// Adds a pick
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddPick(float delta)
        {
            PicksPerFrame++;
            PickingTotalTimePerFrame += delta;
            PickingAverageTime = PickingTotalTimePerFrame / PicksPerFrame;
        }
        /// <summary>
        /// Adds a frustum test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeFrustumTest(float delta)
        {
            VolumeFrustumTestPerFrame++;
            VolumeFrustumTestTotalTimePerFrame += delta;
            VolumeFrustumTestAverageTime = VolumeFrustumTestTotalTimePerFrame / VolumeFrustumTestPerFrame;
        }
        /// <summary>
        /// Adds a box test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeBoxTest(float delta)
        {
            VolumeBoxTestPerFrame++;
            VolumeBoxTestTotalTimePerFrame += delta;
            VolumeBoxTestAverageTime = VolumeBoxTestTotalTimePerFrame / VolumeBoxTestPerFrame;
        }
        /// <summary>
        /// Adds a sphere test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeSphereTest(float delta)
        {
            VolumeSphereTestPerFrame++;
            VolumeSphereTestTotalTimePerFrame += delta;
            VolumeSphereTestAverageTime = VolumeSphereTestTotalTimePerFrame / VolumeSphereTestPerFrame;
        }

        /// <summary>
        /// Reset all counters to zero.
        /// </summary>
        public void Reset()
        {
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
    }
}
