using SharpDX;
using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Engine constant buffer interface
    /// </summary>
    public interface IEngineConstantBuffer : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        Buffer GetBuffer();
    }

    /// <summary>
    /// Engine constant buffer
    /// </summary>
    /// <typeparam name="T">Type of fuffer</typeparam>
    public class EngineConstantBuffer<T> : IEngineConstantBuffer where T : struct, IBufferData
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private readonly Graphics graphics;
        /// <summary>
        /// Buffer
        /// </summary>
        private readonly Buffer buffer;
        /// <summary>
        /// Data stream to update the buffer in memory
        /// </summary>
        private readonly DataStream dataStream;
        /// <summary>
        /// Current data value
        /// </summary>
        private T currentData;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        public EngineConstantBuffer(Graphics graphics, string name)
        {
            this.graphics = graphics;

            Name = name;
            dataStream = new DataStream(Marshal.SizeOf(typeof(T)), true, true);
            buffer = graphics.CreateConstantBuffer<T>(name);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineConstantBuffer()
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
                dataStream?.Dispose();
                buffer?.Dispose();
            }
        }

        /// <summary>
        /// Writes data to de constant buffer
        /// </summary>
        /// <param name="data">Data</param>
        public void WriteData(T data)
        {
            if (data.Equals(currentData))
            {
                return;
            }

            graphics.UpdateConstantBuffer(dataStream, buffer, data);

            currentData = data;
        }

        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        public Buffer GetBuffer()
        {
            return buffer;
        }
    }
}
