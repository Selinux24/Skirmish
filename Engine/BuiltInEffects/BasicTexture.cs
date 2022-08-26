
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Texture drawer
    /// </summary>
    public class BasicTexture : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<TextureVs>();
            SetPixelShader<TexturePs>();
        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void Update(EngineShaderResourceView texture, uint textureIndex)
        {
            var pixelShader = GetPixelShader<TexturePs>();
            pixelShader?.WriteCBPerFrame(textureIndex);
            pixelShader?.SetTexture(texture);
            pixelShader?.SetSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
