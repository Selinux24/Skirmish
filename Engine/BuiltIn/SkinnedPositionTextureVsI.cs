﻿using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Skinned position texture instanced vertex shader 
    /// </summary>
    public class SkinnedPositionTextureVsI : IDisposable
    {
        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EngineVertexShader Shader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public SkinnedPositionTextureVsI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionTexture_Skinned_I_Cso == null;
            var bytes = Resources.Vs_PositionTexture_Skinned_I_Cso ?? Resources.Vs_PositionTexture_Skinned_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(SkinnedPositionTextureVsI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(SkinnedPositionTextureVsI), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkinnedPositionTextureVsI()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
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
            }
        }

        /// <summary>
        /// Sets the vertex shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSGlobal(),
                BuiltInShaders.GetVSPerFrame(),
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPalette(),
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
