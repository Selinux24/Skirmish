using AISamples.Common;
using Engine.BuiltIn.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Start(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.CarUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 1f);
        }
    }
}
