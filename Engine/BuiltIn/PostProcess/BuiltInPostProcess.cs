
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
            SetVertexShader<PostProcessVs>(false);
            SetPixelShader<PostProcessPs>(false);

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Update pass state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">State</param>
        public void UpdatePass(IEngineDeviceContext dc, BuiltInPostProcessState state)
        {
            var cbPerPass = BuiltInShaders.GetConstantBuffer<PerPass>();
            cbPerPass.WriteData(PerPass.Build(state));
            dc.UpdateConstantBuffer(cbPerPass);

            var pixelShader = GetPixelShader<PostProcessPs>();
            pixelShader?.SetPerPassConstantBuffer(cbPerPass);
        }
        /// <summary>
        /// Update pass state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="sourceTexture">Source texture</param>
        /// <param name="effect">Effect</param>
        public void UpdateEffect(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect)
        {
            var cbPerEffect = BuiltInShaders.GetConstantBuffer<PerEffect>();
            cbPerEffect.WriteData(PerEffect.Build(effect));
            dc.UpdateConstantBuffer(cbPerEffect);

            var pixelShader = GetPixelShader<PostProcessPs>();
            pixelShader?.SetPerEffectConstantBuffer(cbPerEffect);
            pixelShader?.SetDiffuseMap(sourceTexture);
            pixelShader?.SetDiffseSampler(linear);
        }
    }
}
