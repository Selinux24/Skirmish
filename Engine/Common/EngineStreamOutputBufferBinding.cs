
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Properties defining the way a buffer is bound to the pipeline as a target for stream output operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the SharpDX.Direct3D11.StreamOutputBufferBinding struct.
    /// </remarks>
    /// <param name="name">Name</param>
    /// <param name="buffer">The buffer being bound.</param>
    /// <param name="offset">The offset to the first vertex (in bytes).</param>
    public struct EngineStreamOutputBufferBinding(string name, EngineBuffer buffer, uint offset)
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// Gets or sets the buffer being bound.
        /// </summary>
        public EngineBuffer Buffer { get; set; } = buffer;
        /// <summary>
        /// Gets or sets the offset from the start of the buffer of the first vertex to use (in bytes).
        /// </summary>
        public uint Offset { get; set; } = offset;

        /// <summary>
        /// Gets the stream output buffer binding
        /// </summary>
        internal readonly (ID3D11Buffer Buffer, uint Offset) GetStreamOutputBufferBinding()
        {
            return new (Buffer?.GetBuffer(), Offset);
        }
    }
}
