﻿
namespace Engine.BuiltIn.Drawers.Forward
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Common;
    using Engine.Common;

    /// <summary>
    /// Skinned position-texture instanced drawer
    /// </summary>
    public class BuiltInPositionTextureSkinnedInstanced : BuiltInDrawer
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
        public BuiltInPositionTextureSkinnedInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionTextureSkinnedVsI>();
            SetPixelShader<PositionTexturePs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionTextureSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<PositionTexturePs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(state.UseAnisotropic ? anisotropic : linear);
        }
    }
}
