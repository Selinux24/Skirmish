using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine.Content
{
    using Engine.Common;

    public class SubMeshContent
    {
        private Vertex[] vertices;
        private uint[] indices;

        public PrimitiveTopology Topology { get; set; }
        public VertexTypes VertexType { get; set; }
        public Vertex[] Vertices
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
        public string Material { get; set; }
        public BoundingBox BoundingBox { get; private set; }
        public BoundingSphere BoundingSphere { get; private set; }

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
        private BoundingSphere ComputeBoundingSphere()
        {
            List<Vector3> list = new List<Vector3>();

            foreach (Vertex v in this.Vertices)
            {
                if (v.Position.HasValue)
                {
                    list.Add(v.Position.Value);
                }
            }

            return BoundingSphere.FromPoints(list.ToArray());
        }
        private BoundingBox ComputeBoundingBox()
        {
            List<Vector3> list = new List<Vector3>();

            foreach (Vertex v in this.Vertices)
            {
                if (v.Position.HasValue)
                {
                    list.Add(v.Position.Value);
                }
            }

            return BoundingBox.FromPoints(list.ToArray());
        }

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
