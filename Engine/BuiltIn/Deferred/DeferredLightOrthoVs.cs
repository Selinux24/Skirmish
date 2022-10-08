﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Deferred light vertex shader
    /// </summary>
    public class DeferredLightOrthoVs : IBuiltInVertexShader
    {
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
        public DeferredLightOrthoVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(DeferredLightOrthoVs), "main", DeferredRenderingResources.DeferredLightOrtho_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DeferredLightOrthoVs()
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

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}