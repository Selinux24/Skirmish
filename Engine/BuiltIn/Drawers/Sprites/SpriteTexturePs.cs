using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Sprites
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Texture sprite pixel shader
    /// </summary>
    public class SpriteTexturePs : IBuiltInShader<EnginePixelShader>
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
        /// Constructor
        /// </summary>
        public SpriteTexturePs()
        {
            Shader = BuiltInShaders.CompilePixelShader<SpriteTexturePs>("main", UIRenderingResources.SpriteTexture_ps);

            textureSampler = BuiltInShaders.GetSamplerLinear();
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerSprite,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceView(0, texture);

            dc.SetPixelShaderSampler(0, textureSampler);
        }
    }
}
