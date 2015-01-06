using System.Collections.Generic;
using SharpDX;
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

                this.BoundingBox = this.ComputeBoundingBox();
                this.BoundingSphere = this.ComputeBoundingSphere();
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
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }
        /// <summary>
        /// Bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }

        /// <summary>
        /// Compute triangle list
        /// </summary>
        /// <returns>Returns computed triangle list</returns>
        public Triangle[] ComputeTriangleList()
        {
            if (this.indices != null && this.indices.Length > 0)
            {
                return Triangle.ComputeTriangleList(this.Topology, this.vertices, this.indices);
            }
            else
            {
                return Triangle.ComputeTriangleList(this.Topology, this.vertices);
            }
        }
        /// <summary>
        /// Compute bounding sphere
        /// </summary>
        /// <returns>Returns computed bounding sphere</returns>
        private BoundingSphere ComputeBoundingSphere()
        {
            List<Vector3> list = new List<Vector3>();

            foreach (VertexData v in this.Vertices)
            {
                if (v.Position.HasValue)
                {
                    list.Add(v.Position.Value);
                }
            }

            return BoundingSphere.FromPoints(list.ToArray());
        }
        /// <summary>
        /// Compute bounding box
        /// </summary>
        /// <returns>Returns computed bounding box</returns>
        private BoundingBox ComputeBoundingBox()
        {
            List<Vector3> list = new List<Vector3>();

            foreach (VertexData v in this.Vertices)
            {
                if (v.Position.HasValue)
                {
                    list.Add(v.Position.Value);
                }
            }

            return BoundingBox.FromPoints(list.ToArray());
        }

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
