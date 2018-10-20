using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine buffer
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class EngineBuffer<T> : IDisposable where T : struct, IVertexData
    {
        /// <summary>
        /// Buffer
        /// </summary>
        public Buffer VertexBuffer { get; set; }
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        public VertexBufferBinding[] VertexBufferBinding { get; set; }
        /// <summary>
        /// Input layouts
        /// </summary>
        public List<InputLayout> InputLayouts { get; set; } = new List<InputLayout>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="data">Data</param>
        /// <param name="dynamic">Dynamic flag</param>
        public EngineBuffer(Graphics graphics, string name, T[] data, bool dynamic)
        {
            this.VertexBuffer = graphics.CreateVertexBuffer<T>(name, data, dynamic);

            this.VertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.VertexBuffer, default(T).GetStride(), 0),
            };
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineBuffer()
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
                if (VertexBuffer != null)
                {
                    VertexBuffer.Dispose();
                    VertexBuffer = null;
                }
                if (InputLayouts != null)
                {
                    for (int i = 0; i < InputLayouts.Count; i++)
                    {
                        InputLayouts[i]?.Dispose();
                        InputLayouts[i] = null;
                    }

                    InputLayouts.Clear();
                    InputLayouts = null;
                }
            }
        }

        /// <summary>
        /// Adds a new input layout
        /// </summary>
        /// <param name="input">Input layout</param>
        public void AddInputLayout(InputLayout input)
        {
            this.InputLayouts.Add(input);
        }
    }
}
