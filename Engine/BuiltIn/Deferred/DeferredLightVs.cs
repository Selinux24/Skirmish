using Engine.Shaders.Properties;
using System;
using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Deferred light vertex shader
    /// </summary>
    public class DeferredLightVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Light constant buffer
        /// </summary>
        private IEngineConstantBuffer perLightBuffer;

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
        public DeferredLightVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(DeferredLightVs), "main", DeferredRenderingResources.DeferredLight_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DeferredLightVs()
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
        /// Sets per-light constant buffer
        /// </summary>
        /// <param name="perLightBuffer">Constant buffer</param>
        public void SetPerLightConstantBuffer(IEngineConstantBuffer perLightBuffer)
        {
            this.perLightBuffer = perLightBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext context)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                perLightBuffer,
            };

            context.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
