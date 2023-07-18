
namespace Engine.Common
{
    /// <summary>
    /// Draw options
    /// </summary>
    public struct DrawOptions
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexBuffer { get; set; }
        /// <summary>
        /// Vertex draw count
        /// </summary>
        public int VertexDrawCount { get; set; }
        /// <summary>
        /// Vertex buffer additional offset
        /// </summary>
        public int VertexBufferOffset { get; set; }
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; set; }

        /// <summary>
        /// Use indices
        /// </summary>
        public readonly bool Indexed
        {
            get
            {
                return IndexBuffer != null;
            }
        }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        public BufferDescriptor IndexBuffer { get; set; }
        /// <summary>
        /// Index draw count
        /// </summary>
        public int IndexDrawCount { get; set; }
        /// <summary>
        /// Index buffer additional offset
        /// </summary>
        public int IndexBufferOffset { get; set; }

        /// <summary>
        /// Use instanced drawing
        /// </summary>
        public bool Instanced { get; set; }
        /// <summary>
        /// Instance count
        /// </summary>
        public int InstanceCount { get; set; }
        /// <summary>
        /// Start instance location in instanced buffer
        /// </summary>
        public int StartInstanceLocation { get; set; }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Device context</param>
        public readonly void Draw(EngineDeviceContext context)
        {
            if (Instanced)
            {
                DrawInstanced(context);
            }
            else
            {
                DrawSingle(context);
            }
        }
        /// <summary>
        /// Draw single
        /// </summary>
        /// <param name="context">Device context</param>
        private readonly void DrawSingle(EngineDeviceContext context)
        {
            if (Indexed)
            {
                int drawCount = IndexDrawCount > 0 ? IndexDrawCount : IndexBuffer.Count;

                context.DrawIndexed(
                    drawCount,
                    IndexBuffer.BufferOffset + IndexBufferOffset,
                    VertexBuffer.BufferOffset + VertexBufferOffset);
            }
            else
            {
                int drawCount = VertexDrawCount > 0 ? VertexDrawCount : VertexBuffer.Count;

                context.Draw(
                    drawCount,
                    VertexBuffer.BufferOffset + VertexBufferOffset);
            }
        }
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="context">Device context</param>
        private readonly void DrawInstanced(EngineDeviceContext context)
        {
            if (Indexed)
            {
                int drawCount = IndexDrawCount > 0 ? IndexDrawCount : IndexBuffer.Count;

                context.DrawIndexedInstanced(
                    drawCount,
                    InstanceCount,
                    IndexBuffer.BufferOffset + IndexBufferOffset,
                    VertexBuffer.BufferOffset + VertexBufferOffset, StartInstanceLocation);
            }
            else
            {
                int drawCount = VertexDrawCount > 0 ? VertexDrawCount : VertexBuffer.Count;

                context.DrawInstanced(
                    drawCount,
                    InstanceCount,
                    VertexBuffer.BufferOffset + VertexBufferOffset, StartInstanceLocation);
            }
        }
    }
}
