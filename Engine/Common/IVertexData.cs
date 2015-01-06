
namespace Engine.Common
{
    /// <summary>
    /// Vertex data
    /// </summary>
    public interface IVertexData : IBufferData
    {
        /// <summary>
        /// Vertex type
        /// </summary>
        VertexTypes VertexType { get; }
    }
}
