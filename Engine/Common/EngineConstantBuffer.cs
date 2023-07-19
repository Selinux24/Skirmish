using System;

namespace Engine.Common
{
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
        EngineBuffer Buffer { get; }
        /// <summary>
        /// Gets the internal data stream
        /// </summary>
        EngineDataStream DataStream { get; }

        /// <summary>
        /// Gets the current constant data
        /// </summary>
        IBufferData GetData();
    }

    /// <summary>
    /// Engine constant buffer
    /// </summary>
    /// <typeparam name="T">Type of fuffer</typeparam>
    public class EngineConstantBuffer<T> : IEngineConstantBuffer where T : struct, IBufferData
    {
        /// <summary>
        /// Current data value
        /// </summary>
        private T currentData;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        public EngineBuffer Buffer { get; private set; }
        /// <summary>
        /// Gets the internal data stream
        /// </summary>
        public EngineDataStream DataStream { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        public EngineConstantBuffer(Graphics graphics, string name)
        {
            Name = name;
            DataStream = new EngineDataStream(Marshal.SizeOf(typeof(T)), true, true);
            Buffer = new EngineBuffer(graphics.CreateConstantBuffer<T>(name));
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
                DataStream?.Dispose();
                Buffer?.Dispose();
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

            currentData = data;
        }

        /// <summary>
        /// Gets the current constant data
        /// </summary>
        public IBufferData GetData()
        {
            return currentData;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(EngineConstantBuffer<T>)} {Name}";
        }
    }
}
