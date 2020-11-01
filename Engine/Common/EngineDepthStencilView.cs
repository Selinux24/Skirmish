using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Depth stencil view
    /// </summary>
    public class EngineDepthStencilView : IDisposable
    {
        /// <summary>
        /// Internal depth stencil view
        /// </summary>
        private DepthStencilView dsv = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dsv">Depth stencil view</param>
        internal EngineDepthStencilView(DepthStencilView dsv)
        {
            this.dsv = dsv;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDepthStencilView()
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
                dsv?.Dispose();
                dsv = null;
            }
        }

        /// <summary>
        /// Gets the depth stencil
        /// </summary>
        /// <returns></returns>
        internal DepthStencilView GetDepthStencil()
        {
            return dsv;
        }
    }
}
