using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Render target
    /// </summary>
    public class EngineRenderTargetView : IDisposable
    {
        /// <summary>
        /// Render target list
        /// </summary>
        private List<RenderTargetView> rtv = new List<RenderTargetView>();

        /// <summary>
        /// Gets the render target count
        /// </summary>
        public int Count
        {
            get
            {
                return this.rtv.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EngineRenderTargetView()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="resource">Resource</param>
        public EngineRenderTargetView(Graphics graphics, Resource resource)
        {
            this.rtv.Add(new RenderTargetView(graphics.Device, resource));
        }
        /// <summary>
        /// Adds a new Render Target to the collection
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="resource">Resource</param>
        public void Add(Graphics graphics, Resource resource)
        {
            this.rtv.Add(new RenderTargetView(graphics.Device, resource));
        }

        /// <summary>
        /// Gets the render target
        /// </summary>
        /// <returns>Returns the internal render target</returns>
        public RenderTargetView GetRenderTarget()
        {
            return this.rtv[0];
        }
        /// <summary>
        /// Gets the render targets
        /// </summary>
        /// <returns>Returns the internal render target list</returns>
        public RenderTargetView[] GetRenderTargets()
        {
            return this.rtv.ToArray();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.rtv);
        }
    }
}
