using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Particles
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// CPU particles geometry shader
    /// </summary>
    public class ParticlesGS : IBuiltInShader<EngineGeometryShader>
    {
        /// <summary>
        /// Per emitter constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEmitter;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticlesGS()
        {
            Shader = BuiltInShaders.CompileGeometryShader<ParticlesGS>("main", ForwardRenderingResources.Particles_gs);
        }

        /// <summary>
        /// Sets per emitter constant buffer
        /// </summary>
        public void SetPerEmitterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerEmitter = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerEmitter,
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
