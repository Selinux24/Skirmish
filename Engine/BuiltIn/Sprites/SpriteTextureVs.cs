using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;

    /// <summary>
    /// Tetxure sprite vertex shader
    /// </summary>
    public class SpriteTextureVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per sprite constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerSprite;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteTextureVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<SpriteTextureVs>("main", UIRenderingResources.SpriteTexture_vs);
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
