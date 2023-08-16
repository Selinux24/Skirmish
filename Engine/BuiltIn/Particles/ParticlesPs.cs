using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;

    /// <summary>
    /// CPU particles pixel shader
    /// </summary>
    public class ParticlesPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEmitter;
        /// <summary>
        /// Texture array resource view
        /// </summary>
        private EngineShaderResourceView textureArray;
        /// <summary>
        /// Particles sampler
        /// </summary>
        private readonly EngineSamplerState samplerParticles;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticlesPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<ParticlesPs>("main", ForwardRenderingResources.Particles_ps);

            var samplerDesc = EngineSamplerStateDescription.Default();
            samplerDesc.Filter = Filter.MinMagMipPoint;
            samplerDesc.AddressU = TextureAddressMode.Clamp;
            samplerDesc.AddressV = TextureAddressMode.Clamp;

            samplerParticles = BuiltInShaders.GetSamplerCustom($"{nameof(ParticlesPs)}.ParticlesSampler", samplerDesc);
        }

        /// <summary>
        /// Sets per emitter constant buffer
        /// </summary>
        public void SetPerEmitterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerEmitter = constantBuffer;
        }
        /// <summary>
        /// Sets the texture array
        /// </summary>
        /// <param name="textureArray">Texture array</param>
        public void SetTextureArray(EngineShaderResourceView textureArray)
        {
            this.textureArray = textureArray;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderConstantBuffer(0, cbPerEmitter);

            dc.SetPixelShaderResourceView(0, textureArray);

            dc.SetPixelShaderSampler(0, samplerParticles);
        }
    }
}
