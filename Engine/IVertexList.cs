using System.Collections.Generic;

namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Vertex list interface
    /// </summary>
    public interface IVertexList
    {
        /// <summary>
        /// Gets vertex position list
        /// </summary>
        /// <returns>Returns the vertex position list</returns>
        IEnumerable<Vector3> GetVertices();
        /// <summary>
        /// Gets the vertex list stride
        /// </summary>
        /// <returns>Returns the list stride</returns>
        uint GetStride();
        /// <summary>
        /// Gets the vertex list topology
        /// </summary>
        /// <returns>Returns the list topology</returns>
        Topology GetTopology();
    }
}
