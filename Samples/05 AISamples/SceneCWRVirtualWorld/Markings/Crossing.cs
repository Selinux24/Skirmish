using AISamples.Common;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Crossing(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public Segment2[] GetBorders()
        {
            var polySegments = GetPolygon().GetSegments();

            return [polySegments[0], polySegments[2]];
        }

        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.CrossingUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 1.066f);
        }

        public override bool Update(IGameTime gameTime)
        {
            return false;
        }
    }
}
