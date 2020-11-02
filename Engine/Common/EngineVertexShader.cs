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
        /// Input layout
        /// </summary>
        private InputLayout layout = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shader">Vertex shader</param>
        /// <param name="layout">Input layout</param>
        internal EngineVertexShader(VertexShader shader, InputLayout layout)
        {
            this.shader = shader;
            this.layout = layout;
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

                layout?.Dispose();
                layout = null;
            }
        }
    }
}
