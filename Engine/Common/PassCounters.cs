
namespace Engine.Common
{
    /// <summary>
    /// Pass counters
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Pass name</param>
    /// <param name="passIndex">Pass index</param>
    public class PassCounters(string name, int passIndex)
    {
        /// <summary>
        /// Pass name
        /// </summary>
        public string Name { get; private set; } = name;
        /// <summary>
        /// Pass index
        /// </summary>
        public int PassIndex { get; private set; } = passIndex;

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
}
