﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Decals
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Decals vertex shader
    /// </summary>
    public class DecalsVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per decal constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerDecal;

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
        public DecalsVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(DecalsVs), "main", ForwardRenderingResources.Decal_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DecalsVs()
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
        /// Sets per decal constant buffer
        /// </summary>
        public void SetPerDecalConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerDecal = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerDecal,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
