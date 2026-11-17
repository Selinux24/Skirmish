
namespace Engine.Mathematics
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
        /// <summary>
        /// Gets the vertex list stride
        /// </summary>
        /// <returns>Returns the list stride</returns>
        uint GetStride();
    }
}
