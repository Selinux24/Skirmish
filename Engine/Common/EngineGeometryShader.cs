using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Geometry shader description
    /// </summary>
    public class EngineGeometryShader : IEngineGeometryShader
    {
        /// <summary>
        /// Geometry shader
        /// </summary>
        private readonly GeometryShader shader;
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
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineGeometryShader(string name, GeometryShader vertexShader, byte[] byteCode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A geometry shader name must be specified.");
            shader = vertexShader ?? throw new ArgumentNullException(nameof(vertexShader), "A geometry shader must be specified.");
            shaderByteCode = byteCode ?? throw new ArgumentNullException(nameof(byteCode), "The geometry shader byte code must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineGeometryShader()
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
        internal GeometryShader GetShader()
        {
            return shader;
        }
    }
}
