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
        /// <param name="graphics">Graphics</param>
        /// <param name="resource">Resource</param>
        /// <param name="description">Depth stencil view description</param>
        public EngineDepthStencilView(Graphics graphics, Resource resource, DepthStencilViewDescription description)
        {
            this.dsv = new DepthStencilView(graphics.Device, resource, description);
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
