
namespace Engine.BuiltIn.Deferred
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Terrain drawer
    /// </summary>
    public class BuiltInTerrain : BuiltInDrawer
    {
        /// <summary>
        /// Per terrain constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerTerrain> cbPerTerrain;
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
        public BuiltInTerrain() : base()
        {
            SetVertexShader<TerrainVs>();
            SetPixelShader<TerrainPs>();

            cbPerTerrain = BuiltInShaders.GetConstantBuffer<PerTerrain>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <summary>
        /// Updates the terrain
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Terrain state</param>
        public void Update(IEngineDeviceContext dc, BuiltInTerrainState state)
        {
            cbPerTerrain.WriteData(PerTerrain.Build(state));
            dc.UpdateConstantBuffer(cbPerTerrain);

            var vertexShader = GetVertexShader<TerrainVs>();
            vertexShader?.SetPerTerrainConstantBuffer(cbPerTerrain);

            var pixelShader = GetPixelShader<TerrainPs>();
            pixelShader?.SetPerTerrainConstantBuffer(cbPerTerrain);
            pixelShader?.SetAlphaMap(state.AlphaMap);
            pixelShader?.SetNormalMap(state.MormalMap);
            pixelShader?.SetColorTexture(state.ColorTexture);
            pixelShader?.SetLowResolutionTexture(state.LowResolutionTexture);
            pixelShader?.SetHighResolutionTexture(state.HighResolutionTexture);
            pixelShader?.SetDiffuseSampler(state.UseAnisotropic ? anisotropic : linear);
            pixelShader?.SetNormalSampler(linear);
        }
    }
}
