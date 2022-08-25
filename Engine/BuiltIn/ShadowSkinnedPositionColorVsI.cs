using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position color instanced vertex shader
    /// </summary>
    public class ShadowSkinnedPositionColorVsI : IBuiltInVertexShader
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
        public ShadowSkinnedPositionColorVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(ShadowSkinnedPositionColorVsI), "main", ShaderShadowBasicResources.PositionColorSkinnedI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowSkinnedPositionColorVsI()
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
                BuiltInShaders.GetVSGlobal(),
                BuiltInShaders.GetVSPerFrame(),
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
