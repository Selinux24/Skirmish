
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Properties defining the way a buffer (containing vertex data) is bound to the pipeline for rendering.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the SharpDX.Direct3D11.VertexBufferBinding struct.
    /// </remarks>
    /// <param name="buffer">The buffer being bound.</param>
    /// <param name="stride">The stride between vertex element (in bytes).</param>
    /// <param name="offset">The offset to the first vertex (in bytes).</param>
    public struct EngineVertexBufferBinding(EngineBuffer buffer, int stride, int offset)
    {
        /// <summary>
        /// Gets or sets the buffer being bound.
        /// </summary>
        public EngineBuffer Buffer { get; set; } = buffer;
        /// <summary>
        /// Gets or sets the stride between vertex elements in the buffer (in bytes).
        /// </summary>
        public int Stride { get; set; } = stride;
        /// <summary>
        /// Gets or sets the offset from the start of the buffer of the first vertex to use (in bytes).
        /// </summary>
        public int Offset { get; set; } = offset;

        /// <summary>
        /// Gets the vertext buffer binding
        /// </summary>
        internal readonly VertexBufferBinding GetVertexBufferBinding()
        {
            return new(Buffer?.GetBuffer(), Stride, Offset);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Offset: {Offset}; Stride: {Stride}; Buffer: {Buffer}";
        }
    }
}
