using System;

namespace Engine.Common
{
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
        /// <param name="srv">Shader resource view</param>
        public EngineShaderResourceView(ShaderResourceView1 srv)
        {
            this.srv = srv;
        }

        /// <summary>
        /// Get the internal shader resource view
        /// </summary>
        /// <returns>Returns the internal shader resource view</returns>
        public ShaderResourceView1 GetResource()
        {
            return this.srv;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (this.srv != null)
            {
                this.srv.Dispose();
                this.srv = null;
            }
        }
    }
}
