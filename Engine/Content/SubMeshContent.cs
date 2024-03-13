using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Sub mesh content
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="topology">Submesh topology</param>
    /// <param name="material">Material name</param>
    /// <param name="isTextured">Is textured mesh</param>
    /// <param name="isHull">Is hull mesh</param>
    /// <param name="transform">Transform</param>
    public class SubMeshContent(Topology topology, string material, bool isTextured, bool isHull, Matrix transform)
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
        /// Submesh id
        /// </summary>
        public int Id { get; private set; } = GetNextId();
        /// <summary>
        /// Vertex Topology
        /// </summary>
        public Topology Topology { get; set; } = topology;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType { get; private set; } = VertexTypes.Unknown;
        /// <summary>
        /// Gets or sets whether the submesh has attached a textured material
        /// </summary>
        public bool Textured { get; private set; } = isTextured;
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexData[] Vertices { get; private set; } = [];
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices { get; private set; } = [];
        /// <summary>
        /// Material
        /// </summary>
        public string Material { get; set; } = material;
        /// <summary>
        /// Gets or sets whether the current submesh content is a hull mesh
        /// </summary>
        public bool IsHull { get; set; } = isHull;
        /// <summary>
        /// Transform
        /// </summary>
        public Matrix Transform { get; private set; } = transform;

        /// <summary>
        /// Submesh grouping optimization
        /// </summary>
        /// <param name="meshArray">Mesh array</param>
        /// <param name="optimizedMesh">Optimized mesh result</param>
        /// <returns>Returns true if the mesh array was optimized</returns>
        public static bool OptimizeMeshes(IEnumerable<SubMeshContent> meshArray, out SubMeshContent optimizedMesh)
        {
            if (meshArray?.Any() != true)
            {
                optimizedMesh = null;

                return false;
            }

            if (meshArray.Count() == 1)
            {
                optimizedMesh = meshArray.First();

                return true;
            }

            if (meshArray.Select(m => (m.VertexType, m.Topology)).Distinct().Count() > 1)
            {
                optimizedMesh = null;

                return false;
            }

            var firstMesh = meshArray.First();

            var meshTopology = firstMesh.Topology;
            string meshMaterial = firstMesh.Material;
            bool meshTextured = firstMesh.Textured;

            var verts = new List<VertexData>();
            var idx = new List<uint>();

            uint indexOffset = 0;

            foreach (var mesh in meshArray)
            {
                if (!mesh.Transform.IsIdentity)
                {
                    mesh.BakeTransform(mesh.Transform);
                }

                if (mesh.Vertices.Length > 0)
                {
                    verts.AddRange(mesh.Vertices);
                }

                if (mesh.Indices.Length > 0)
                {
                    idx.AddRange(mesh.Indices.Select(i => i + indexOffset));
                }

                indexOffset = (uint)verts.Count;
            }

            optimizedMesh = new(meshTopology, meshMaterial, meshTextured, false, Matrix.Identity);

            optimizedMesh.SetVertices(verts);
            optimizedMesh.SetIndices(idx);

            return true;
        }

        /// <summary>
        /// Sets the submesh vertex list
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        public void SetVertices(IEnumerable<VertexData> vertices)
        {
            Vertices = vertices?.ToArray() ?? [];
            VertexType = vertices?.Any() != true ? VertexTypes.Unknown : VertexData.GetVertexType(Vertices[0], Textured);
        }
        /// <summary>
        /// Sets the submesh index list
        /// </summary>
        /// <param name="indices">Index list</param>
        public void SetIndices(IEnumerable<uint> indices)
        {
            Indices = indices?.ToArray() ?? [];
        }
        /// <summary>
        /// Sets whether the submesh is textured or not
        /// </summary>
        /// <param name="isTextured"></param>
        public void SetTextured(bool isTextured)
        {
            Textured = isTextured;

            if (Vertices.Length > 0)
            {
                VertexType = VertexData.GetVertexType(Vertices[0], Textured);
            }
        }

        /// <summary>
        /// Compute UV tangen space
        /// </summary>
        public void ComputeTangents()
        {
            if (Vertices.Length == 0)
            {
                return;
            }

            if (Indices.Length > 0)
            {
                for (int i = 0; i < Indices.Length; i += 3)
                {
                    var v0 = Vertices[(int)Indices[i + 0]];
                    var v1 = Vertices[(int)Indices[i + 1]];
                    var v2 = Vertices[(int)Indices[i + 2]];

                    var n = GeometryUtil.ComputeNormals(
                        v0.Position.Value, v1.Position.Value, v2.Position.Value,
                        v0.Texture.Value, v1.Texture.Value, v2.Texture.Value);

                    v0.Tangent = n.Tangent;
                    v1.Tangent = n.Tangent;
                    v2.Tangent = n.Tangent;

                    v0.BiNormal = n.Binormal;
                    v1.BiNormal = n.Binormal;
                    v2.BiNormal = n.Binormal;

                    Vertices[(int)Indices[i + 0]] = v0;
                    Vertices[(int)Indices[i + 1]] = v1;
                    Vertices[(int)Indices[i + 2]] = v2;
                }
            }
            else
            {
                for (int i = 0; i < Vertices.Length; i += 3)
                {
                    var v0 = Vertices[i + 0];
                    var v1 = Vertices[i + 1];
                    var v2 = Vertices[i + 2];

                    var n = GeometryUtil.ComputeNormals(
                        v0.Position.Value, v1.Position.Value, v2.Position.Value,
                        v0.Texture.Value, v1.Texture.Value, v2.Texture.Value);

                    v0.Tangent = n.Tangent;
                    v1.Tangent = n.Tangent;
                    v2.Tangent = n.Tangent;

                    v0.BiNormal = n.Binormal;
                    v1.BiNormal = n.Binormal;
                    v2.BiNormal = n.Binormal;

                    Vertices[i + 0] = v0;
                    Vertices[i + 1] = v1;
                    Vertices[i + 2] = v2;
                }
            }

            VertexType = VertexData.GetVertexType(Vertices[0], Textured);
        }

        /// <summary>
        /// Gets vertex data list
        /// </summary>
        /// <returns>Returns the vertex data list</returns>
        public IEnumerable<VertexData> GetVertices()
        {
            if (Transform.IsIdentity)
            {
                return Vertices.AsReadOnly();
            }

            return VertexData.Transform(Vertices, Transform);
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns the triangle list</returns>
        public IEnumerable<Triangle> GetTriangles()
        {
            if (Topology != Topology.TriangleList)
            {
                throw new InvalidOperationException($"Bad source topology for triangle list: {Topology}");
            }

            var vertices = GetVertices().ToArray();

            List<Triangle> triangles = [];

            if (Indices.Length > 0)
            {
                for (int i = 0; i < Indices.Length; i += 3)
                {
                    triangles.Add(new(
                        vertices[(int)Indices[i + 0]].Position.Value,
                        vertices[(int)Indices[i + 1]].Position.Value,
                        vertices[(int)Indices[i + 2]].Position.Value));
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    triangles.Add(new(
                        vertices[i + 0].Position.Value,
                        vertices[i + 1].Position.Value,
                        vertices[i + 2].Position.Value));
                }
            }

            return triangles;
        }
        /// <summary>
        /// Transforms the vertex data and resets de internal transform to identity
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        public void BakeTransform(Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return;
            }

            Transform = Matrix.Identity;
            Vertices = Vertices.Select(v => v.Transform(transform)).ToArray();
        }
        /// <summary>
        /// Sets the specified transform matrix
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        public void SetTransform(Matrix transform)
        {
            Transform = transform;
        }

        /// <summary>
        /// Process the vertex data
        /// </summary>
        /// <param name="vertexType">Vertext type</param>
        /// <param name="constraint">Constraint</param>
        public async Task<(IEnumerable<VertexData> vertices, IEnumerable<uint> indices)> ProcessVertexData(VertexTypes vertexType, BoundingBox? constraint)
        {
            if (VertexData.IsTangent(vertexType))
            {
                ComputeTangents();
            }

            if (!constraint.HasValue)
            {
                return (Vertices, Indices);
            }

            if (Indices?.Length > 0)
            {
                return await GeometryUtil.ConstraintIndicesAsync(constraint.Value, Vertices, Indices);
            }
            else
            {
                var vertices = await GeometryUtil.ConstraintVerticesAsync(constraint.Value, Vertices);
                var indices = Array.Empty<uint>();

                return (vertices, indices);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"VertexType: {VertexType}; Vertices: {Vertices?.Length ?? 0}; Indices: {Indices?.Length ?? 0}; Material: {Material}";
        }
    }
}
