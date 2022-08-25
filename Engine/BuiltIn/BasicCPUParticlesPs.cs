﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// CPU particles pixel shader
    /// </summary>
    public class BasicCPUParticlesPs : IBuiltInPixelShader
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
        public BasicCPUParticlesPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(BasicCPUParticlesPs), "main", ShaderDefaultBasicResources.CPUParticles_ps, HelperShaders.PSProfile);

            var samplerDesc = new EngineSamplerStateDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
            };
            samplerParticles = EngineSamplerState.Create(graphics, $"{nameof(BasicCPUParticlesPs)}.ParticlesSampler", samplerDesc);
            samplerParticles = EngineSamplerState.Create(graphics, $"{nameof(BasicCPUParticlesPs)}.ParticlesSampler", samplerDesc);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicCPUParticlesPs()
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
        public void SetShaderResources()
        {
            Graphics.SetPixelShaderConstantBuffer(0, cbPerEmitter);

            Graphics.SetPixelShaderResourceView(0, textureArray);

            Graphics.SetPixelShaderSampler(0, samplerParticles);
        }
    }
}
