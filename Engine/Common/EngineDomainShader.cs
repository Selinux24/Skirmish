using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Domain shader description
    /// </summary>
    public class EngineDomainShader : IEngineDomainShader
    {
        /// <summary>
        /// Domain shader
        /// </summary>
        private readonly DomainShader shader;
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
        /// <param name="domainShader">Domain shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineDomainShader(string name, DomainShader domainShader, byte[] byteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A domain shader name must be specified.");
            shader = domainShader ?? throw new ArgumentNullException(nameof(domainShader), "A domain shader must be specified.");
            shaderByteCode = byteCode ?? throw new ArgumentNullException(nameof(byteCode), "The domain shader byte code must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDomainShader()
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
        internal DomainShader GetShader()
        {
            return shader;
        }
    }
}
