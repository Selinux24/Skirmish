
namespace Engine.BuiltIn.Forward
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialTexture> cbPerMaterial;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;
        /// <summary>
        /// Anisotropic sampler
        /// </summary>
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionNormalTextureInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionNormalTextureVsI>();
            SetPixelShader<PositionNormalTexturePs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<PositionNormalTexturePs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(state.UseAnisotropic ? anisotropic : linear);
        }
    }
}
