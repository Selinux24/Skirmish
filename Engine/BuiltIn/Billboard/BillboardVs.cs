using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Billboard
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Billboards vertex shader
    /// </summary>
    public class BillboardVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per billboard constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerBillboard;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BillboardVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(BillboardVs), "main", ForwardRenderingResources.Billboard_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BillboardVs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shader?.Dispose();
                Shader = null;
            }
        }

        /// <summary>
        /// Sets per billboard constant buffer
        /// </summary>
        public void SetPerBillboardConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerBillboard = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext dc)
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
