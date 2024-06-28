
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Graphic states management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Render target view
        /// </summary>
        private EngineRenderTargetView renderTargetView = null;
        /// <summary>
        /// Depth stencil view
        /// </summary>
        private EngineDepthStencilView depthStencilView = null;

        /// <summary>
        /// Depth stencil state with z-buffer enabled for write
        /// </summary>
        private EngineDepthStencilState depthStencilWRzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for write
        /// </summary>
        private EngineDepthStencilState depthStencilWRzBufferDisabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer enabled for read
        /// </summary>
        private EngineDepthStencilState depthStencilRDzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for read
        /// </summary>
        private EngineDepthStencilState depthStencilRDzBufferDisabled = null;
        /// <summary>
        /// No depth, no stencil
        /// </summary>
        private EngineDepthStencilState depthStencilNone = null;
        /// <summary>
        /// Depth stencil state for shadow mapping
        /// </summary>
        private EngineDepthStencilState depthStencilShadowMapping = null;

        /// <summary>
        /// Disabled blend state
        /// </summary>
        private EngineBlendState blendDisabled = null;
        /// <summary>
        /// Default blend state
        /// </summary>
        private EngineBlendState blendDefault = null;
        /// <summary>
        /// Default alpha blend state
        /// </summary>
        private EngineBlendState blendAlphaBlend = null;
        /// <summary>
        /// Default alpha blend conservative state
        /// </summary>
        private EngineBlendState blendAlphaConservativeBlend = null;
        /// <summary>
        /// Blend state for transparent blending
        /// </summary>
        private EngineBlendState blendTransparent = null;
        /// <summary>
        /// Blend state for transparent conservative blending
        /// </summary>
        private EngineBlendState blendTransparentConservative = null;
        /// <summary>
        /// Additive blend state
        /// </summary>
        private EngineBlendState blendAdditive = null;

        /// <summary>
        /// Default rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerDefault = null;
        /// <summary>
        /// Wireframe rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerWireframe = null;
        /// <summary>
        /// No-cull rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerNoCull = null;
        /// <summary>
        /// Cull counter-clockwise face rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerCullFrontFace = null;
        /// <summary>
        /// Shadow mapping rasterizer state
        /// </summary>
        private EngineRasterizerState rasterizerShadowMapping = null;

        /// <summary>
        /// Gets the default render target
        /// </summary>
        public EngineRenderTargetView DefaultRenderTarget
        {
            get
            {
                return renderTargetView;
            }
        }
        /// <summary>
        /// Gets the default depth stencil buffer
        /// </summary>
        public EngineDepthStencilView DefaultDepthStencil
        {
            get
            {
                return depthStencilView;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        private void DisposeResources()
        {
            renderTargetView?.Dispose();
            renderTargetView = null;
            depthStencilView?.Dispose();
            depthStencilView = null;

            depthStencilWRzBufferEnabled?.Dispose();
            depthStencilWRzBufferEnabled = null;
            depthStencilWRzBufferDisabled?.Dispose();
            depthStencilWRzBufferDisabled = null;
            depthStencilRDzBufferEnabled?.Dispose();
            depthStencilRDzBufferEnabled = null;
            depthStencilRDzBufferDisabled?.Dispose();
            depthStencilRDzBufferDisabled = null;
            depthStencilNone?.Dispose();
            depthStencilNone = null;
            depthStencilShadowMapping?.Dispose();
            depthStencilShadowMapping = null;

            rasterizerDefault?.Dispose();
            rasterizerDefault = null;
            rasterizerWireframe?.Dispose();
            rasterizerWireframe = null;
            rasterizerNoCull?.Dispose();
            rasterizerNoCull = null;
            rasterizerCullFrontFace?.Dispose();
            rasterizerCullFrontFace = null;
            rasterizerShadowMapping?.Dispose();
            rasterizerShadowMapping = null;

            blendDisabled?.Dispose();
            blendDisabled = null;
            blendDefault?.Dispose();
            blendDefault = null;
            blendAlphaBlend?.Dispose();
            blendAlphaBlend = null;
            blendAlphaConservativeBlend?.Dispose();
            blendAlphaConservativeBlend = null;
            blendTransparent?.Dispose();
            blendTransparent = null;
            blendTransparentConservative?.Dispose();
            blendTransparentConservative = null;
            blendAdditive?.Dispose();
            blendAdditive = null;
        }

        /// <summary>
        /// Gets the enabled z-buffer for write
        /// </summary>
        public EngineDepthStencilState GetDepthStencilWRZEnabled()
        {
            depthStencilWRzBufferEnabled ??= EngineDepthStencilState.WRzBufferEnabled(this, nameof(Graphics));

            return depthStencilWRzBufferEnabled;
        }
        /// <summary>
        /// Gets the disabled z-buffer for write
        /// </summary>
        public EngineDepthStencilState GetDepthStencilWRZDisabled()
        {
            depthStencilWRzBufferDisabled ??= EngineDepthStencilState.WRzBufferDisabled(this, nameof(Graphics));

            return depthStencilWRzBufferDisabled;
        }
        /// <summary>
        /// Gets the enabled z-buffer for read
        /// </summary>
        public EngineDepthStencilState GetDepthStencilRDZEnabled()
        {
            depthStencilRDzBufferEnabled ??= EngineDepthStencilState.RDzBufferEnabled(this, nameof(Graphics));

            return depthStencilRDzBufferEnabled;
        }
        /// <summary>
        /// Gets the disabled z-buffer for read
        /// </summary>
        public EngineDepthStencilState GetDepthStencilRDZDisabled()
        {
            depthStencilRDzBufferDisabled ??= EngineDepthStencilState.RDzBufferDisabled(this, nameof(Graphics));

            return depthStencilRDzBufferDisabled;
        }
        /// <summary>
        /// Gets the disabled depth stencil
        /// </summary>
        public EngineDepthStencilState GetDepthStencilNone()
        {
            depthStencilNone ??= EngineDepthStencilState.None(this, nameof(Graphics));

            return depthStencilNone;
        }
        /// <summary>
        /// Gets the depth state for shadow mapping
        /// </summary>
        public EngineDepthStencilState GetDepthStencilShadowMapping()
        {
            depthStencilShadowMapping ??= EngineDepthStencilState.ShadowMapping(this, nameof(Graphics));

            return depthStencilShadowMapping;
        }

        /// <summary>
        /// Gets default blend state
        /// </summary>
        public EngineBlendState GetBlendDefault()
        {
            blendDefault ??= EngineBlendState.Default(this);

            return blendDefault;
        }
        /// <summary>
        /// Gets default alpha blend state
        /// </summary>
        /// <param name="alphaConservative">Alpha conservative</param>
        public EngineBlendState GetBlendAlpha(bool alphaConservative = false)
        {
            if (alphaConservative)
            {
                blendAlphaConservativeBlend ??= EngineBlendState.AlphaConservativeBlend(this);

                return blendAlphaConservativeBlend;
            }
            else
            {
                blendAlphaBlend ??= EngineBlendState.AlphaBlend(this);

                return blendAlphaBlend;
            }
        }
        /// <summary>
        /// Gets transparent blend state
        /// </summary>
        /// <param name="alphaConservative">Alpha conservative</param>
        public EngineBlendState GetBlendTransparent(bool alphaConservative = false)
        {
            if (alphaConservative)
            {
                blendTransparentConservative ??= EngineBlendState.TransparentConservative(this);

                return blendTransparentConservative;
            }
            else
            {
                blendTransparent ??= EngineBlendState.Transparent(this);

                return blendTransparent;
            }
        }
        /// <summary>
        /// Gets additive blend state
        /// </summary>
        public EngineBlendState GetBlendAdditive()
        {
            blendAdditive ??= EngineBlendState.Additive(this);

            return blendAdditive;
        }
        /// <summary>
        /// Gets blend state
        /// </summary>
        /// <param name="drawerMode">Drawer mode</param>
        /// <param name="blendMode">Blend mode</param>
        public EngineBlendState GetBlendState(DrawerModes drawerMode, BlendModes blendMode)
        {
            if (blendMode.HasFlag(BlendModes.Additive))
            {
                return GetBlendAdditive();
            }

            if (drawerMode.HasFlag(DrawerModes.OpaqueOnly))
            {
                return GetBlendDefault();
            }

            if (blendMode.HasFlag(BlendModes.Transparent))
            {
                return GetBlendTransparent(blendMode.HasFlag(BlendModes.PostProcess));
            }

            if (blendMode.HasFlag(BlendModes.Alpha))
            {
                return GetBlendAlpha(blendMode.HasFlag(BlendModes.PostProcess));
            }

            return GetBlendDefault();
        }

        /// <summary>
        /// Gets default rasterizer
        /// </summary>
        public EngineRasterizerState GetRasterizerDefault()
        {
            rasterizerDefault ??= EngineRasterizerState.Default(this, nameof(Graphics));

            return rasterizerDefault;
        }
        /// <summary>
        /// Gets wireframe rasterizer
        /// </summary>
        public EngineRasterizerState GetRasterizerWireframe()
        {
            rasterizerWireframe ??= EngineRasterizerState.Wireframe(this, nameof(Graphics));

            return rasterizerWireframe;
        }
        /// <summary>
        /// Gets no-cull rasterizer
        /// </summary>
        public EngineRasterizerState GetRasterizerCullNone()
        {
            rasterizerNoCull ??= EngineRasterizerState.NoCull(this, nameof(Graphics));

            return rasterizerNoCull;
        }
        /// <summary>
        /// Gets cull counter-clockwise face rasterizer
        /// </summary>
        public EngineRasterizerState GetRasterizerCullFrontFace()
        {
            rasterizerCullFrontFace ??= EngineRasterizerState.CullFrontFace(this, nameof(Graphics));

            return rasterizerCullFrontFace;
        }
        /// <summary>
        /// Gets shadow mapping rasterizer state
        /// </summary>
        public EngineRasterizerState GetRasterizerShadowMapping()
        {
            rasterizerShadowMapping ??= EngineRasterizerState.ShadowMapping(this, nameof(Graphics));

            return rasterizerShadowMapping;
        }
    }
}
