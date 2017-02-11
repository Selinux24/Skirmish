using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Batcher
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    class Batcher<T> where T : struct
    {
        const int Stack = 1024 * 8;

        private Game game = null;

        private int bindingOffset = 0;
        private int offset = 0;
        private int bufferAssigned = 0;
        private int bufferSize = 0;

        /// <summary>
        /// Buffer
        /// </summary>
        public Buffer Buffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        public VertexBufferBinding[] BufferBinding;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bindingOffset">Binding offset</param>
        public Batcher(Game game, int bindingOffset)
        {
            this.game = game;

            this.bindingOffset = bindingOffset;
        }

        /// <summary>
        /// Add data to buffer
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Returns data offset in buffer</returns>
        public int Add(IEnumerable<T> data)
        {
            if (this.Buffer == null)
            {
                int bufferLength = data.Count() + Stack;
                bufferLength = (bufferLength = +Stack) & (~Stack);

                this.Buffer = this.game.Graphics.Device.CreateBuffer<T>(
                    null,
                    bufferLength,
                    SharpDX.Direct3D11.ResourceUsage.Dynamic,
                    SharpDX.Direct3D11.BindFlags.VertexBuffer,
                    SharpDX.Direct3D11.CpuAccessFlags.Write);

                this.game.Graphics.DeviceContext.WriteNoOverwriteBuffer<T>(this.Buffer, this.offset, data.ToArray());

                this.offset += data.Count();
                this.bufferAssigned += data.Count();
                this.bufferSize += bufferLength;

                this.AddVertexBufferBinding(new VertexBufferBinding(this.Buffer, Marshal.SizeOf(typeof(T)), this.bindingOffset));
            }
            else
            {
                if (bufferSize - bufferAssigned > data.Count())
                {
                    return -1;
                }
                else
                {
                    this.game.Graphics.DeviceContext.WriteNoOverwriteBuffer<T>(this.Buffer, this.offset, data.ToArray());

                    this.offset += data.Count();
                    this.bufferAssigned += data.Count();
                }
            }

            return this.offset;
        }

        /// <summary>
        /// Adds binding to precached buffer bindings for input assembler
        /// </summary>
        /// <param name="binding">Binding</param>
        public virtual void AddVertexBufferBinding(VertexBufferBinding binding)
        {
            Array.Resize(ref this.BufferBinding, this.BufferBinding.Length + 1);

            this.BufferBinding[this.BufferBinding.Length - 1] = binding;
        }

        public void AddComponents(ICollection<Drawable> components)
        {

        }
    }
}
