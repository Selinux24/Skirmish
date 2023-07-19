
namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Transparent point shadow skinned position drawer
    /// </summary>
    public class BuiltInTransparentPositionSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSkinned> cbPerMesh;
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
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsSkinned">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BuiltInTransparentPositionSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionSkinnedVs>();
            SetGeometryShader<ShadowsTransparentGs>();
            SetPixelShader<TransparentPs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSkinned>();
            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLight.Build(context));
            context.DeviceContext.UpdateConstantBuffer(cbPerLight);

            var geometryShader = GetGeometryShader<ShadowsTransparentGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
        /// <inheritdoc/>
        public override void UpdateMesh(EngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSkinned.Build(state));
            dc.UpdateConstantBuffer(cbPerMesh);

            var vertexShader = GetVertexShader<PositionTextureSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialTexture.Build(state));
            dc.UpdateConstantBuffer(cbPerMaterial);

            var vertexShader = GetVertexShader<PositionTextureSkinnedVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<TransparentPs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffuseSampler(linear);
        }
    }
}
