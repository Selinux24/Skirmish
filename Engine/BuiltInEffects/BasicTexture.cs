
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BasicTexture : BuiltInDrawer<TextureVs, EmptyGs, TexturePs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicTexture(Graphics graphics) : base(graphics)
        {

        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void Update(EngineShaderResourceView texture, uint textureIndex)
        {
            PixelShader.WriteCBPerFrame(textureIndex);
            PixelShader.SetTexture(texture);
            PixelShader.SetSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
