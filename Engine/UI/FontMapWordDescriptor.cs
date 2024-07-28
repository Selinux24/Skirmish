using Engine.BuiltIn.Primitives;

namespace Engine.UI
{
    /// <summary>
    /// Word descriptor
    /// </summary>
    struct FontMapWordDescriptor
    {
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexFont[] Vertices { get; set; }
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices { get; set; }
        /// <summary>
        /// Word height
        /// </summary>
        public float Height { get; set; }
    }
}
