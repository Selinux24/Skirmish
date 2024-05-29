
namespace Engine.BuiltIn.Deferred
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture-tangent drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangent : BuiltInDrawer
    {
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
        /// Anisotropic sampler
        /// </summary>
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionNormalTextureTangent(Game game) : base(game)
        {
            SetVertexShader<PositionNormalTextureTangentVs>();
            SetPixelShader<PositionNormalTextureTangentPs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialTexture>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            dc.UpdateConstantBuffer(cbPerMesh, PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureTangentVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureTangentVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<PositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(state.UseAnisotropic ? anisotropic : linear);
            pixelShader?.SetNormalMap(state.Material?.NormalMap);
            pixelShader?.SetNormalSampler(state.UseAnisotropic ? anisotropic : linear);
        }
    }
}
