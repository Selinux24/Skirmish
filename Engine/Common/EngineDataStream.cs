using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine data stream
    /// </summary>
    public class EngineDataStream : IDisposable
    {
        /// <summary>
        /// Data stream
        /// </summary>
        private readonly DataStream dataStream;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataStream">Data stream</param>
        internal EngineDataStream(DataStream dataStream)
        {
            this.dataStream = dataStream ?? throw new ArgumentNullException(nameof(dataStream));
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sizeInBytes">Size in bytes</param>
        /// <param name="canRead">Can read</param>
        /// <param name="canWrite">Can write</param>
        public EngineDataStream(int sizeInBytes, bool canRead, bool canWrite)
        {
            dataStream = new DataStream(sizeInBytes, canRead, canWrite);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDataStream()
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
            }
        }

        /// <summary>
        /// Gets the internal data stream
        /// </summary>
        internal DataStream GetDataStream()
        {
            return dataStream;
        }
    }
}
