using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class PositionTexturePs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerFrame;
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;
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
        public PositionTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(PositionTexturePs), "main", ShaderDefaultBasicResources.PositionTexture_ps, HelperShaders.PSProfile);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionTexturePs()
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
        /// Sets per frame constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerFrameConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerFrame = constantBuffer;
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
        /// Sets the diffuse sampler state
        /// </summary>
        /// <param name="samplerDiffuse">Diffuse sampler</param>
        public void SetDiffseSampler(EngineSamplerState samplerDiffuse)
        {
            this.samplerDiffuse = samplerDiffuse;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerFrame,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            Graphics.SetPixelShaderResourceView(0, diffuseMapArray);

            Graphics.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
