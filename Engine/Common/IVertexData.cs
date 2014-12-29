
namespace Engine.Common
{
    public interface IVertexData : IBufferData
    {
        VertexTypes VertexType { get; }
        IVertexData Convert(VertexData vert);
    }
}
