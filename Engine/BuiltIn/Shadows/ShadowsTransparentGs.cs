using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Point geometry shader
    /// </summary>
    public class ShadowsTransparentGs : IBuiltInGeometryShader
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
        public ShadowsTransparentGs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader($"{nameof(Shadows)}_{nameof(ShadowsTransparentGs)}", "main", ShadowRenderingResources.ShadowTransparent_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowsTransparentGs()
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
