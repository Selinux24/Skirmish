using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Hull shader description
    /// </summary>
    public class EngineHullShader : IEngineHullShader
    {
        /// <summary>
        /// Hull shader
        /// </summary>
        private readonly HullShader shader;
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
        /// <param name="hullShader">Hull shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineHullShader(string name, HullShader hullShader, byte[] byteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A hull shader name must be specified.");
            shader = hullShader ?? throw new ArgumentNullException(nameof(hullShader), "A hull shader must be specified.");
            shaderByteCode = byteCode ?? throw new ArgumentNullException(nameof(byteCode), "The hull shader byte code must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineHullShader()
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
        internal HullShader GetShader()
        {
            return shader;
        }
    }
}
