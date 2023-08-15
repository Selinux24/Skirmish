﻿using Engine.Shaders.Properties;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Cubemap
{
    using Engine.Common;
    using Engine.Helpers;

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

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public SkymapPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(SkymapPs), "main", ForwardRenderingResources.Skymap_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkymapPs()
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
            }
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
