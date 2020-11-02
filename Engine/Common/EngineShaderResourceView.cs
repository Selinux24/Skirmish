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
            srv = view;
        }

        /// <summary>
        /// Updates the texture data
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="game">Game instance</param>
        /// <param name="data">New data</param>
        public void Update<T>(Game game, IEnumerable<T> data) where T : struct
        {
            if (srv == null)
            {
                return;
            }

            if (srv.Description1.Dimension == ShaderResourceViewDimension.Texture1D)
            {
                game.Graphics.UpdateTexture1D(this, data);
            }
            else if (srv.Description1.Dimension == ShaderResourceViewDimension.Texture2D)
            {
                game.Graphics.UpdateTexture2D(this, data);
            }
        }
    }
}
