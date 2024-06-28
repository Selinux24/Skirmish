using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Forward
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class PositionTexturePs : IBuiltInShader<EnginePixelShader>
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

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionTexturePs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionTexturePs>("main", ForwardRenderingResources.PositionTexture_ps);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerFrame,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceView(0, diffuseMapArray);

            dc.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
