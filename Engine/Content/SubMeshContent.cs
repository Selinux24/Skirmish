using System.Collections.Generic;
using SharpDX.Direct3D;

namespace Engine.Content
{
    using Engine.Common;
    using SharpDX;

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
        /// Submesh grouping optimization
        /// </summary>
        /// <param name="meshArray">Mesh array</param>
        /// <param name="optimizedMesh">Optimized mesh result</param>
        /// <returns>Returns true if the mesh array was optimized</returns>
        public static bool OptimizeMeshes(SubMeshContent[] meshArray, out SubMeshContent optimizedMesh)
        {
            if (meshArray == null || meshArray.Length == 0)
            {
                optimizedMesh = null;

                return true;
            }
            else if (meshArray.Length == 1)
            {
                optimizedMesh = meshArray[0];

                return true;
            }
            else
            {
                string material = meshArray[0].Material;
                PrimitiveTopology topology = meshArray[0].Topology;
                VertexTypes vertexType = meshArray[0].VertexType;

                List<VertexData> vertices = new List<VertexData>();
                List<uint> indices = new List<uint>();

                uint indexOffset = 0;

                foreach (SubMeshContent mesh in meshArray)
                {
                    if (mesh.VertexType != vertexType || mesh.Topology != topology)
                    {
                        optimizedMesh = null;

                        return false;
                    }

                    if (mesh.Vertices != null && mesh.Vertices.Length > 0)
                    {
                        foreach (VertexData v in mesh.Vertices)
                        {
                            vertices.Add(v);
                        }
                    }

                    if (mesh.Indices != null && mesh.Indices.Length > 0)
                    {
                        foreach (uint i in mesh.Indices)
                        {
                            indices.Add(indexOffset + i);
                        }
                    }

                    indexOffset = (uint)vertices.Count;
                }

                optimizedMesh = new SubMeshContent()
                {
                    Material = material,
                    Topology = topology,
                    VertexType = vertexType,
                    Indices = indices.ToArray(),
                    Vertices = vertices.ToArray(),
                };

                return true;
            }
        }
        /// <summary>
        /// Compute UV tangen space
        /// </summary>
        public void ComputeTangents()
        {
            if (this.vertices != null && this.vertices.Length > 0)
            {
                if (this.indices != null && this.indices.Length > 0)
                {
                    for (int i = 0; i < this.indices.Length; i += 3)
                    {
                        Vector3 tangent;
                        Vector3 binormal;
                        Vector3 normal;
                        VertexData.ComputeNormals(
                            this.vertices[this.indices[i + 0]],
                            this.vertices[this.indices[i + 1]],
                            this.vertices[this.indices[i + 2]], 
                            out tangent, out binormal, out normal);

                        this.vertices[this.indices[i + 0]].Tangent = tangent;
                        this.vertices[this.indices[i + 1]].Tangent = tangent;
                        this.vertices[this.indices[i + 2]].Tangent = tangent;
                    }
                }
                else
                {
                    for (int i = 0; i < this.vertices.Length; i += 3)
                    {
                        Vector3 tangent;
                        Vector3 binormal;
                        Vector3 normal;
                        VertexData.ComputeNormals(
                            this.vertices[i + 0],
                            this.vertices[i + 1],
                            this.vertices[i + 2], 
                            out tangent, out binormal, out normal);

                        this.vertices[i + 0].Tangent = tangent;
                        this.vertices[i + 1].Tangent = tangent;
                        this.vertices[i + 2].Tangent = tangent;
                    }
                }
            }
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
