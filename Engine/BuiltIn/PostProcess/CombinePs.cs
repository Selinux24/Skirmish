using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.PostProcess
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Combine pixel shader
    /// </summary>
    public class CombinePs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Texture 1 resource view
        /// </summary>
        private EngineShaderResourceView texture1;
        /// <summary>
        /// Texture 2 resource view
        /// </summary>
        private EngineShaderResourceView texture2;
        /// <summary>
        /// Diffuse sampler
        /// </summary>
        private EngineSamplerState samplerDiffuse;

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
        public CombinePs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(CombinePs), "main", PostProcessResources.Combine_ps, HelperShaders.PSProfile);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~CombinePs()
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
        /// Sets the textures to combine
        /// </summary>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        public void SetTextures(EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            this.texture1 = texture1;
            this.texture2 = texture2;
        }
        /// <summary>
        /// Sets the diffuse sampler state
        /// </summary>
        /// <param name="samplerDiffuse">Diffuse sampler</param>
        public void SetDiffseSampler(EngineSamplerState samplerDiffuse)
        {
            this.samplerDiffuse = samplerDiffuse;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var rv = new[]
            {
                texture1,
                texture2,
            };

            dc.SetPixelShaderResourceViews(0, rv);

            dc.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
