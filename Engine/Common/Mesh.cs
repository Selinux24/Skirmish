using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Mesh
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        /// Static id counter
        /// </summary>
        private static int ID = 0;

        /// <summary>
        /// Position list cache
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;

        /// <summary>
        /// Mesh id
        /// </summary>
        public int Id = -1;
        /// <summary>
        /// Vertices cache
        /// </summary>
        public IVertexData[] Vertices = null;
        /// <summary>
        /// Indexed model
        /// </summary>
        public bool Indexed = false;
        /// <summary>
        /// Indices cache
        /// </summary>
        public uint[] Indices = null;
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        public int VertexBufferOffset = -1;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        public int VertexBufferSlot = -1;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        public int IndexBufferOffset = -1;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        public int IndexBufferSlot = -1;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Material name
        /// </summary>
        public string Material { get; private set; }
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertextType { get; private set; }
        /// <summary>
        /// Has skin
        /// </summary>
        public bool Skinned { get; private set; }
        /// <summary>
        /// Has textures
        /// </summary>
        public bool Textured { get; private set; }
        /// <summary>
        /// Topology
        /// </summary>
        public PrimitiveTopology Topology { get; private set; }
        /// <summary>
        /// Stride
        /// </summary>
        public int VertexBufferStride { get; internal set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertexCount { get; internal set; }
        /// <summary>
        /// Index count
        /// </summary>
        public int IndexCount { get; internal set; }
        /// <summary>
        /// Is instanced
        /// </summary>
        public bool Instanced { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="material">Material name</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="instanced">Instanced</param>
        public Mesh(string name, string material, PrimitiveTopology topology, IVertexData[] vertices, uint[] indices, bool instanced)
        {
            this.Id = ++ID;
            this.Name = name;
            this.Material = material;
            this.Topology = topology;
            this.Vertices = vertices;
            this.VertexCount = vertices.Length;
            this.Indexed = (indices != null && indices.Length > 0);
            this.Indices = indices;
            this.IndexCount = (indices != null ? indices.Length : 0);
            this.VertextType = vertices[0].VertexType;
            this.VertexBufferStride = vertices[0].GetStride();
            this.Textured = VertexData.IsTextured(vertices[0].VertexType);
            this.Skinned = VertexData.IsSkinned(vertices[0].VertexType);
            this.Instanced = instanced;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {

        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public virtual void Draw(Graphics graphics)
        {
            if (this.Indexed)
            {
                if (this.IndexCount > 0)
                {
                    graphics.DeviceContext.DrawIndexed(
                        this.IndexCount,
                        this.IndexBufferOffset, this.VertexBufferOffset);
                }
            }
            else
            {
                if (this.VertexCount > 0)
                {
                    graphics.DeviceContext.Draw(
                        this.VertexCount,
                        this.VertexBufferOffset);
                }
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        /// <param name="count">Instance count</param>
        public virtual void Draw(Graphics graphics, int startInstanceLocation, int count)
        {
            if (count > 0)
            {
                if (this.Indexed)
                {
                    if (this.IndexCount > 0)
                    {
                        graphics.DeviceContext.DrawIndexedInstanced(
                            this.IndexCount,
                            count,
                            this.IndexBufferOffset, this.VertexBufferOffset, startInstanceLocation);
                    }
                }
                else
                {
                    if (this.VertexCount > 0)
                    {
                        graphics.DeviceContext.DrawInstanced(
                            this.VertexCount,
                            count,
                            this.VertexBufferOffset, startInstanceLocation);
                    }
                }
            }
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(bool refresh = false)
        {
            if (refresh || this.positionCache == null)
            {
                List<Vector3> positionList = new List<Vector3>();

                if (this.Vertices != null && this.Vertices.Length > 0)
                {
                    if (this.Vertices[0].HasChannel(VertexDataChannels.Position))
                    {
                        Array.ForEach(this.Vertices, v =>
                        {
                            positionList.Add(v.GetChannelValue<Vector3>(VertexDataChannels.Position));
                        });
                    }
                }

                this.positionCache = positionList.ToArray();
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.positionCache == null)
            {
                Vector3[] res = new Vector3[this.Vertices.Length];

                for (int i = 0; i < this.Vertices.Length; i++)
                {
                    res[i] = VertexData.ApplyWeight(this.Vertices[i], boneTransforms);
                }

                this.positionCache = res;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                Vector3[] positions = this.GetPoints(refresh);
                if (positions != null && positions.Length > 0)
                {
                    if (this.Indices != null && this.Indices.Length > 0)
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                    }
                    else
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                    }
                }
            }

            return this.triangleCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                Vector3[] positions = this.GetPoints(boneTransforms, refresh);

                if (this.Indices != null && this.Indices.Length > 0)
                {
                    this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                }
                else
                {
                    this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                }
            }

            return this.triangleCache;
        }
    }
}
