using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Common
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Position normal texture tangent instanced vertex shader
    /// </summary>
    public class PositionNormalTextureTangentVsI : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionNormalTextureTangentVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionNormalTextureTangentVsI>("main", CommonResources.PositionNormalTextureTangentI_vs);
        }

        /// <summary>
        /// Sets per material constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerMaterialConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMaterial = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerMaterial,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            dc.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPaletteResourceView());
        }
    }
}
