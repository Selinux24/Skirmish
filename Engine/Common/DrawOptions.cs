
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
    }
}
