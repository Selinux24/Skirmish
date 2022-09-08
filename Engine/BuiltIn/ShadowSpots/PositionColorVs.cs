﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.ShadowSpots
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position color vertex shader
    /// </summary>
    public class PositionColorVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMesh;

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
        public PositionColorVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionColorVs), "main", ShaderShadowBasicResources.PositionColor_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVs()
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
        /// Sets per mesh constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerMeshConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMesh = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerMesh,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
