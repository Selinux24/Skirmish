using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Cubemap
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Cubemap pixel shader
    /// </summary>
    public class CubemapPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Cubemap resource view
        /// </summary>
        private EngineShaderResourceView cubemap;
        /// <summary>
        /// Cubemap sampler
        /// </summary>
        private EngineSamplerState sampler;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CubemapPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<CubemapPs>("main", ForwardRenderingResources.Cubemap_ps);
        }

        /// <summary>
        /// Sets the cubemap
        /// </summary>
        /// <param name="cubemap">Cubemap</param>
        public void SetCubemap(EngineShaderResourceView cubemap)
        {
            this.cubemap = cubemap;
        }
        /// <summary>
        /// Sets the cubemap sampler state
        /// </summary>
        /// <param name="sampler">Sampler</param>
        public void SetCubemapSampler(EngineSamplerState sampler)
        {
            this.sampler = sampler;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderResourceView(0, cubemap);

            dc.SetPixelShaderSampler(0, sampler);
        }
    }
}
