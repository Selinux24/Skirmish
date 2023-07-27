using System;
using System.Collections.Generic;

namespace Engine.Common
{
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
        protected EngineBuffer VertexBuffer { get; set; }
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected EngineVertexBufferBinding[] VertexBufferBinding { get; set; }
        /// <summary>
        /// Stream-out binding
        /// </summary>
        protected EngineStreamOutputBufferBinding[] StreamOutBinding { get; set; }
        /// <summary>
        /// Input layout
        /// </summary>
        protected EngineInputLayout InputLayout { get; set; }
        /// <summary>
        /// Buffer slot
        /// </summary>
        protected int BufferSlot { get; set; }
        /// <summary>
        /// Parameters
        /// </summary>
        protected VertexBufferParams Parameters { get; set; }

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
        /// <param name="parameters">Vertex buffer parameters</param>
        public EngineVertexBuffer(Graphics graphics, string name, IEnumerable<T> data, VertexBufferParams parameters)
        {
            Graphics = graphics;
            Parameters = parameters;

            Name = name;

            if (parameters == VertexBufferParams.Dynamic)
            {
                VertexBuffer = graphics.CreateVertexBuffer(name, data, true);
            }
            else if (parameters == VertexBufferParams.StreamOut)
            {
                VertexBuffer = graphics.CreateStreamOutBuffer(name, data);

                StreamOutBinding = new[]
                {
                    new EngineStreamOutputBufferBinding(name, VertexBuffer, 0),
                };
            }
            else
            {
                VertexBuffer = graphics.CreateVertexBuffer(name, data, false);
            }

            VertexBufferBinding = new[]
            {
                new EngineVertexBufferBinding(VertexBuffer, default(T).GetStride(), 0),
            };
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="length">Data length</param>
        /// <param name="parameters">Vertex buffer parameters</param>
        public EngineVertexBuffer(Graphics graphics, string name, int length, VertexBufferParams parameters)
        {
            Graphics = graphics;
            Parameters = parameters;

            Name = name;

            int sizeInBytes = default(T).GetStride() * length;

            if (parameters == VertexBufferParams.Dynamic)
            {
                VertexBuffer = graphics.CreateVertexBuffer(name, sizeInBytes, true);
            }
            else if (parameters == VertexBufferParams.StreamOut)
            {
                VertexBuffer = graphics.CreateStreamOutBuffer(name, sizeInBytes);

                StreamOutBinding = new[]
                {
                    new EngineStreamOutputBufferBinding(name, VertexBuffer, 0),
                };
            }
            else
            {
                VertexBuffer = graphics.CreateVertexBuffer(name, sizeInBytes, false);
            }

            VertexBufferBinding = new[]
            {
                new EngineVertexBufferBinding (VertexBuffer, default(T).GetStride(), 0),
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
        /// <param name="dc">Device context</param>
        /// <param name="data">Data to write</param>
        public void Write(EngineDeviceContext dc, IEnumerable<T> data)
        {
            dc.WriteDiscardBuffer(VertexBuffer, data);
        }

        /// <inheritdoc/>
        public void CreateInputLayout(string name, byte[] signature, int bufferSlot)
        {
            var layout = Graphics.CreateInputLayout<T>(name, signature, bufferSlot);
            BufferSlot = bufferSlot;

            InputLayout?.Dispose();
            InputLayout = layout;
        }
        /// <inheritdoc/>
        public void SetVertexBuffers(EngineDeviceContext dc)
        {
            dc.IASetVertexBuffers(BufferSlot, VertexBufferBinding);
        }
        /// <inheritdoc/>
        public void SetInputLayout(EngineDeviceContext dc)
        {
            dc.IAInputLayout = InputLayout;
        }
        /// <inheritdoc/>
        public void SetStreamOutputTargets(EngineDeviceContext dc)
        {
            dc.SetGeometryShaderStreamOutputTargets(StreamOutBinding);
        }
        /// <inheritdoc/>
        public void Draw(EngineDeviceContext dc, int drawCount)
        {
            dc.Draw(drawCount, 0);
        }
        /// <inheritdoc/>
        public void DrawAuto(EngineDeviceContext dc)
        {
            dc.DrawAuto();
        }
    }

    /// <summary>
    /// Vertex buffer creation parameters
    /// </summary>
    public enum VertexBufferParams
    {
        /// <summary>
        /// Default
        /// </summary>
        Default,
        /// <summary>
        /// Dynamic
        /// </summary>
        Dynamic,
        /// <summary>
        /// Stream out buffer
        /// </summary>
        StreamOut,
    }
}
