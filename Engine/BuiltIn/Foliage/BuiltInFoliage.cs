
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
        /// <param name="graphics">Graphics</param>
        public BuiltInFoliage(Graphics graphics) : base(graphics)
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
        /// <param name="state">Billboard state</param>
        public void UpdateFoliage(BuiltInFoliageState state)
        {
            cbPerMaterial.WriteData(PerMaterial.Build(state));
            cbPerPatch.WriteData(PerPatch.Build(state));

            var vertexShader = GetVertexShader<FoliageVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var geometryShader = GetGeometryShader<FoliageGS>();
            geometryShader?.SetPerPatchConstantBuffer(cbPerPatch);
            geometryShader?.SetRandomTexture(state.RandomTexture);

            var pixelShader = GetPixelShader<FoliagePs>();
            pixelShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
            pixelShader?.SetTextureArray(state.Texture);
            pixelShader?.SetNormalMapArray(state.NormalMaps);
        }
    }
}
