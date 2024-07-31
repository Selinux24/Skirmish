using AISamples.Common;
using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Target(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        protected override VertexPositionTexture[] CreateMarking(float height)
        {
            var support = GetSupport();
            var uvs = Constants.TargetUVs;

            return CreateQuadFromSupport(Width, height, support, uvs, 1f);
        }

        public override bool Update(IGameTime gameTime)
        {
            return false;
        }
    }
}
