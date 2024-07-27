using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Combine pixel shader
    /// </summary>
    public class CombinePs : IShader<EnginePixelShader>
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

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CombinePs()
        {
            Shader = BuiltInShaders.CompilePixelShader<CombinePs>("main", PostProcessResources.Combine_ps);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
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
