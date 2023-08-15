
namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;

    /// <summary>
    /// Texture sprite drawer
    /// </summary>
    public class BuiltInSpriteTexture : BuiltInDrawer
    {
        /// <summary>
        /// Per font constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerSprite> cbPerSprite;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInSpriteTexture() : base()
        {
            SetVertexShader<SpriteTextureVs>();
            SetPixelShader<SpriteTexturePs>();

            cbPerSprite = BuiltInShaders.GetConstantBuffer<PerSprite>();
        }

        /// <summary>
        /// Updates the sprite drawer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        public void UpdateSprite(IEngineDeviceContext dc, BuiltInSpriteState state)
        {
            cbPerSprite.WriteData(PerSprite.Build(state));
            dc.UpdateConstantBuffer(cbPerSprite);

            var vertexShader = GetVertexShader<SpriteTextureVs>();
            vertexShader?.SetPerSpriteConstantBuffer(cbPerSprite);

            var pixelShader = GetPixelShader<SpriteTexturePs>();
            pixelShader?.SetPerSpriteConstantBuffer(cbPerSprite);
            pixelShader?.SetTextureResourceView(state.Texture);
        }
    }
}
