
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BasicCubemap : BuiltInDrawer<CubemapVs, CubemapPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicCubemap(Graphics graphics) : base(graphics)
        {

        }

        /// <summary>
        /// Updates the cubemap
        /// </summary>
        /// <param name="cubemap">Cubemap texture</param>
        public void Update(EngineShaderResourceView cubemap)
        {
            PixelShader.SetCubemap(cubemap);
            PixelShader.SetCubemapSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
