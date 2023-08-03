using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Fonts
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Fonts vertex shader
    /// </summary>
    public class FontsVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per text constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerText;

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
        public FontsVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(FontsVs), "main", UIRenderingResources.Font_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FontsVs()
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
        /// Sets per text constant buffer
        /// </summary>
        public void SetPerTextConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerText = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerText,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
