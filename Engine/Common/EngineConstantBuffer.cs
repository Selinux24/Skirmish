using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine constant buffer
    /// </summary>
    /// <typeparam name="T">Type of fuffer</typeparam>
    public class EngineConstantBuffer<T> : IEngineConstantBuffer where T : struct, IBufferData
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        public EngineBuffer Buffer { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        public EngineConstantBuffer(Graphics graphics, string name)
        {
            if (graphics == null) throw new ArgumentNullException(nameof(graphics), "Must specify the graphics instance to create the constant buffer in the graphics device.");

            Name = name;
            Buffer = graphics.CreateConstantBuffer<T>(name);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineConstantBuffer()
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
                Buffer?.Dispose();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(EngineConstantBuffer<>)} {Name}";
        }
    }
}
