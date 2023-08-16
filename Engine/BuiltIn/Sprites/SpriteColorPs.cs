using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;

    /// <summary>
    /// Color sprite pixel shader
    /// </summary>
    public class SpriteColorPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per sprite constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerSprite;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteColorPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<SpriteColorPs>("main", UIRenderingResources.SpriteColor_ps);
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

            dc.SetPixelShaderConstantBuffers(0, cb);
        }
    }
}
