using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;

    /// <summary>
    /// Stream-out GPU particles geometry shader
    /// </summary>
    public class StreamOutGS : IBuiltInShader<EngineGeometryShader>
    {
        /// <summary>
        /// Per stream out pass constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerStreamOut;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamOutGS()
        {
            var so = new[]
            {
                new EngineStreamOutputElement("POSITION", 0, 0, 3, 0),
                new EngineStreamOutputElement("VELOCITY", 0, 0, 3, 0),
                new EngineStreamOutputElement("RANDOM", 0, 0, 4, 0),
                new EngineStreamOutputElement("MAX_AGE", 0, 0, 1, 0),
                new EngineStreamOutputElement("TYPE", 0, 0, 1, 0),
                new EngineStreamOutputElement("EMISSION_TIME", 0, 0, 1, 0),
            };

            Shader = BuiltInShaders.CompileGeometryShaderWithStreamOut<StreamOutGS>("main", ForwardRenderingResources.Streamout_gs, so);
        }

        /// <summary>
        /// Sets per stream-out pass constant buffer
        /// </summary>
        public void SetPerStreamOutConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerStreamOut = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerStreamOut,
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
