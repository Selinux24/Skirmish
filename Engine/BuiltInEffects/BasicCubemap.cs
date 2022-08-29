
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Cubemap;
    using Engine.Common;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BasicCubemap : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicCubemap(Graphics graphics) : base(graphics)
        {
            SetVertexShader<CubemapVs>();
            SetPixelShader<CubemapPs>();
        }

        /// <summary>
        /// Updates the cubemap
        /// </summary>
        /// <param name="cubemap">Cubemap texture</param>
        public void Update(EngineShaderResourceView cubemap)
        {
            var pixelShader = GetPixelShader<CubemapPs>();

            pixelShader?.SetCubemap(cubemap);
            pixelShader?.SetCubemapSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
