using System;
using System.Collections.Generic;
using System.Linq;

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
        private List<RenderTargetView1> rtvList = new();

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the render target count
        /// </summary>
        public int Count
        {
            get
            {
                return rtvList.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public EngineRenderTargetView(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A render target name must be specified.");
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="renderTarget">Render target view</param>
        internal EngineRenderTargetView(string name, RenderTargetView1 renderTarget)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A render target name must be specified.");

            Add(renderTarget);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineRenderTargetView()
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
                for (int i = 0; i < rtvList?.Count; i++)
                {
                    rtvList[i]?.Dispose();
                    rtvList[i] = null;
                }

                rtvList?.Clear();
                rtvList = null;
            }
        }

        /// <summary>
        /// Adds a new Render Target to the collection
        /// </summary>
        /// <param name="renderTarget">Render target view</param>
        internal void Add(RenderTargetView1 renderTarget)
        {
            if (renderTarget == null)
            {
                throw new ArgumentNullException(nameof(renderTarget), "A render target must be specified.");
            }

            renderTarget.DebugName = Name;
            rtvList.Add(renderTarget);
        }

        /// <summary>
        /// Gets the render target
        /// </summary>
        /// <returns>Returns the internal render target</returns>
        internal RenderTargetView1 GetRenderTarget()
        {
            return rtvList.FirstOrDefault();
        }
        /// <summary>
        /// Gets the render targets
        /// </summary>
        /// <returns>Returns the internal render target list</returns>
        internal IEnumerable<RenderTargetView1> GetRenderTargets()
        {
            return rtvList.ToArray();
        }
    }
}
