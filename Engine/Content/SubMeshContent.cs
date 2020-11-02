﻿using SharpDX;
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
        public VertexTypes VertexType { get; private set; } = VertexTypes.Unknown;
        /// <summary>
        /// Gets or sets wether the submesh has attached a textured material
        /// </summary>
        public bool Textured { get; private set; } = false;
        /// <summary>
        /// Vertices
        /// </summary>
        public VertexData[] Vertices { get; private set; } = new VertexData[] { };
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices { get; private set; } = new uint[] { };
        /// <summary>
        /// Material
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// Gets or sets whether the current submesh content is a volume mesh
        /// </summary>
        public bool IsVolume { get; set; }
        /// <summary>
        /// Transform
        /// </summary>
        public Matrix Transform { get; set; } = Matrix.Identity;

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

                optimizedMesh = new SubMeshContent(topology, material, isTextured, false);

                optimizedMesh.SetVertices(verts);
                optimizedMesh.SetIndices(idx);
            }

            return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topology">Submesh topology</param>
        /// <param name="material">Material name</param>
        /// <param name="isTextured">Textured mesh</param>
        /// <param name="isVolume">Volume mesh</param>
        public SubMeshContent(Topology topology, string material, bool isTextured, bool isVolume)
        {
            Id = GetNextId();

            Topology = topology;
            Material = material;
            Textured = isTextured;
            IsVolume = isVolume;
        }

        /// <summary>
        /// Sets the submesh vertex list
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        public void SetVertices(IEnumerable<VertexData> vertices)
        {
            Vertices = new VertexData[] { };
            VertexType = VertexTypes.Unknown;

            if (vertices?.Any() == true)
            {
                Vertices = new List<VertexData>(vertices).ToArray();
                VertexType = VertexData.GetVertexType(Vertices[0], Textured);
            }
        }
        /// <summary>
        /// Sets the submesh index list
        /// </summary>
        /// <param name="indices">Index list</param>
        public void SetIndices(IEnumerable<uint> indices)
        {
            Indices = new uint[] { };

            if (indices?.Any() == true)
            {
                Indices = new List<uint>(indices).ToArray();
            }
        }
        /// <summary>
        /// Sets whether the submesh is textured or not
        /// </summary>
        /// <param name="isTextured"></param>
        public void SetTextured(bool isTextured)
        {
            Textured = isTextured;

            if (Vertices?.Length > 0)
            {
                VertexType = VertexData.GetVertexType(Vertices[0], Textured);
            }
        }

        /// <summary>
        /// Compute UV tangen space
        /// </summary>
        public void ComputeTangents()
        {
            if (Vertices.Length > 0)
            {
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
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns the triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (Topology == Topology.TriangleList)
            {
                List<Triangle> triangles = new List<Triangle>();

                if (Indices.Length > 0)
                {
                    for (int i = 0; i < Indices.Length; i += 3)
                    {
                        triangles.Add(new Triangle(
                            Vertices[(int)Indices[i + 0]].Position.Value,
                            Vertices[(int)Indices[i + 1]].Position.Value,
                            Vertices[(int)Indices[i + 2]].Position.Value));
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Length; i += 3)
                    {
                        triangles.Add(new Triangle(
                            Vertices[i + 0].Position.Value,
                            Vertices[i + 1].Position.Value,
                            Vertices[i + 2].Position.Value));
                    }
                }

                return triangles.ToArray();
            }
            else
            {
                throw new InvalidOperationException($"Bad source topology for triangle list: {Topology}");
            }
        }
        /// <summary>
        /// Transforms the vertex data
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        public void ApplyTransform(Matrix transform)
        {
            Transform = Matrix.Identity;

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vertices[i].Transform(transform);
            }
        }

        /// <summary>
        /// Process the vertex data
        /// </summary>
        /// <param name="description">Decription</param>
        /// <param name="vertexType">Vertext type</param>
        /// <param name="vertices">Resulting vertices</param>
        /// <param name="indices">Resulting indices</param>
        public void ProcessVertexData(VertexTypes vertexType, BoundingBox? constraint, out IEnumerable<VertexData> vertices, out IEnumerable<uint> indices)
        {
            if (VertexData.IsTangent(vertexType))
            {
                ComputeTangents();
            }

            if (!constraint.HasValue)
            {
                vertices = Vertices;
                indices = Indices;

                return;
            }

            if (Indices?.Length > 0)
            {
                GeometryUtil.ConstraintIndices(
                    constraint.Value,
                    Vertices, Indices,
                    out vertices, out indices);
            }
            else
            {
                GeometryUtil.ConstraintVertices(
                    constraint.Value,
                    Vertices,
                    out vertices);

                indices = new uint[] { };
            }
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            return $"VertexType: {VertexType}; Vertices: {Vertices?.Length ?? 0}; Indices: {Indices?.Length ?? 0}; Material: {Material}";
        }
    }
}
