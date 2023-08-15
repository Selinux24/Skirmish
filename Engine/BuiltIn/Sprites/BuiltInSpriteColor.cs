
namespace Engine.BuiltIn.Sprites
{
    using Engine.Common;

    /// <summary>
    /// Color sprite drawer
    /// </summary>
    public class BuiltInSpriteColor : BuiltInDrawer
    {
        /// <summary>
        /// Per font constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerSprite> cbPerSprite;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInSpriteColor() : base()
        {
            SetVertexShader<SpriteColorVs>();
            SetPixelShader<SpriteColorPs>();

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

            var vertexShader = GetVertexShader<SpriteColorVs>();
            vertexShader?.SetPerSpriteConstantBuffer(cbPerSprite);

            var pixelShader = GetPixelShader<SpriteColorPs>();
            pixelShader?.SetPerSpriteConstantBuffer(cbPerSprite);
        }
    }
}
