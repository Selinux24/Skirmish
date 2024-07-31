using AISamples.Common;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine.BuiltIn.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Parking(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public Segment2[] GetBorders()
        {
            var polySegments = GetPolygon().GetSegments();

            return [polySegments[0], polySegments[2]];
        }

        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.ParkingUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 1f);
        }
    }
}
