using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using SharpDX.Direct3D;
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
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public EngineShaderResourceView(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A shader resource name must be specified.");
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="view">Shader resource view</param>
        public EngineShaderResourceView(string name, ShaderResourceView1 view)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A shader resource name must be specified.");

            SetResource(view);
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
                srv?.Dispose();
                srv = null;
            }
        }

        /// <summary>
        /// Get the internal shader resource view
        /// </summary>
        /// <returns>Returns the internal shader resource view</returns>
        internal ShaderResourceView1 GetResource()
        {
            return srv;
        }
        /// <summary>
        /// Sets the internal shader resource view
        /// </summary>
        /// <param name="view">Resource view</param>
        internal void SetResource(ShaderResourceView1 view)
        {
            srv = view ?? throw new ArgumentNullException(nameof(view), "A shader resource must be specified.");

            srv.DebugName = Name;
        }

        /// <summary>
        /// Updates the texture data
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="data">New data</param>
        public void Update<T>(EngineDeviceContext dc, IEnumerable<T> data) where T : struct
        {
            if (srv == null)
            {
                return;
            }

            if (srv.Description1.Dimension == ShaderResourceViewDimension.Texture1D)
            {
                dc.UpdateTexture1D(this, data);
            }
            else if (srv.Description1.Dimension == ShaderResourceViewDimension.Texture2D)
            {
                dc.UpdateTexture2D(this, data);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(EngineShaderResourceView)} {Name}";
        }
    }
}
