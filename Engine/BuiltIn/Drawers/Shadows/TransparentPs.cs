using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Shadows
{
    /// <summary>
    /// Shadow transparent texture pixel shader
    /// </summary>
    public class TransparentPs : IShader<EnginePixelShader>
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
        /// Constructor
        /// </summary>
        public TransparentPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<TransparentPs>("main", ShadowRenderingResources.Transparent_ps);
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
