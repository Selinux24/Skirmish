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
        /// Resource dispose
        /// </summary>
        public void Dispose()
        {
            if (this.shader != null)
            {
                this.shader.Dispose();
                this.shader = null;
            }

            if (this.layout != null)
            {
                this.layout.Dispose();
                this.layout = null;
            }
        }
    }
}
