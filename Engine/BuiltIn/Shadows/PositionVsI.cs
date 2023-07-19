using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position instanced vertex shader
    /// </summary>
    public class PositionVsI : IBuiltInVertexShader
    {
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
        public PositionVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader($"{nameof(Shadows)}_{nameof(PositionVsI)}", "main", ShadowRenderingResources.PositionI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionVsI()
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

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext dc)
        {
            // No shader resources
        }
    }
}
