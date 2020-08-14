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
        /// Gets the next Id
        /// </summary>
        /// <returns>Returns the next Id</returns>
        private static int GetNextId()
        {
            return ++ID;
        }

        /// <summary>
        /// Position list cache
        /// </summary>
        private IEnumerable<Vector3> positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private IEnumerable<Triangle> triangleCache = null;

        /// <summary>
        /// Mesh id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Vertices cache
        /// </summary>
        public IEnumerable<IVertexData> Vertices { get; set; }
        /// <summary>
        /// Indexed model
        /// </summary>
        public bool Indexed { get; set; } = false;
        /// <summary>
        /// Indices cache
        /// </summary>
        public IEnumerable<uint> Indices { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertextType { get; private set; }
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; private set; }

        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexBuffer { get; set; } = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        public BufferDescriptor IndexBuffer { get; set; } = null;
        /// <summary>
        /// Gets whether the internal state of the mesh is ready from drawing
        /// </summary>
        public bool Ready
        {
            get
            {
                return VertexBuffer.Ready && (IndexBuffer?.Ready ?? true);
            }
        }
        /// <summary>
        /// Gets the primitive count of the mesh
        /// </summary>
        public int Count
        {
            get
            {
                int count = IndexBuffer?.Count > 0 ? IndexBuffer.Count : VertexBuffer.Count;
                switch (Topology)
                {
                    case Topology.LineList:
                        return count / 2;
                    case Topology.TriangleList:
                        return count / 3;
                    default:
                        return count;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        public Mesh(string name, Topology topology, IEnumerable<IVertexData> vertices, IEnumerable<uint> indices)
        {
            this.Id = GetNextId();
            this.Name = name;
            this.Topology = topology;
            this.Vertices = vertices ?? new IVertexData[] { };
            this.VertextType = vertices?.FirstOrDefault()?.VertexType ?? VertexTypes.Unknown;
            this.Indices = indices ?? new uint[] { };
            this.Indexed = indices?.Any() == true;
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
                        this.IndexBuffer.BufferOffset,
                        this.VertexBuffer.BufferOffset);
                }
            }
            else
            {
                if (this.VertexBuffer.Count > 0)
                {
                    graphics.Draw(
                        this.VertexBuffer.Count,
                        this.VertexBuffer.BufferOffset);
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
                            this.IndexBuffer.BufferOffset,
                            this.VertexBuffer.BufferOffset, startInstanceLocation);
                    }
                }
                else
                {
                    if (this.VertexBuffer.Count > 0)
                    {
                        graphics.DrawInstanced(
                            this.VertexBuffer.Count,
                            count,
                            this.VertexBuffer.BufferOffset, startInstanceLocation);
                    }
                }
            }
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            if (refresh || this.positionCache == null)
            {
                var positionList = new List<Vector3>();

                if (this.Vertices.FirstOrDefault().HasChannel(VertexDataChannels.Position))
                {
                    this.Vertices.ToList().ForEach(v =>
                    {
                        positionList.Add(v.GetChannelValue<Vector3>(VertexDataChannels.Position));
                    });
                }

                this.positionCache = positionList.ToArray();
            }

            return this.positionCache?.ToArray() ?? new Vector3[] { };
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.positionCache == null)
            {
                var positionList = new List<Vector3>();

                if (this.Vertices.FirstOrDefault().HasChannel(VertexDataChannels.Position) &&
                    this.Vertices.FirstOrDefault().HasChannel(VertexDataChannels.BoneIndices) &&
                    this.Vertices.FirstOrDefault().HasChannel(VertexDataChannels.Weights))
                {
                    this.Vertices.ToList().ForEach(v =>
                    {
                        positionList.Add(VertexData.ApplyWeight(v, boneTransforms));
                    });
                }

                this.positionCache = positionList.ToArray();
            }

            return this.positionCache?.ToArray() ?? new Vector3[] { };
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                var positions = this.GetPoints(refresh);
                if (positions.Any())
                {
                    if (this.Indices.Any())
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                    }
                    else
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                    }
                }
                else
                {
                    this.triangleCache = new Triangle[] { };
                }
            }

            return this.triangleCache?.ToArray() ?? new Triangle[] { };
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                var positions = this.GetPoints(boneTransforms, refresh);
                if (positions.Any())
                {
                    if (this.Indices.Any())
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                    }
                    else
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                    }
                }
                else
                {
                    this.triangleCache = new Triangle[] { };
                }
            }

            return this.triangleCache?.ToArray() ?? new Triangle[] { };
        }
    }
}
