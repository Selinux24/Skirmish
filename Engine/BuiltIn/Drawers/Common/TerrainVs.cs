using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Common
{
    /// <summary>
    /// Terrain vertex shader
    /// </summary>
    public class TerrainVs : IShader<EngineVertexShader>
    {
        /// <summary>
        /// Per terrain constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerTerrain;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TerrainVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<TerrainVs>("main", CommonResources.Terrain_vs);
        }

        /// <summary>
        /// Sets per terrain constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerTerrainConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerTerrain = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerTerrain,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            dc.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPaletteResourceView());
        }
    }
}
