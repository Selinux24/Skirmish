using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine constant buffer
    /// </summary>
    /// <typeparam name="T">Type of fuffer</typeparam>
    public class EngineConstantBuffer<T> : IDisposable where T : struct
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
                buffer?.Dispose();
            }
        }

        /// <summary>
        /// Writes data to de constant buffer
        /// </summary>
        /// <param name="data">Data</param>
        public void WriteData(T data)
        {
            graphics.WriteDiscardBuffer(buffer, data);
        }
    }
}
