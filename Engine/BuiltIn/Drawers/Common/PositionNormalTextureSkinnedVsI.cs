using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Common
{
    /// <summary>
    /// Skinned position normal texture instanced vertex shader
    /// </summary>
    public class PositionNormalTextureSkinnedVsI : IShader<EngineVertexShader>
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
        public PositionNormalTextureSkinnedVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionNormalTextureSkinnedVsI>("main", CommonResources.PositionNormalTextureSkinnedI_vs);
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

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPaletteResourceView(),
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            dc.SetVertexShaderResourceViews(0, rv);
        }
    }
}
