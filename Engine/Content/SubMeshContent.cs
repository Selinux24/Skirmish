using SharpDX.Direct3D;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Sub mesh content
    /// </summary>
    public class SubMeshContent
    {
        /// <summary>
        /// Vertices
        /// </summary>
        private VertexData[] vertices;
        /// <summary>
        /// Indices
        /// </summary>
        private uint[] indices;

        /// <summary>
        /// Vertex Topology
        /// </summary>
        public PrimitiveTopology Topology { get; set; }
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType { get; set; }
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexData[] Vertices
        {
            get
            {
                return this.vertices;
            }
            set
            {
                this.vertices = value;
            }
        }
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices
        {
            get
            {
                return this.indices;
            }
            set
            {
                this.indices = value;
            }
        }
        /// <summary>
        /// Material
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            string text = null;

            text += string.Format("VertexType: {0}; ", this.VertexType);
            if (this.Vertices != null) text += string.Format("Vertices: {0}; ", this.Vertices.Length);
            if (this.Indices != null) text += string.Format("Indices: {0}; ", this.Indices.Length);
            if (this.Material != null) text += string.Format("Material: {0}; ", this.Material);

            return text;
        }
    }
}
