using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class PositionTexturePs : IShader<EnginePixelShader>
    {
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
            Shader = BuiltInShaders.CompilePixelShader<PositionTexturePs>("main", DeferredRenderingResources.PositionTexture_ps);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
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
            dc.SetPixelShaderResourceView(0, diffuseMapArray);

            dc.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
