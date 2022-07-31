using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vertex shader description
    /// </summary>
    public class EngineVertexShader : IDisposable
    {
        /// <summary>
        /// Vertex shader
        /// </summary>
        private VertexShader shader = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="inputLayout">Input layout</param>
        internal EngineVertexShader(string name, VertexShader vertexShader)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A vertex shader name must be specified.");
            shader = vertexShader ?? throw new ArgumentNullException(nameof(vertexShader), "A vertex shader must be specified.");

            shader.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineVertexShader()
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

        /// <summary>
        /// Gets the internal shader
        /// </summary>
        internal VertexShader GetShader()
        {
            return shader;
        }
    }
}
