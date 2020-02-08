using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer descriptor request
    /// </summary>
    class BufferDescriptorRequestVertices : IBufferDescriptorRequest
    {
        /// <summary>
        /// Requester Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Data to assign
        /// </summary>
        public IEnumerable<IVertexData> Data { get; set; }
        /// <summary>
        /// Gets or sets wheter the destination buffer must be dynamic or not
        /// </summary>
        public bool Dynamic { get; set; }
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexDescriptor { get; set; } = new BufferDescriptor();
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;
        /// <summary>
        /// Request action
        /// </summary>
        public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
        /// <summary>
        /// Gets wheter the descriptor is processed into the buffer manager or not
        /// </summary>
        public bool Processed { get; set; } = false;
    }
}
