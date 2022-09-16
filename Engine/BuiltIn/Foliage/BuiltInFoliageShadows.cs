
namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;

    /// <summary>
    /// Foliage drawer
    /// </summary>
    public class BuiltInFoliageShadows : BuiltInDrawer
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
        public BuiltInFoliageShadows(Graphics graphics) : base(graphics)
        {
            SetVertexShader<FoliageShadowsVs>();
            SetGeometryShader<FoliageShadowsGS>();
            SetPixelShader<FoliageShadowsPs>();

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

            var geometryShader = GetGeometryShader<FoliageShadowsGS>();
            geometryShader?.SetPerPatchConstantBuffer(cbPerPatch);
            geometryShader?.SetRandomTexture(state.RandomTexture);

            var pixelShader = GetPixelShader<FoliageShadowsPs>();
            pixelShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
            pixelShader?.SetTextureArray(state.Texture);
        }
    }
}
