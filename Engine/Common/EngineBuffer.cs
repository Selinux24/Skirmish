using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using Engine.Helpers;
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
        public Buffer VertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        public VertexBufferBinding[] VertexBufferBinding;
        /// <summary>
        /// Input layouts
        /// </summary>
        public List<InputLayout> InputLayouts = new List<InputLayout>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="data">Data</param>
        /// <param name="dynamic">Dynamic flag</param>
        public EngineBuffer(Graphics graphics, string name, T[] data, bool dynamic)
        {
            if (dynamic)
            {
                this.VertexBuffer = graphics.CreateVertexBufferWrite<T>(name, data);
            }
            else
            {
                this.VertexBuffer = graphics.CreateVertexBufferImmutable<T>(name, data);
            }

            this.VertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.VertexBuffer, default(T).GetStride(), 0),
            };
        }

        /// <summary>
        /// Adds a new input layout
        /// </summary>
        /// <param name="input">Input layout</param>
        public void AddInputLayout(InputLayout input)
        {
            this.InputLayouts.Add(input);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);

            Helper.Dispose(this.InputLayouts);
        }
    }
}
