using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using SharpDX;
    using SharpDX.Mathematics.Interop;

    /// <summary>
    /// Graphic viewport management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Sets the default viewport
        /// </summary>
        public void SetDefaultViewport()
        {
            SetViewport(Viewport);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(Viewport viewport)
        {
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(ViewportF viewport)
        {
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<Viewport> viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<ViewportF> viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        private void SetViewPorts(IEnumerable<RawViewportF> viewports)
        {
            if (Helper.CompareEnumerables(currentViewports, viewports))
            {
                return;
            }

            immediateContext.Rasterizer.SetViewports(viewports.ToArray());

            currentViewports = viewports;
        }
    }
}
