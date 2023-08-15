﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.SkyScattering
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Sky scatering vertex shader
    /// </summary>
    public class SkyScatteringVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerObject;

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
        public SkyScatteringVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(SkyScatteringVs), "main", ForwardRenderingResources.SkyScattering_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkyScatteringVs()
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
        /// Sets per object constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerObjectConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerObject = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerObject,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
