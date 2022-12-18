using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Compute shader description
    /// </summary>
    public class EngineComputeShader : IEngineComputeShader
    {
        /// <summary>
        /// Compute shader
        /// </summary>
        private readonly ComputeShader shader;
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
        /// <param name="computeShader">Compute shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineComputeShader(string name, ComputeShader computeShader, byte[] byteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A compute shader name must be specified.");
            shader = computeShader ?? throw new ArgumentNullException(nameof(computeShader), "A compute shader must be specified.");
            shaderByteCode = byteCode ?? throw new ArgumentNullException(nameof(byteCode), "The compute shader byte code must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineComputeShader()
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
        internal ComputeShader GetShader()
        {
            return shader;
        }
    }
}
