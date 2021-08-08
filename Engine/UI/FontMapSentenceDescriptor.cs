using SharpDX;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sentence descriptor
    /// </summary>
    struct FontMapSentenceDescriptor
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
        /// Sentence size
        /// </summary>
        public Vector2 Size { get; set; }
    }
}
