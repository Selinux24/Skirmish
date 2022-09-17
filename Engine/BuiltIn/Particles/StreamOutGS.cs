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

            var so = new[]
            {
                new EngineStreamOutputElement("POSITION", 0, 0, 3, 0),
                new EngineStreamOutputElement("VELOCITY", 0, 0, 3, 0),
                new EngineStreamOutputElement("RANDOM", 0, 0, 4, 0),
                new EngineStreamOutputElement("MAX_AGE", 0, 0, 1, 0),
                new EngineStreamOutputElement("TYPE", 0, 0, 1, 0),
                new EngineStreamOutputElement("EMISSION_TIME", 0, 0, 1, 0),
            };

            Shader = graphics.CompileGeometryShaderWithStreamOut(nameof(StreamOutGS), "main", ForwardRenderingResources.Streamout_gs, HelperShaders.GSProfile, so);
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
