using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Clouds
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Clouds pixel shader
    /// </summary>
    public class CloudsPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per cloud constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerCloud;
        /// <summary>
        /// First cloud layer
        /// </summary>
        private EngineShaderResourceView clouds1;
        /// <summary>
        /// Second cloud layer
        /// </summary>
        private EngineShaderResourceView clouds2;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState samplerLinear;
        /// <summary>
        /// Anisotropic sampler
        /// </summary>
        private readonly EngineSamplerState samplerAnisotropic;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public CloudsPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(CloudsPs), "main", ForwardRenderingResources.Clouds_ps, HelperShaders.PSProfile);

            samplerLinear = BuiltInShaders.GetSamplerLinear();
            samplerAnisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~CloudsPs()
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
        /// Sets per cloud constant buffer
        /// </summary>
        public void SetPerCloudConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerCloud = constantBuffer;
        }
        /// <summary>
        /// Sets the first cloud layer
        /// </summary>
        /// <param name="clouds1">Cloud layer</param>
        public void SetFirstCloudLayer(EngineShaderResourceView clouds1)
        {
            this.clouds1 = clouds1;
        }
        /// <summary>
        /// Sets the second cloud layer
        /// </summary>
        /// <param name="clouds2">Cloud layer</param>
        public void SetSecondCloudLayer(EngineShaderResourceView clouds2)
        {
            this.clouds2 = clouds2;
        }

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerCloud,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                clouds1,
                clouds2,
            };

            dc.SetPixelShaderResourceViews(0, rv);

            var ss = new[]
            {
                samplerLinear,
                samplerAnisotropic,
            };

            dc.SetPixelShaderSamplers(0, ss);
        }
    }
}
