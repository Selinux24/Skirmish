using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Cubemap vertex shader
    /// </summary>
    public class TextureVs : IBuiltInVertexShader
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
        public TextureVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(TextureVs), "main", ShaderDefaultBasicResources.Texture_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextureVs()
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
                BuiltInShaders.GetVSPerFrame(),
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
