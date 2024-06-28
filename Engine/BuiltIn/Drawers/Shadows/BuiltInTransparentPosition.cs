
namespace Engine.BuiltIn.Drawers.Shadows
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Transparent point shadow position drawer
    /// </summary>
    public class BuiltInTransparentPosition : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialTexture> cbPerMaterial;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInTransparentPosition(Game game) : base(game)
        {
            SetVertexShader<PositionTextureVs>();
            SetGeometryShader<ShadowsTransparentGs>();
            SetPixelShader<TransparentPs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            context.DeviceContext.UpdateConstantBuffer(cbPerLight, PerCastingLight.Build(context));

            var geometryShader = GetGeometryShader<ShadowsTransparentGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
        /// <inheritdoc/>
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            dc.UpdateConstantBuffer(cbPerMesh, PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionTextureVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionTextureVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<TransparentPs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffuseSampler(linear);
        }
    }
}
