using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine buffer
    /// </summary>
    public class EngineBuffer : IDisposable
    {
        /// <summary>
        /// Buffer
        /// </summary>
        private readonly Buffer buffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer</param>
        internal EngineBuffer(Buffer buffer)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
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
                buffer?.Dispose();
            }
        }

        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        internal Buffer GetBuffer()
        {
            return buffer;
        }
    }
}
