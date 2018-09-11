using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Has transparency
        /// </summary>
        public bool Transparent { get; private set; }
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; private set; }
        /// <summary>
        /// Stride
        /// </summary>
        public int VertexBufferStride { get; internal set; }
        /// <summary>
        /// Is instanced
        /// </summary>
        public bool Instanced { get; protected set; }

        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        public BufferDescriptor IndexBuffer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="material">Material name</param>
        /// <param name="isTransparent">Has transparency</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="instanced">Instanced</param>
        public Mesh(string name, string material, bool isTransparent, Topology topology, IVertexData[] vertices, uint[] indices, bool instanced)
        {
            var vFirst = vertices[0];

            this.Id = ++ID;
            this.Name = name;
            this.Material = material;
            this.Topology = topology;
            this.Vertices = vertices;
            this.Indexed = (indices != null && indices.Length > 0);
            this.Indices = indices;
            this.VertextType = vFirst.VertexType;
            this.VertexBufferStride = vFirst.GetStride();
            this.Textured = VertexData.IsTextured(vFirst.VertexType);
            this.Transparent = isTransparent;
            this.Skinned = VertexData.IsSkinned(vFirst.VertexType);
            this.Instanced = instanced;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Mesh()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
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
                if (this.IndexBuffer.Count > 0)
                {
                    graphics.DrawIndexed(
                        this.IndexBuffer.Count,
                        this.IndexBuffer.Offset, this.VertexBuffer.Offset);
                }
            }
            else
            {
                if (this.VertexBuffer.Count > 0)
                {
                    graphics.Draw(
                        this.VertexBuffer.Count,
                        this.VertexBuffer.Offset);
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
                    if (this.IndexBuffer.Count > 0)
                    {
                        graphics.DrawIndexedInstanced(
                            this.IndexBuffer.Count,
                            count,
                            this.IndexBuffer.Offset, this.VertexBuffer.Offset, startInstanceLocation);
                    }
                }
                else
                {
                    if (this.VertexBuffer.Count > 0)
                    {
                        graphics.DrawInstanced(
                            this.VertexBuffer.Count,
                            count,
                            this.VertexBuffer.Offset, startInstanceLocation);
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
                var positionList = new List<Vector3>();

                if (this.Vertices != null && this.Vertices.Length > 0)
                {
                    if (this.Vertices[0].HasChannel(VertexDataChannels.Position))
                    {
                        this.Vertices.ToList().ForEach(v =>
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
                var res = new List<Vector3>();

                foreach (var v in this.Vertices)
                {
                    res.Add(VertexData.ApplyWeight(v, boneTransforms));
                }

                this.positionCache = res.ToArray();
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
                var positions = this.GetPoints(refresh);
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
                var positions = this.GetPoints(boneTransforms, refresh);

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
