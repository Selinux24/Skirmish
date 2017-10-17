using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Pixel shader description
    /// </summary>
    public class EnginePixelShader : IDisposable
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private PixelShader shader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shader">Pixel shader</param>
        internal EnginePixelShader(PixelShader shader)
        {
            this.shader = shader;
        }
        
        /// <summary>
        /// Resource dispose
        /// </summary>
        public void Dispose()
        {
            if (this.shader != null)
            {
                this.shader.Dispose();
                this.shader = null;
            }
        }
    }
}
