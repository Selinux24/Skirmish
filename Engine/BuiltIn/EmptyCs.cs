using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Empty compute shader
    /// </summary>
    public class EmptyCs : IBuiltInComputeShader
    {
        /// <inheritdoc/>
        public EngineComputeShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EmptyCs(Graphics graphics)
        {
            Graphics = graphics;
            Shader = null;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EmptyCs()
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
            // Empty shader
        }
    }
}
