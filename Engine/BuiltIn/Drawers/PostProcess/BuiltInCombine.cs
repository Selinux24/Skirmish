
namespace Engine.BuiltIn.Drawers.PostProcess
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Combine textures drawer
    /// </summary>
    public class BuiltInCombine : BuiltInDrawer
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private readonly CombinePs pixelShader;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState sSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInCombine(Game game) : base(game)
        {
            SetVertexShader<PostProcessVs>(false);
            pixelShader = SetPixelShader<CombinePs>(false);

            sSampler = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        public void Update(EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            pixelShader.SetTextures(texture1, texture2);
            pixelShader.SetDiffseSampler(sSampler);
        }
    }
}
