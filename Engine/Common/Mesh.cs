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
    public class Mesh<T>(string name, Topology topology, Matrix transform, IEnumerable<T> vertices, IEnumerable<uint> indices) : IMesh
        where T : struct, IVertexData
    {
        /// <summary>
        /// Vertices cache
        /// </summary>
        private readonly IEnumerable<T> vertices = vertices ?? [];
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
        public int Id { get; } = IMesh.GetNextId();
        /// <summary>
        /// Indexed model
        /// </summary>
        public bool Indexed { get; } = indices?.Any() == true;
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;
        /// <inheritdoc/>
        public Topology Topology { get; } = topology;
        /// <inheritdoc/>
        public Matrix Transform { get; } = transform;
        /// <inheritdoc/>
        public BufferDescriptor VertexBuffer { get; private set; } = null;
        /// <inheritdoc/>
        public BufferDescriptor IndexBuffer { get; private set; } = null;
        /// <inheritdoc/>
        public bool Ready
        {
            get
            {
                return (VertexBuffer?.Ready ?? false) && (IndexBuffer?.Ready ?? true);
            }
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual void Draw(IEngineDeviceContext dc)
        {
            if (Indexed)
            {
                dc.DrawIndexed(IndexBuffer, VertexBuffer);

                return;
            }

            dc.Draw(VertexBuffer);
        }
        /// <inheritdoc/>
        public virtual void Draw(IEngineDeviceContext dc, int instanceCount, int startInstanceLocation)
        {
            if (Indexed)
            {
                dc.DrawIndexedInstanced(instanceCount, startInstanceLocation, IndexBuffer, VertexBuffer);

                return;
            }

            dc.DrawInstanced(instanceCount, startInstanceLocation, VertexBuffer);
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        public IVertexData GetVertexType()
        {
            return default(T);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Indexed ?
                $"Id: {Id}; {nameof(T)}; Vertices: {vertices.Count()}; Indices: {indices.Count()}" :
                $"Id: {Id}; {nameof(T)}; Vertices: {vertices.Count()}";
        }
    }
}
