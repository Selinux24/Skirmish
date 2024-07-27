using Engine.Common;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Post-process drawer
    /// </summary>
    public class BuiltInPostProcess : BuiltInDrawer
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private readonly PostProcessPs pixelShader;
        /// <summary>
        /// Per pass constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerPass> cbPerPass;
        /// <summary>
        /// Per effect constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerEffect> cbPerEffect;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState sSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPostProcess(Game game) : base(game)
        {
            SetVertexShader<PostProcessVs>(false);
            pixelShader = SetPixelShader<PostProcessPs>(false);

            cbPerPass = BuiltInShaders.GetConstantBuffer<PerPass>(nameof(BuiltInPostProcess), false);
            cbPerEffect = BuiltInShaders.GetConstantBuffer<PerEffect>(nameof(BuiltInPostProcess), false);

            sSampler = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Update pass state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">State</param>
        public void UpdatePass(IEngineDeviceContext dc, BuiltInPostProcessState state)
        {
            dc.UpdateConstantBuffer(cbPerPass, PerPass.Build(state));

            pixelShader.SetPerPassConstantBuffer(cbPerPass);
        }
        /// <summary>
        /// Update pass state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="sourceTexture">Source texture</param>
        /// <param name="effect">Effect</param>
        public void UpdateEffect(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect)
        {
            dc.UpdateConstantBuffer(cbPerEffect, PerEffect.Build(effect));

            pixelShader.SetPerEffectConstantBuffer(cbPerEffect);
            pixelShader.SetDiffseSampler(sSampler);
            pixelShader.SetDiffuseMap(sourceTexture);
        }
    }
}
