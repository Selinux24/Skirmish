using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Cubemap
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Skymap pixel shader
    /// </summary>
    public class SkymapPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per sky constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerSky;
        /// <summary>
        /// Texture resource view
        /// </summary>
        private EngineShaderResourceView texture;
        /// <summary>
        /// Texture sampler
        /// </summary>
        private EngineSamplerState sampler;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkymapPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<SkymapPs>("main", ForwardRenderingResources.Skymap_ps);
        }

        /// <summary>
        /// Sets per sky constant buffer
        /// </summary>
        public void SetPerSkyConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerSky = constantBuffer;
        }
        /// <summary>
        /// Sets the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(EngineShaderResourceView texture)
        {
            this.texture = texture;
        }
        /// <summary>
        /// Sets the texture sampler state
        /// </summary>
        /// <param name="sampler">Sampler</param>
        public void SetSampler(EngineSamplerState sampler)
        {
            this.sampler = sampler;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderConstantBuffer(0, cbPerSky);

            dc.SetPixelShaderResourceView(0, texture);

            dc.SetPixelShaderSampler(0, sampler);
        }
    }
}
