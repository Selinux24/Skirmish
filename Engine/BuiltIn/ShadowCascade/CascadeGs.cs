using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Cascade geometry shader
    /// </summary>
    public class CascadeGs : IBuiltInGeometryShader
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerCastingLight;

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
        public CascadeGs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader($"{nameof(ShadowCascade)}_{nameof(CascadeGs)}", "main", ShaderShadowCascadeResources.ShadowCascade_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~CascadeGs()
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
        /// Sets per mesh constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerCastingLightConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerCastingLight = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                cbPerCastingLight,
            };

            Graphics.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
