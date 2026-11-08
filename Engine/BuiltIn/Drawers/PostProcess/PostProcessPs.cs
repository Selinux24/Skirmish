using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Post-process pixel shader
    /// </summary>
    public class PostProcessPs : IShader<EnginePixelShader>
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

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostProcessPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PostProcessPs>("main", PostProcessResources.PostProcess_ps);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
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
        public void SetShaderResources(IEngineDeviceContext dc)
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
