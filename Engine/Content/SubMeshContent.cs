using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Sub mesh content
    /// </summary>
    public class SubMeshContent
    {
        /// <summary>
        /// Global id counter
        /// </summary>
        private static int ID = 0;
        /// <summary>
        /// Gets the next instance Id
        /// </summary>
        /// <returns>Returns the next instance Id</returns>
        private static int GetNextId()
        {
            return ++ID;
        }

        /// <summary>
        /// Vertices
        /// </summary>
        private List<VertexData> vertices = new List<VertexData>();
        /// <summary>
        /// Indices
        /// </summary>
        private List<uint> indices = new List<uint>();
        /// <summary>
        /// The submesh has attached a textured material
        /// </summary>
        /// <remarks>It's used for vertex type calculation</remarks>
        private bool textured = false;

        /// <summary>
        /// Submesh id
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Vertex Topology
        /// </summary>
        public Topology Topology { get; set; }
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType { get; private set; }
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexData[] Vertices
        {
            get
            {
                return this.vertices.ToArray();
            }
            set
            {
                this.VertexType = VertexTypes.Unknown;

                this.vertices.Clear();
                if (value != null && value.Length > 0)
                {
                    this.vertices.AddRange(value);
                    this.VertexType = VertexData.GetVertexType(this.vertices[0], this.textured);
                }
            }
        }
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices
        {
            get
            {
                return this.indices.ToArray();
            }
            set
            {
                this.indices.Clear();
                if (value != null && value.Length > 0)
                {
                    this.indices.AddRange(value);
                }
            }
        }
        /// <summary>
        /// Material
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// Gets or sets whether the current submesh content is a volume mesh
        /// </summary>
        public bool IsVolume { get; set; }
        /// <summary>
        /// Gets or sets wether the submesh has attached a textured material
        /// </summary>
        public bool Textured
        {
            get
            {
                return this.textured;
            }
            set
            {
                this.textured = value;

                if (this.vertices != null && this.vertices.Count > 0)
                {
                    this.VertexType = VertexData.GetVertexType(this.vertices[0], this.textured);
                }
            }
        }

        /// <summary>
        /// Submesh grouping optimization
        /// </summary>
        /// <param name="meshArray">Mesh array</param>
        /// <param name="optimizedMesh">Optimized mesh result</param>
        /// <returns>Returns true if the mesh array was optimized</returns>
        public static bool OptimizeMeshes(IEnumerable<SubMeshContent> meshArray, out SubMeshContent optimizedMesh)
        {
            optimizedMesh = null;

            int? count = meshArray?.Count();

            if (count == 1)
            {
                optimizedMesh = meshArray.First();
            }
            else if (count > 1)
            {
                var firstMesh = meshArray.First();

                string material = firstMesh.Material;
                Topology topology = firstMesh.Topology;
                VertexTypes vertexType = firstMesh.VertexType;
                bool isTextured = firstMesh.Textured;

                List<VertexData> verts = new List<VertexData>();
                List<uint> idx = new List<uint>();

                uint indexOffset = 0;

                foreach (var mesh in meshArray)
                {
                    if (mesh.VertexType != vertexType || mesh.Topology != topology)
                    {
                        optimizedMesh = null;

                        return false;
                    }

                    if (mesh.vertices.Count > 0)
                    {
                        verts.AddRange(mesh.vertices);
                    }

                    if (mesh.indices.Count > 0)
                    {
                        idx.AddRange(mesh.indices.Select(i => i + indexOffset));
                    }

                    indexOffset = (uint)verts.Count;
                }

                optimizedMesh = new SubMeshContent()
                {
                    Material = material,
                    Topology = topology,
                    VertexType = vertexType,

                    indices = idx,
                    vertices = verts,
                    textured = isTextured,
                };
            }

            return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SubMeshContent()
        {
            this.Id = GetNextId();
        }

        /// <summary>
        /// Compute UV tangen space
        /// </summary>
        public void ComputeTangents()
        {
            if (this.vertices.Count > 0)
            {
                if (this.indices.Count > 0)
                {
                    for (int i = 0; i < this.indices.Count; i += 3)
                    {
                        var v0 = this.vertices[(int)this.indices[i + 0]];
                        var v1 = this.vertices[(int)this.indices[i + 1]];
                        var v2 = this.vertices[(int)this.indices[i + 2]];

                        GeometryUtil.ComputeNormals(
                            v0.Position.Value, v1.Position.Value, v2.Position.Value,
                            v0.Texture.Value, v1.Texture.Value, v2.Texture.Value,
                            out Vector3 tangent, out Vector3 binormal, out Vector3 normal);

                        v0.Tangent = tangent;
                        v1.Tangent = tangent;
                        v2.Tangent = tangent;

                        v0.BiNormal = binormal;
                        v1.BiNormal = binormal;
                        v2.BiNormal = binormal;

                        this.vertices[(int)this.indices[i + 0]] = v0;
                        this.vertices[(int)this.indices[i + 1]] = v1;
                        this.vertices[(int)this.indices[i + 2]] = v2;
                    }
                }
                else
                {
                    for (int i = 0; i < this.vertices.Count; i += 3)
                    {
                        var v0 = this.vertices[i + 0];
                        var v1 = this.vertices[i + 1];
                        var v2 = this.vertices[i + 2];

                        GeometryUtil.ComputeNormals(
                            v0.Position.Value, v1.Position.Value, v2.Position.Value,
                            v0.Texture.Value, v1.Texture.Value, v2.Texture.Value,
                            out Vector3 tangent, out Vector3 binormal, out Vector3 normal);

                        v0.Tangent = tangent;
                        v1.Tangent = tangent;
                        v2.Tangent = tangent;

                        v0.BiNormal = binormal;
                        v1.BiNormal = binormal;
                        v2.BiNormal = binormal;

                        this.vertices[i + 0] = v0;
                        this.vertices[i + 1] = v1;
                        this.vertices[i + 2] = v2;
                    }
                }

                this.VertexType = VertexData.GetVertexType(this.vertices[0], this.textured);
            }
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns the triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (this.Topology == Topology.TriangleList)
            {
                List<Triangle> triangles = new List<Triangle>();

                if (this.indices.Count > 0)
                {
                    for (int i = 0; i < this.indices.Count; i += 3)
                    {
                        triangles.Add(new Triangle(
                            this.vertices[(int)this.indices[i + 0]].Position.Value,
                            this.vertices[(int)this.indices[i + 1]].Position.Value,
                            this.vertices[(int)this.indices[i + 2]].Position.Value));
                    }
                }
                else
                {
                    for (int i = 0; i < this.vertices.Count; i += 3)
                    {
                        triangles.Add(new Triangle(
                            this.vertices[i + 0].Position.Value,
                            this.vertices[i + 1].Position.Value,
                            this.vertices[i + 2].Position.Value));
                    }
                }

                return triangles.ToArray();
            }
            else
            {
                throw new InvalidOperationException(string.Format("Bad source topology for triangle list: {0}", this.Topology));
            }
        }
        /// <summary>
        /// Transforms the vertex data
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        public void Transform(Matrix transform)
        {
            for (int i = 0; i < this.vertices.Count; i++)
            {
                this.vertices[i] = this.vertices[i].Transform(transform);
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
