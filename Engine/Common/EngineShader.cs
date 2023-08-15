using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Generic shader description
    /// </summary>
    public class EngineShader<T> : IEngineShader where T : DeviceChild
    {
        /// <summary>
        /// Generic shader
        /// </summary>
        private readonly T shader;
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
        /// <param name="shader">Shader</param>
        /// <param name="shaderByteCode">Shader byte code</param>
        internal EngineShader(string name, T shader, byte[] shaderByteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A shader name must be specified.");
            this.shader = shader ?? throw new ArgumentNullException(nameof(shader), "A shader must be specified.");
            this.shaderByteCode = shaderByteCode ?? throw new ArgumentNullException(nameof(shaderByteCode), "The shader byte code must be specified.");

            this.shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineShader()
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
        internal T GetShader()
        {
            return shader;
        }
    }
}
