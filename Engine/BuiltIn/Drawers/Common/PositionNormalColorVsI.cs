using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Common
{
    /// <summary>
    /// Position normal color instanced vertex shader
    /// </summary>
    public class PositionNormalColorVsI : IShader<EngineVertexShader>
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
        public PositionNormalColorVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionNormalColorVsI>("main", CommonResources.PositionNormalColorI_vs);
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
