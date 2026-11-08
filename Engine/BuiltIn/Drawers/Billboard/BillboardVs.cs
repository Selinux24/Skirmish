using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Billboard
{
    /// <summary>
    /// Billboards vertex shader
    /// </summary>
    public class BillboardVs : IShader<EngineVertexShader>
    {
        /// <summary>
        /// Per billboard constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerBillboard;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BillboardVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<BillboardVs>("main", ForwardRenderingResources.Billboard_vs);
        }

        /// <summary>
        /// Sets per billboard constant buffer
        /// </summary>
        public void SetPerBillboardConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerBillboard = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                cbPerBillboard,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            dc.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPaletteResourceView());
        }
    }
}
