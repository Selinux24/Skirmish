using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Sprites
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Color sprite vertex shader
    /// </summary>
    public class SpriteColorVs : IBuiltInShader<EngineVertexShader>
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
        public SpriteColorVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<SpriteColorVs>("main", UIRenderingResources.SpriteColor_vs);
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
