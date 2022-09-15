using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Texture sprite pixel shader
    /// </summary>
    public class SpriteTexturePs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per sprite constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerSprite;
        /// <summary>
        /// Texture
        /// </summary>
        private EngineShaderResourceView texture;
        /// <summary>
        /// Texture sampler
        /// </summary>
        private readonly EngineSamplerState textureSampler;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public SpriteTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(SpriteTexturePs), "main", UIRenderingResources.SpriteTexture_ps, HelperShaders.PSProfile);

            textureSampler = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteTexturePs()
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
        /// <summary>
        /// Sets the sprite texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTextureResourceView(EngineShaderResourceView texture)
        {
            this.texture = texture;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerSprite,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            Graphics.SetPixelShaderResourceView(0, texture);

            Graphics.SetPixelShaderSampler(0, textureSampler);
        }
    }
}
