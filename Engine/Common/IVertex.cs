
namespace Engine.Common
{
    public interface IVertex : IBuffer
    {
        VertexTypes VertexType { get; }
        IVertex Convert(Vertex vert);
    }
}
