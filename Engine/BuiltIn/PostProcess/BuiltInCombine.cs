
namespace Engine.BuiltIn.PostProcess
{
    using Engine.Common;

    /// <summary>
    /// Combine textures drawer
    /// </summary>
    public class BuiltInCombine : BuiltInDrawer
    {
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInCombine(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PostProcessVs>();
            SetPixelShader<CombinePs>();

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        public void Update(EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            var pixelShader = GetPixelShader<CombinePs>();
            pixelShader?.SetTextures(texture1, texture2);
            pixelShader?.SetDiffseSampler(linear);
        }
    }
}
