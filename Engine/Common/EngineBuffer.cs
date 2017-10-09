using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    class EngineBuffer<T> : IDisposable where T : struct, IVertexData
    {
        /// <summary>
        /// Buffer
        /// </summary>
        internal Buffer VertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        internal VertexBufferBinding[] VertexBufferBinding;
        /// <summary>
        /// Input layouts
        /// </summary>
        internal List<InputLayout> InputLayouts = new List<InputLayout>();

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

        public void AddInputLayout(InputLayout input)
        {
            this.InputLayouts.Add(input);
        }

        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);

            Helper.Dispose(this.InputLayouts);
        }
    }
}
