using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Texture view
    /// </summary>
    public class EngineShaderResourceView : IDisposable
    {
        /// <summary>
        /// Shader resource view
        /// </summary>
        private ShaderResourceView1 srv = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public EngineShaderResourceView()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="srv">Shader resource view</param>
        public EngineShaderResourceView(ShaderResourceView1 srv)
        {
            this.srv = srv;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineShaderResourceView()
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
                this.srv?.Dispose();
                this.srv = null;
            }
        }

        /// <summary>
        /// Get the internal shader resource view
        /// </summary>
        /// <returns>Returns the internal shader resource view</returns>
        public ShaderResourceView1 GetResource()
        {
            return this.srv;
        }
        /// <summary>
        /// Sets the internal shader resource view
        /// </summary>
        /// <param name="view">Resource view</param>
        public void SetResource(ShaderResourceView1 view)
        {
            this.srv = view;
        }
    }
}
