
namespace Common.Utils
{
    public interface IVertex
    {
        int GetStride();
        VertexTypes GetVertexType();
        IVertex Convert(Vertex vert);
    }
}
