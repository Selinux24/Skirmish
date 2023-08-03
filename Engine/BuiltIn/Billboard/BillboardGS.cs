using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Billboard
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Billboards geometry shader
    /// </summary>
    public class BillboardGS : IBuiltInGeometryShader
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
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BillboardGS(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader(nameof(BillboardGS), "main", ForwardRenderingResources.Billboard_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BillboardGS()
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
