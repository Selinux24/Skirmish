using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// CPU particles vertex shader
    /// </summary>
    public class ParticlesVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per emitter constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerEmitter;

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
        public ParticlesVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(ParticlesVs), "main", ForwardRenderingResources.Particles_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ParticlesVs()
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
        /// Sets per emitter constant buffer
        /// </summary>
        public void SetPerEmitterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerEmitter = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerEmitter,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
