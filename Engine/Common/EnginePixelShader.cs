using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Pixel shader description
    /// </summary>
    public class EnginePixelShader : IDisposable, IEnginePixelShader
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private PixelShader shader = null;
        /// <summary>
        /// Shader byte code
        /// </summary>
        private readonly byte[] shaderByteCode;

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pixelShader">Pixel shader</param>
        internal EnginePixelShader(string name, PixelShader pixelShader, byte[] byteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A pixel shader name must be specified.");
            shader = pixelShader ?? throw new ArgumentNullException(nameof(pixelShader), "A pixel shader must be specified.");
            shaderByteCode = byteCode ?? throw new ArgumentNullException(nameof(byteCode), "The pixel shader byte code must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EnginePixelShader()
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
                shader?.Dispose();
                shader = null;
            }
        }

        /// <inheritdoc/>
        public byte[] GetShaderBytecode()
        {
            return shaderByteCode;
        }

        /// <summary>
        /// Gets the internal shader
        /// </summary>
        internal PixelShader GetShader()
        {
            return shader;
        }
    }
}
