using System;

namespace Engine.Common
{
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
    }
}
