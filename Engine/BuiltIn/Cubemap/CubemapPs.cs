using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Cubemap
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Cubemap pixel shader
    /// </summary>
    public class CubemapPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Cubemap resource view
        /// </summary>
        private EngineShaderResourceView cubemap;
        /// <summary>
        /// Cubemap sampler
        /// </summary>
        private EngineSamplerState sampler;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public CubemapPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(CubemapPs), "main", ForwardRenderingResources.Cubemap_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~CubemapPs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                Shader?.Dispose();
                Shader = null;
            }
        }

        /// <summary>
        /// Sets the cubemap
        /// </summary>
        /// <param name="cubemap">Cubemap</param>
        public void SetCubemap(EngineShaderResourceView cubemap)
        {
            this.cubemap = cubemap;
        }
        /// <summary>
        /// Sets the cubemap sampler state
        /// </summary>
        /// <param name="sampler">Sampler</param>
        public void SetCubemapSampler(EngineSamplerState sampler)
        {
            this.sampler = sampler;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderResourceView(0, cubemap);

            dc.SetPixelShaderSampler(0, sampler);
        }
    }
}
