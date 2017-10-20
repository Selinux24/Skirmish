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
        private List<RenderTargetView1> rtv = new List<RenderTargetView1>();

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
        /// <param name="rtv">Render target view</param>
        internal EngineRenderTargetView(RenderTargetView1 rtv)
        {
            this.rtv.Add(rtv);
        }
        /// <summary>
        /// Adds a new Render Target to the collection
        /// </summary>
        /// <param name="rtv">Render target view</param>
        internal void Add(RenderTargetView1 rtv)
        {
            this.rtv.Add(rtv);
        }

        /// <summary>
        /// Gets the render target
        /// </summary>
        /// <returns>Returns the internal render target</returns>
        public RenderTargetView1 GetRenderTarget()
        {
            return this.rtv[0];
        }
        /// <summary>
        /// Gets the render targets
        /// </summary>
        /// <returns>Returns the internal render target list</returns>
        public RenderTargetView1[] GetRenderTargets()
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
