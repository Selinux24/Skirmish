using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Pixel shader description
    /// </summary>
    public class EnginePixelShader : IDisposable
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private PixelShader shader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shader">Pixel shader</param>
        internal EnginePixelShader(PixelShader shader)
        {
            this.shader = shader;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EnginePixelShader()
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
                shader?.Dispose();
                shader = null;
            }
        }
    }
}
