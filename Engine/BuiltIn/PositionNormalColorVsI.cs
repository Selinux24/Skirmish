﻿using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color instanced vertex shader
    /// </summary>
    public class PositionNormalColorVsI : IDisposable
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
        public PositionNormalColorVsI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionNormalColor_I_Cso == null;
            var bytes = Resources.Vs_PositionNormalColor_I_Cso ?? Resources.Vs_PositionNormalColor_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionNormalColorVsI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionNormalColorVsI), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorVsI()
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

            Graphics.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPalette());
        }
    }
}
