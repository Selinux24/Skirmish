﻿
namespace Engine.BuiltIn.Drawers.Deferred
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Common;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangentSkinnedInstanced : BuiltInDrawer
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
        public BuiltInPositionNormalTextureTangentSkinnedInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionNormalTextureTangentSkinnedVsI>();
            SetPixelShader<PositionNormalTextureTangentPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureTangentSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<PositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(state.UseAnisotropic ? anisotropic : linear);
            pixelShader?.SetNormalMap(state.Material?.NormalMap);
            pixelShader?.SetNormalSampler(state.UseAnisotropic ? anisotropic : linear);
        }
    }
}
