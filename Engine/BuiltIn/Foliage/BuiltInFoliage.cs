
namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;

    /// <summary>
    /// Foliage drawer
    /// </summary>
    public class BuiltInFoliage : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterial> cbPerMaterial;
        /// <summary>
        /// Per patch constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerPatch> cbPerPatch;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInFoliage() : base()
        {
            SetVertexShader<FoliageVs>();
            SetGeometryShader<FoliageGS>();
            SetPixelShader<FoliagePs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterial>();
            cbPerPatch = BuiltInShaders.GetConstantBuffer<PerPatch>();
        }

        /// <summary>
        /// Updates the foliage drawer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Billboard state</param>
        public void UpdateFoliage(IEngineDeviceContext dc, BuiltInFoliageState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterial.Build(state));
            dc.UpdateConstantBuffer(cbPerPatch, PerPatch.Build(state));

            var vertexShader = GetVertexShader<FoliageVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var geometryShader = GetGeometryShader<FoliageGS>();
            geometryShader?.SetPerPatchConstantBuffer(cbPerPatch);
            geometryShader?.SetRandomTexture(state.RandomTexture);

            var pixelShader = GetPixelShader<FoliagePs>();
            pixelShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
            pixelShader?.SetPerPatchConstantBuffer(cbPerPatch);
            pixelShader?.SetTextureArray(state.Texture);
            pixelShader?.SetNormalMapArray(state.NormalMaps);
        }
    }
}
