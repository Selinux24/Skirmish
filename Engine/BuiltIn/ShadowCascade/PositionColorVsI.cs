using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position color instanced vertex shader
    /// </summary>
    public class PositionColorVsI : IBuiltInVertexShader
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
        public PositionColorVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader($"{nameof(ShadowCascade)}_{nameof(PositionColorVsI)}", "main", ShaderShadowCascadeResources.PositionColorI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVsI()
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
        public void SetShaderResources()
        {
            // No shader resources
        }
    }
}
