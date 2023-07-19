using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.PostProcess
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Post-process pixel shader
    /// </summary>
    public class PostProcessPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per pass constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerPass;
        /// <summary>
        /// Per effect constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEffect;
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
        public PostProcessPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(PostProcessPs), "main", PostProcessResources.PostProcess_ps, HelperShaders.PSProfile);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PostProcessPs()
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
        /// Sets per pass constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerPassConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerPass = constantBuffer;
        }
        /// <summary>
        /// Sets per effect constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerEffectConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerEffect = constantBuffer;
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
        public void SetShaderResources(EngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerPass,
                cbPerEffect,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceView(0, diffuseMapArray);

            dc.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
