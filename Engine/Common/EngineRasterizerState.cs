using System;

namespace Engine.Common
{
    using RasterizerState2 = SharpDX.Direct3D11.RasterizerState2;

    /// <summary>
    /// Engine rasterizer state
    /// </summary>
    public class EngineRasterizerState : IDisposable
    {
        /// <summary>
        /// Internal rasterizer state
        /// </summary>
        private RasterizerState2 state = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a default rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Returns the default rasterizer state</returns>
        public static EngineRasterizerState Default(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(Default)}", desc);
        }
        /// <summary>
        /// Creates a wire frame rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Returns the wire frame rasterizer state</returns>
        public static EngineRasterizerState Wireframe(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(Wireframe)}", desc);
        }
        /// <summary>
        /// Creates a no cull rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the no cull rasterizer state</returns>
        public static EngineRasterizerState NoCull(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(NoCull)}", desc);
        }
        /// <summary>
        /// Creates a cull front face rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the cull front face rasterizer state</returns>
        public static EngineRasterizerState CullFrontFace(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(CullFrontFace)}", desc);
        }
        /// <summary>
        /// Creates a stencil pass rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the stencil pass rasterizer state</returns>
        public static EngineRasterizerState StencilPass(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(StencilPass)}", desc);
        }
        /// <summary>
        /// Creates a lighting pass rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the lighting pass rasterizer state</returns>
        public static EngineRasterizerState LightingPass(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(LightingPass)}", desc);
        }
        /// <summary>
        /// Creates a shadow mapping rasterizer state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the shadow mapping rasterizer state</returns>
        public static EngineRasterizerState ShadowMapping(Graphics graphics, string name)
        {
            var desc = new EngineRasterizerStateDescription()
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

            return graphics.CreateRasterizerState($"{name}.{nameof(ShadowMapping)}", desc);
        }
        /// <summary>
        /// Creates a new rasterizer state
        /// </summary>
        /// <param name="graphics">Grahics</param>
        /// <param name="name">Name</param>
        /// <param name="description">Rasterizer state descriptio</param>
        public static EngineRasterizerState Create(Graphics graphics, string name, EngineRasterizerStateDescription description)
        {
            return graphics.CreateRasterizerState(name, description);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="rasterizerState">Rasterizer state</param>
        internal EngineRasterizerState(string name, RasterizerState2 rasterizerState)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A rasterizer state name must be specified.");
            state = rasterizerState ?? throw new ArgumentNullException(nameof(rasterizerState), "A rasterizer state must be specified.");

            state.DebugName = name;
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
                state?.Dispose();
                state = null;
            }
        }

        /// <summary>
        /// Gets the internal rasterizer state
        /// </summary>
        /// <returns>Returns the internal rasterizer state</returns>
        internal RasterizerState2 GetRasterizerState()
        {
            return state;
        }
    }
}
