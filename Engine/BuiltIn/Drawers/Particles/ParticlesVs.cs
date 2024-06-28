using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Particles
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// CPU particles vertex shader
    /// </summary>
    public class ParticlesVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per emitter constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEmitter;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticlesVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<ParticlesVs>("main", ForwardRenderingResources.Particles_vs);
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
                cbPerEmitter,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
