using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine rasterizer state
    /// </summary>
    public class EngineRasterizerState : IDisposable
    {
        /// <summary>
        /// Internal rasterizer state
        /// </summary>
        private RasterizerState2 rasterizerState = null;

        /// <summary>
        /// Creates a default rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the default rasterizer state</returns>
        public static EngineRasterizerState Default(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = true,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a wire frame rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the wire frame rasterizer state</returns>
        public static EngineRasterizerState Wireframe(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Wireframe,
                IsFrontCounterClockwise = false,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = true,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a no cull rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the no cull rasterizer state</returns>
        public static EngineRasterizerState NoCull(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = true,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a cull front face rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the cull front face rasterizer state</returns>
        public static EngineRasterizerState CullFrontFace(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = true,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = true,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a stencil pass rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the stencil pass rasterizer state</returns>
        public static EngineRasterizerState StencilPass(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a lighting pass rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the lighting pass rasterizer state</returns>
        public static EngineRasterizerState LightingPass(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = true,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }
        /// <summary>
        /// Creates a shadow mapping rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the shadow mapping rasterizer state</returns>
        public static EngineRasterizerState ShadowMapping(Graphics graphics)
        {
            var desc = new RasterizerStateDescription2()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsAntialiasedLineEnabled = graphics.MultiSampled,
                IsMultisampleEnabled = graphics.MultiSampled,
                IsScissorEnabled = false,
                IsDepthClipEnabled = true,
                DepthBias = 85,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 5.0f,
                ForcedSampleCount = 0,
                ConservativeRasterizationMode = ConservativeRasterizationMode.Off,
            };

            return graphics.CreateRasterizerState(desc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rasterizerState">Rasterizer state</param>
        internal EngineRasterizerState(RasterizerState2 rasterizerState)
        {
            this.rasterizerState = rasterizerState;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineRasterizerState()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.rasterizerState != null)
                {
                    this.rasterizerState.Dispose();
                    this.rasterizerState = null;
                }
            }
        }

        /// <summary>
        /// Gets the internal rasterizer state
        /// </summary>
        /// <returns>Returns the internal rasterizer state</returns>
        internal RasterizerState2 GetRasterizerState()
        {
            return this.rasterizerState;
        }
    }
}
