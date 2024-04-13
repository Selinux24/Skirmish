using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Mesh
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Mesh name</param>
    /// <param name="topology">Topology</param>
    /// <param name="transform">World transform</param>
    /// <param name="vertices">Vertices</param>
    /// <param name="indices">Indices</param>
    public class Mesh(string name, Topology topology, Matrix transform, IEnumerable<IVertexData> vertices, IEnumerable<uint> indices) : IDisposable
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
        private readonly IEnumerable<IVertexData> vertices = vertices ?? [];
        /// <summary>
        /// Indices cache
        /// </summary>
        private readonly IEnumerable<uint> indices = indices ?? [];
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
        public int Id { get; set; } = GetNextId();
        /// <summary>
        /// Indexed model
        /// </summary>
        public bool Indexed { get; set; } = indices?.Any() == true;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; } = name;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertextType { get; private set; } = vertices?.FirstOrDefault()?.VertexType ?? VertexTypes.Unknown;
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; private set; } = topology;
        /// <summary>
        /// Transform
        /// </summary>
        public Matrix Transform { get; set; } = transform;

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
        /// <param name="name">Owner name</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public void Initialize(string name, BufferManager bufferManager, bool dynamicBuffers, BufferDescriptor instancingBuffer)
        {
            try
            {
                Logger.WriteTrace(this, $"{name}.{Name} Processing Mesh => {this}");

                //Vertices
                var trnVertices = VertexData.Transform(vertices, Transform);
                VertexBuffer = bufferManager.AddVertexData($"{name}.{Name}", dynamicBuffers, trnVertices, instancingBuffer);

                if (Indexed)
                {
                    //Indices
                    IndexBuffer = bufferManager.AddIndexData($"{name}.{Name}", dynamicBuffers, indices);
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
                dc.DrawIndexed(IndexBuffer, VertexBuffer);

                return;
            }

            dc.Draw(VertexBuffer);
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        public virtual void Draw(IEngineDeviceContext dc, int instanceCount, int startInstanceLocation)
        {
            if (Indexed)
            {
                dc.DrawIndexedInstanced(instanceCount, startInstanceLocation, IndexBuffer, VertexBuffer);

                return;
            }

            dc.DrawInstanced(instanceCount, startInstanceLocation, VertexBuffer);
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
                return [];
            }

            var first = vertices.First();
            if (!first.HasChannel(VertexDataChannels.Position))
            {
                return [];
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
                return [];
            }

            var first = vertices.First();
            if (!first.HasChannel(VertexDataChannels.Position) ||
                !first.HasChannel(VertexDataChannels.BoneIndices) ||
                !first.HasChannel(VertexDataChannels.Weights))
            {
                return [];
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
                return [];
            }

            if (!indices.Any())
            {
                triangleCache = Triangle.ComputeTriangleList(positions);
            }
            else
            {
                triangleCache = Triangle.ComputeTriangleList(positions, indices);
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
                return [];
            }

            if (!indices.Any())
            {
                triangleCache = Triangle.ComputeTriangleList(positions);
            }
            else
            {
                triangleCache = Triangle.ComputeTriangleList(positions, indices);
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
