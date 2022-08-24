using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Cubemap pixel shader
    /// </summary>
    public class CubemapPs : IBuiltInPixelShader
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

            bool compile = Resources.Ps_Cubemap_Cso == null;
            var bytes = Resources.Ps_Cubemap_Cso ?? Resources.Ps_Cubemap;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(CubemapPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(CubemapPs), bytes);
            }
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
        public void SetShaderResources()
        {
            Graphics.SetPixelShaderResourceView(0, cubemap);

            Graphics.SetPixelShaderSampler(0, sampler);

            Graphics.SetPixelShaderSampler(0, BuiltInShaders.GetSamplerLinear());
        }
    }
}
