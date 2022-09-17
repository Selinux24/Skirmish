using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Stream-out GPU particles geometry shader
    /// </summary>
    public class StreamOutGS : IBuiltInGeometryShader
    {
        /// <summary>
        /// Per stream out pass constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerStreamOut;

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
        public StreamOutGS(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader(nameof(StreamOutGS), "main", ForwardRenderingResources.Streamout_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~StreamOutGS()
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
        /// Sets per stream-out pass constant buffer
        /// </summary>
        public void SetPerStreamOutConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerStreamOut = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                cbPerStreamOut,
            };

            Graphics.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
