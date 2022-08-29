using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine vertex buffer
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class EngineVertexBuffer<T> : IEngineVertexBuffer where T : struct, IVertexData
    {
        /// <summary>
        /// Graphics
        /// </summary>
        protected Graphics Graphics { get; private set; }
        /// <summary>
        /// Buffer
        /// </summary>
        protected Buffer VertexBuffer { get; set; }
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBinding { get; set; }
        /// <summary>
        /// Input layout
        /// </summary>
        protected InputLayout InputLayout { get; set; }
        /// <summary>
        /// Buffer slot
        /// </summary>
        protected int BufferSlot { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="data">Data</param>
        /// <param name="dynamic">Dynamic flag</param>
        public EngineVertexBuffer(Graphics graphics, string name, IEnumerable<T> data, bool dynamic)
        {
            Graphics = graphics;

            Name = name;

            VertexBuffer = graphics.CreateVertexBuffer(name, data, dynamic);

            VertexBufferBinding = new[]
            {
                new VertexBufferBinding(VertexBuffer, default(T).GetStride(), 0),
            };
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineVertexBuffer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                VertexBuffer?.Dispose();
                VertexBuffer = null;

                InputLayout?.Dispose();
                InputLayout = null;
            }
        }

        /// <summary>
        /// Writes data to the buffer
        /// </summary>
        /// <param name="data">Data to write</param>
        public void Write(IEnumerable<T> data)
        {
            Graphics.WriteDiscardBuffer(VertexBuffer, data);
        }

        /// <inheritdoc/>
        public void CreateInputLayout(string name, byte[] signature, int bufferSlot)
        {
            var inputElements = default(T).GetInput(bufferSlot);
            var layout = Graphics.CreateInputLayout(name, signature, inputElements);
            BufferSlot = bufferSlot;

            InputLayout?.Dispose();
            InputLayout = layout;
        }
        /// <inheritdoc/>
        public void SetInputAssembler(Topology topology)
        {
            Graphics.IASetVertexBuffers(BufferSlot, VertexBufferBinding);
            Graphics.IAInputLayout = InputLayout;
            Graphics.IAPrimitiveTopology = (PrimitiveTopology)topology;
        }
        /// <inheritdoc/>
        public void Draw(int drawCount)
        {
            Graphics.Draw(drawCount, 0);
        }
    }
}
