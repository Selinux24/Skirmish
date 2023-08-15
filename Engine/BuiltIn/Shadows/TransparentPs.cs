using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Shadow transparent texture pixel shader
    /// </summary>
    public class TransparentPs : IBuiltInShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;
        /// <summary>
        /// Diffuse sampler
        /// </summary>
        private EngineSamplerState diffuseSampler;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public TransparentPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader($"{nameof(Shadows)}_{nameof(TransparentPs)}", "main", ShadowRenderingResources.Transparent_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TransparentPs()
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
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }
        /// <summary>
        /// Sets the diffuse sampler
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse sampler</param>
        public void SetDiffuseSampler(EngineSamplerState diffuseSampler)
        {
            this.diffuseSampler = diffuseSampler;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderResourceView(0, diffuseMapArray);

            dc.SetPixelShaderSampler(0, diffuseSampler);
        }
    }
}
