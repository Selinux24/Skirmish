using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// CPU particles pixel shader
    /// </summary>
    public class ParticlesPs : IBuiltInPixelShader
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
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public ParticlesPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(ParticlesPs), "main", ForwardRenderingResources.Particles_ps, HelperShaders.PSProfile);

            var samplerDesc = EngineSamplerStateDescription.Default();
            samplerDesc.Filter = Filter.MinMagMipPoint;
            samplerDesc.AddressU = TextureAddressMode.Clamp;
            samplerDesc.AddressV = TextureAddressMode.Clamp;

            samplerParticles = EngineSamplerState.Create(graphics, $"{nameof(ParticlesPs)}.ParticlesSampler", samplerDesc);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticlesPs()
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

                samplerParticles?.Dispose();
            }
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
