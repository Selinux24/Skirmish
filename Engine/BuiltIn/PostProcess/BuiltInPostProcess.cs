
namespace Engine.BuiltIn.PostProcess
{
    using Engine.Common;

    /// <summary>
    /// Post-process drawer
    /// </summary>
    public class BuiltInPostProcess : BuiltInDrawer
    {
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPostProcess(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PostProcessVs>();
            SetPixelShader<PostProcessPs>();

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Update pass state
        /// </summary>
        /// <param name="state">State</param>
        public void UpdatePass(BuiltInPostProcessState state)
        {
            var cbPerPass = BuiltInShaders.GetConstantBuffer<PerPass>();
            cbPerPass.WriteData(PerPass.Build(state));

            var pixelShader = GetPixelShader<PostProcessPs>();
            pixelShader?.SetPerPassConstantBuffer(cbPerPass);
            pixelShader?.SetDiffuseMap(state.RenderTargetTexture);
            pixelShader?.SetDiffseSampler(linear);
        }
    }
}
