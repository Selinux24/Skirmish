using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Texture view
    /// </summary>
    public class EngineTexture : IDisposable
    {
        /// <summary>
        /// Shader resource view
        /// </summary>
        private ShaderResourceView srv = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="srv">Shader resource view</param>
        public EngineTexture(ShaderResourceView srv)
        {
            this.srv = srv;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="texture">Texture</param>
        public EngineTexture(Graphics graphics, Resource texture)
        {
            this.srv = new ShaderResourceView(graphics.Device, texture);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="texture">Texture</param>
        /// <param name="description">Texture description</param>
        public EngineTexture(Graphics graphics, Resource texture, ShaderResourceViewDescription description)
        {
            this.srv = new ShaderResourceView(graphics.Device, texture, description);
        }

        /// <summary>
        /// Generate mips
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public void GenerateMips(Graphics graphics)
        {
            graphics.DeviceContext.GenerateMips(this.srv);
        }
        /// <summary>
        /// Get the internal shader resource view
        /// </summary>
        /// <returns>Returns the internal shader resource view</returns>
        public ShaderResourceView GetResource()
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
