﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// CPU particles vertex shader
    /// </summary>
    public class BasicCpuParticlesVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per emitter constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEmitter;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BasicCpuParticlesVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(BasicCpuParticlesVs), "main", ShaderDefaultBasicResources.CPUParticles_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicCpuParticlesVs()
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
        /// Sets per emitter constant buffer
        /// </summary>
        public void SetPerEmitterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerEmitter = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSPerFrame(),
                cbPerEmitter,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
