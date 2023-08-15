
namespace Engine.BuiltIn.Cubemap
{
    using Engine.Common;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class BuiltInCubemap : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInCubemap() : base()
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
