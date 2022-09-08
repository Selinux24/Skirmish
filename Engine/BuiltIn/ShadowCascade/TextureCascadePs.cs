using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Shadow transparent texture pixel shader
    /// </summary>
    public class TextureCascadePs : IBuiltInPixelShader
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public TextureCascadePs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader($"{nameof(ShadowCascade)}_{nameof(TextureCascadePs)}", "main", ShaderShadowCascadeResources.ShadowTextureCascade_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextureCascadePs()
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
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var rv = new[]
            {
                diffuseMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
