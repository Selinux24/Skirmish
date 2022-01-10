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
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="view">Depth stencil view</param>
        internal EngineDepthStencilView(string name, DepthStencilView view)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A depth stencil view name must be specified.");
            dsv = view ?? throw new ArgumentNullException(nameof(view), "A depth stencil view must be specified.");

            dsv.DebugName = name;
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
