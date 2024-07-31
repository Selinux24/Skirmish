using AISamples.Common;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Stop(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public Segment2 GetBorder()
        {
            return GetPolygon().GetSegments()[2];
        }

        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.StopUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 0.66f);
        }

        public override bool Update(IGameTime gameTime)
        {
            return false;
        }
    }
}
