
namespace Engine.Common
{
    public enum VertexTypes
    {
        Unknown,

        Billboard,

        Position,
        PositionColor,
        PositionTexture,
        PositionNormalColor,
        PositionNormalTexture,
        PositionNormalTextureTangent,

        PositionSkinned,
        PositionColorSkinned,
        PositionTextureSkinned,
        PositionNormalColorSkinned,
        PositionNormalTextureSkinned,
        PositionNormalTextureTangentSkinned,
    }
}
