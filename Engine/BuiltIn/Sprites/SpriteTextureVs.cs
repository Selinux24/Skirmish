using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Tetxure sprite vertex shader
    /// </summary>
    public class SpriteTextureVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per sprite constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerSprite;

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
        public SpriteTextureVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(SpriteTextureVs), "main", UIRenderingResources.SpriteTexture_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteTextureVs()
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
        /// Sets per sprite constant buffer
        /// </summary>
        public void SetPerSpriteConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerSprite = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerSprite,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
