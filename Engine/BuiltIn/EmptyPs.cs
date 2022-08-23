using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Empty pixel shader
    /// </summary>
    public class EmptyPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Shader
        /// </summary>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EmptyPs(Graphics graphics)
        {
            Graphics = graphics;
            Shader = null;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EmptyPs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
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
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {

        }
    }
}
