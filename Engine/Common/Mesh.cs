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
        /// Vertices cache
        /// </summary>
        private readonly IEnumerable<IVertexData> vertices = Array.Empty<IVertexData>();
        /// <summary>
        /// Indices cache
        /// </summary>
        private readonly IEnumerable<uint> indices = Array.Empty<uint>();
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
        /// Indexed model
        /// </summary>
        public bool Indexed { get; set; } = false;

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
        /// Transform
        /// </summary>
        public Matrix Transform { get; set; }

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
                return (VertexBuffer?.Ready ?? false) && (IndexBuffer?.Ready ?? true);
            }
        }
        /// <summary>
        /// Gets the primitive count of the mesh
        /// </summary>
        public int Count
        {
            get
            {
                int count = IndexBuffer?.Count > 0 ? IndexBuffer.Count : VertexBuffer?.Count ?? 0;
                return Topology switch
                {
                    Topology.LineList => count / 2,
                    Topology.TriangleList => count / 3,
                    _ => count,
                };
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="topology">Topology</param>
        /// <param name="transform">Transform</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        public Mesh(string name, Topology topology, Matrix transform, IEnumerable<IVertexData> vertices, IEnumerable<uint> indices)
        {
            Id = GetNextId();
            Name = name;
            Topology = topology;
            Transform = transform;
            this.vertices = vertices ?? Array.Empty<IVertexData>();
            VertextType = vertices?.FirstOrDefault()?.VertexType ?? VertexTypes.Unknown;
            this.indices = indices ?? Array.Empty<uint>();
            Indexed = indices?.Any() == true;
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
        /// Initializes a mesh
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public void Initialize(Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer)
        {
            try
            {
                Logger.WriteTrace(this, $"{name}.{Name} Processing Mesh => {this}");

                //Vertices
                var trnVertices = VertexData.Transform(vertices, Transform);
                VertexBuffer = game.BufferManager.AddVertexData($"{name}.{Name}", dynamicBuffers, trnVertices, instancingBuffer);

                if (Indexed)
                {
                    //Indices
                    IndexBuffer = game.BufferManager.AddIndexData($"{name}.{Name}", dynamicBuffers, indices);
                }

                Logger.WriteTrace(this, $"{name}.{Name} Processed Mesh => {this}");
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"{name}.{Name} Error Processing Mesh => {ex.Message}", ex);

                throw;
            }
        }

        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="dc">Device context</param>
        public virtual void Draw(IEngineDeviceContext dc)
        {
            if (Indexed)
            {
                if (IndexBuffer.Count > 0)
                {
                    dc.DrawIndexed(
                        IndexBuffer.Count,
                        IndexBuffer.BufferOffset,
                        VertexBuffer.BufferOffset);
                }
            }
            else
            {
                if (VertexBuffer.Count > 0)
                {
                    dc.Draw(
                        VertexBuffer.Count,
                        VertexBuffer.BufferOffset);
                }
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="count">Instance count</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        public virtual void Draw(IEngineDeviceContext dc, int count, int startInstanceLocation)
        {
            if (count <= 0)
            {
                return;
            }

            if (Indexed && IndexBuffer.Count > 0)
            {
                dc.DrawIndexedInstanced(
                    IndexBuffer.Count,
                    count,
                    IndexBuffer.BufferOffset,
                    VertexBuffer.BufferOffset, startInstanceLocation);

                return;
            }

            if (VertexBuffer.Count > 0)
            {
                dc.DrawInstanced(
                    VertexBuffer.Count,
                    count,
                    VertexBuffer.BufferOffset, startInstanceLocation);
            }
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            if (!refresh && positionCache != null)
            {
                return positionCache.ToArray();
            }

            if (!vertices.Any())
            {
                return Enumerable.Empty<Vector3>();
            }

            var first = vertices.First();
            if (!first.HasChannel(VertexDataChannels.Position))
            {
                return Enumerable.Empty<Vector3>();
            }

            positionCache = vertices.Select(v =>
            {
                var p = v.GetChannelValue<Vector3>(VertexDataChannels.Position);

                if (!Transform.IsIdentity)
                {
                    p = Vector3.TransformCoordinate(p, Transform);
                }

                return p;
            });

            return positionCache.ToArray();
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            if (!refresh && positionCache != null)
            {
                return positionCache.ToArray();
            }

            if (!vertices.Any())
            {
                return Enumerable.Empty<Vector3>();
            }

            var first = vertices.First();
            if (!first.HasChannel(VertexDataChannels.Position) ||
                !first.HasChannel(VertexDataChannels.BoneIndices) ||
                !first.HasChannel(VertexDataChannels.Weights))
            {
                return Enumerable.Empty<Vector3>();
            }

            positionCache = vertices.Select(v =>
            {
                var p = VertexData.ApplyWeight(v, boneTransforms);

                if (!Transform.IsIdentity) p = Vector3.TransformCoordinate(p, Transform);

                return p;
            });

            return positionCache.ToArray();
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            if (!refresh && triangleCache != null)
            {
                return triangleCache.ToArray();
            }

            var positions = GetPoints(refresh);
            if (!positions.Any())
            {
                return Enumerable.Empty<Triangle>();
            }

            if (!indices.Any())
            {
                triangleCache = Triangle.ComputeTriangleList(Topology, positions);
            }
            else
            {
                triangleCache = Triangle.ComputeTriangleList(Topology, positions, indices);
            }

            return triangleCache.ToArray();
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            if (!refresh && triangleCache != null)
            {
                return triangleCache.ToArray();
            }

            var positions = GetPoints(boneTransforms, refresh);
            if (!positions.Any())
            {
                return Enumerable.Empty<Triangle>();
            }

            if (!indices.Any())
            {
                triangleCache = Triangle.ComputeTriangleList(Topology, positions);
            }
            else
            {
                triangleCache = Triangle.ComputeTriangleList(Topology, positions, indices);
            }

            return triangleCache.ToArray();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Indexed ?
                $"Id: {Id}; Vertices: {vertices.Count()}; Indices: {indices.Count()}" :
                $"Id: {Id}; Vertices: {vertices.Count()}";
        }
    }
}
