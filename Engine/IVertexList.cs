using SharpDX;

namespace Engine
{
    /// <summary>
    /// Vertex list interface
    /// </summary>
    public interface IVertexList
    {
        /// <summary>
        /// Gets vertex position list
        /// </summary>
        /// <returns>Returns the vertex position list</returns>
        Vector3[] GetVertices();
    }
}
