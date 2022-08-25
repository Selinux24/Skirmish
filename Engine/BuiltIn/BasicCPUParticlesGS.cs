﻿using Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// CPU particles geometry shader
    /// </summary>
    public class BasicCPUParticlesGS : IBuiltInGeometryShader
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
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BasicCPUParticlesGS(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader(nameof(BasicCPUParticlesGS), "main", ShaderDefaultBasicResources.CPUParticles_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicCPUParticlesGS()
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

            Graphics.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
