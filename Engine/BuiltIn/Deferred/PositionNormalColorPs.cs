using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class PositionNormalColorPs : IBuiltInShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalColorPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(PositionNormalColorPs), "main", DeferredRenderingResources.PositionNormalColor_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorPs()
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            //No resources
        }
    }
}
