using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Mesh interface
    /// </summary>
    public interface IMesh : IDisposable
    {
        /// <summary>
        /// Topology
        /// </summary>
        Topology Topology { get; }
        /// <summary>
        /// Transform
        /// </summary>
        Matrix Transform { get; set; }
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        BufferDescriptor VertexBuffer { get; set; }
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        BufferDescriptor IndexBuffer { get; set; }
        /// <summary>
        /// Gets whether the internal state of the mesh is ready from drawing
        /// </summary>
        bool Ready { get; }
        /// <summary>
        /// Gets the primitive count of the mesh
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Initializes a mesh
        /// </summary>
        /// <param name="name">Owner name</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        void Initialize(string name, BufferManager bufferManager, bool dynamicBuffers, BufferDescriptor instancingBuffer);

        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="dc">Device context</param>
        void Draw(IEngineDeviceContext dc);
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        void Draw(IEngineDeviceContext dc, int instanceCount, int startInstanceLocation);

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        IEnumerable<Vector3> GetPoints(bool refresh = false);
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        IEnumerable<Vector3> GetPoints(IEnumerable<Matrix> boneTransforms, bool refresh = false);
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        IEnumerable<Triangle> GetTriangles(bool refresh = false);
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        IEnumerable<Triangle> GetTriangles(IEnumerable<Matrix> boneTransforms, bool refresh = false);

        /// <summary>
        /// Vertex type
        /// </summary>
        IVertexData GetVertexType();
    }
}
