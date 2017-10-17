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
        /// Gets the depth stencil
        /// </summary>
        /// <returns></returns>
        public DepthStencilView GetDepthStencil()
        {
            return this.dsv;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (this.dsv != null)
            {
                this.dsv.Dispose();
                this.dsv = null;
            }
        }
    }
}
