
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
        public BuiltInSpriteColor(Graphics graphics) : base(graphics)
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
        public void UpdateSprite(EngineDeviceContext dc, BuiltInSpriteState state)
        {
            cbPerSprite.WriteData(dc, PerSprite.Build(state));

            var vertexShader = GetVertexShader<SpriteColorVs>();
            vertexShader?.SetPerSpriteConstantBuffer(cbPerSprite);

            var pixelShader = GetPixelShader<SpriteColorPs>();
            pixelShader?.SetPerSpriteConstantBuffer(cbPerSprite);
        }
    }
}
