﻿using SharpDX;

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
        /// Current depth-stencil state
        /// </summary>
        private EngineDepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current blend state
        /// </summary>
        private EngineBlendState currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private EngineRasterizerState currentRasterizerState = null;

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
        /// Sets default render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        public void SetDefaultRenderTarget(bool clearRT, Color4 clearRTColor)
        {
            SetRenderTargets(
                renderTargetView, clearRT, clearRTColor,
                false);
        }
        /// <summary>
        /// Sets default render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void SetDefaultRenderTarget(bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil)
        {
            SetRenderTargets(
                renderTargetView, clearRT, clearRTColor,
                depthStencilView, clearDepth, clearStencil,
                false);
        }

        /// <summary>
        /// Enables z-buffer for write
        /// </summary>
        public void SetDepthStencilWRZEnabled()
        {
            if (depthStencilWRzBufferEnabled == null)
            {
                depthStencilWRzBufferEnabled = EngineDepthStencilState.WRzBufferEnabled(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilWRzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for write
        /// </summary>
        public void SetDepthStencilWRZDisabled()
        {
            if (depthStencilWRzBufferDisabled == null)
            {
                depthStencilWRzBufferDisabled = EngineDepthStencilState.WRzBufferDisabled(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilWRzBufferDisabled);
        }
        /// <summary>
        /// Enables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZEnabled()
        {
            if (depthStencilRDzBufferEnabled == null)
            {
                depthStencilRDzBufferEnabled = EngineDepthStencilState.RDzBufferEnabled(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilRDzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZDisabled()
        {
            if (depthStencilRDzBufferDisabled == null)
            {
                depthStencilRDzBufferDisabled = EngineDepthStencilState.RDzBufferDisabled(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilRDzBufferDisabled);
        }
        /// <summary>
        /// Disables depth stencil
        /// </summary>
        public void SetDepthStencilNone()
        {
            if (depthStencilNone == null)
            {
                depthStencilNone = EngineDepthStencilState.None(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilNone);
        }
        /// <summary>
        /// Sets the depth state for shadow mapping
        /// </summary>
        public void SetDepthStencilShadowMapping()
        {
            if (depthStencilShadowMapping == null)
            {
                depthStencilShadowMapping = EngineDepthStencilState.ShadowMapping(this, nameof(Graphics));
            }

            SetDepthStencilState(depthStencilShadowMapping);
        }

        /// <summary>
        /// Sets default blend state
        /// </summary>
        public void SetBlendDefault()
        {
            if (blendDefault == null)
            {
                blendDefault = EngineBlendState.Default(this);
            }

            SetBlendState(blendDefault);
        }
        /// <summary>
        /// Sets default alpha blend state
        /// </summary>
        public void SetBlendAlpha(bool alphaConservative = false)
        {
            if (alphaConservative)
            {
                if (blendAlphaConservativeBlend == null)
                {
                    blendAlphaConservativeBlend = EngineBlendState.AlphaConservativeBlend(this);
                }

                SetBlendState(blendAlphaConservativeBlend);
            }
            else
            {
                if (blendAlphaBlend == null)
                {
                    blendAlphaBlend = EngineBlendState.AlphaBlend(this);
                }

                SetBlendState(blendAlphaBlend);
            }
        }
        /// <summary>
        /// Sets transparent blend state
        /// </summary>
        public void SetBlendTransparent(bool alphaConservative = false)
        {
            if (alphaConservative)
            {
                if (blendTransparentConservative == null)
                {
                    blendTransparentConservative = EngineBlendState.TransparentConservative(this);
                }

                SetBlendState(blendTransparentConservative);
            }
            else
            {
                if (blendTransparent == null)
                {
                    blendTransparent = EngineBlendState.Transparent(this);
                }

                SetBlendState(blendTransparent);
            }
        }
        /// <summary>
        /// Sets additive blend state
        /// </summary>
        public void SetBlendAdditive()
        {
            if (blendAdditive == null)
            {
                blendAdditive = EngineBlendState.Additive(this);
            }

            SetBlendState(blendAdditive);
        }

        /// <summary>
        /// Sets default rasterizer
        /// </summary>
        public void SetRasterizerDefault()
        {
            if (rasterizerDefault == null)
            {
                rasterizerDefault = EngineRasterizerState.Default(this, nameof(Graphics));
            }

            SetRasterizerState(rasterizerDefault);
        }
        /// <summary>
        /// Sets wireframe rasterizer
        /// </summary>
        public void SetRasterizerWireframe()
        {
            if (rasterizerWireframe == null)
            {
                rasterizerWireframe = EngineRasterizerState.Wireframe(this, nameof(Graphics));
            }

            SetRasterizerState(rasterizerWireframe);
        }
        /// <summary>
        /// Sets no-cull rasterizer
        /// </summary>
        public void SetRasterizerCullNone()
        {
            if (rasterizerNoCull == null)
            {
                rasterizerNoCull = EngineRasterizerState.NoCull(this, nameof(Graphics));
            }

            SetRasterizerState(rasterizerNoCull);
        }
        /// <summary>
        /// Sets cull counter-clockwise face rasterizer
        /// </summary>
        public void SetRasterizerCullFrontFace()
        {
            if (rasterizerCullFrontFace == null)
            {
                rasterizerCullFrontFace = EngineRasterizerState.CullFrontFace(this, nameof(Graphics));
            }

            SetRasterizerState(rasterizerCullFrontFace);
        }
        /// <summary>
        /// Sets shadow mapping rasterizer state
        /// </summary>
        public void SetRasterizerShadowMapping()
        {
            if (rasterizerShadowMapping == null)
            {
                rasterizerShadowMapping = EngineRasterizerState.ShadowMapping(this, nameof(Graphics));
            }

            SetRasterizerState(rasterizerShadowMapping);
        }
    }
}