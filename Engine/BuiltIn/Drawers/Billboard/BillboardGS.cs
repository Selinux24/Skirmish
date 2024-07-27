using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Billboard
{
    /// <summary>
    /// Billboards geometry shader
    /// </summary>
    public class BillboardGS : IShader<EngineGeometryShader>
    {
        /// <summary>
        /// Per billboard constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerBillboard;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BillboardGS()
        {
            Shader = BuiltInShaders.CompileGeometryShader<BillboardGS>("main", ForwardRenderingResources.Billboard_gs);
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
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerBillboard,
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
