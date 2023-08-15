using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Color sprite vertex shader
    /// </summary>
    public class SpriteColorVs : IBuiltInShader<EngineVertexShader>
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
        public SpriteColorVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(SpriteColorVs), "main", UIRenderingResources.SpriteColor_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteColorVs()
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
