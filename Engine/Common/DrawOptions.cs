
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
        /// Topology
        /// </summary>
        public Topology Topology { get; set; }
        /// <summary>
        /// Primitive draw count
        /// </summary>
        public int DrawCount { get; set; }

        /// <summary>
        /// Use indices
        /// </summary>
        public bool Indexed { get; set; }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        public BufferDescriptor IndexBuffer { get; set; }

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
        /// <param name="graphics">Graphics</param>
        public void Draw(Graphics graphics)
        {
            if (Instanced)
            {
                DrawInstanced(graphics);
            }
            else
            {
                DrawSingle(graphics);
            }
        }
        /// <summary>
        /// Draw single
        /// </summary>
        /// <param name="graphics">Graphics</param>
        private void DrawSingle(Graphics graphics)
        {
            if (Indexed)
            {
                graphics.DrawIndexed(
                    IndexBuffer.Count,
                    IndexBuffer.BufferOffset,
                    VertexBuffer.BufferOffset);
            }
            else
            {
                int drawCount = DrawCount > 0 ? DrawCount : VertexBuffer.Count;

                graphics.Draw(
                    drawCount,
                    VertexBuffer.BufferOffset);
            }
        }
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="graphics">Graphics</param>
        private void DrawInstanced(Graphics graphics)
        {
            if (Indexed)
            {
                graphics.DrawIndexedInstanced(
                    IndexBuffer.Count,
                    InstanceCount,
                    IndexBuffer.BufferOffset,
                    VertexBuffer.BufferOffset, StartInstanceLocation);
            }
            else
            {
                int drawCount = DrawCount > 0 ? DrawCount : VertexBuffer.Count;

                graphics.DrawInstanced(
                    drawCount,
                    InstanceCount,
                    VertexBuffer.BufferOffset, StartInstanceLocation);
            }
        }
    }
}
