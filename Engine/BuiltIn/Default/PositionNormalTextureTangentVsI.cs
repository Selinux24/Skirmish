﻿using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position normal texture tangent instanced vertex shader
    /// </summary>
    public class PositionNormalTextureTangentVsI : IBuiltInVertexShader
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;

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
        public PositionNormalTextureTangentVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionNormalTextureTangentVsI), "main", ShaderDefaultBasicResources.PositionNormalTextureTangentI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureTangentVsI()
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
        /// Sets per material constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerMaterialConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMaterial = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobal(),
                BuiltInShaders.GetVSPerFrame(),
                cbPerMaterial,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            Graphics.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPalette());
        }
    }
}
