using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Clouds
{
    using Engine.Common;

    /// <summary>
    /// Clouds pixel shader
    /// </summary>
    public class CloudsPs : IBuiltInShader<EnginePixelShader>
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
        /// Constructor
        /// </summary>
        public CloudsPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<CloudsPs>("main", ForwardRenderingResources.Clouds_ps);

            samplerLinear = BuiltInShaders.GetSamplerLinear();
            samplerAnisotropic = BuiltInShaders.GetSamplerAnisotropic();
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
        public void SetShaderResources(IEngineDeviceContext dc)
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
