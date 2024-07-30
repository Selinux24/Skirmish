using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Light(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height, true)
    {
        public LightState LightState { get; set; } = LightState.None;

        public Segment2 GetBorder()
        {
            return GetPolygon().GetSegments()[2];
        }

        protected override IEnumerable<VertexPositionTexture> CreateMarking(float height)
        {
            var support = GetSupport();
            var border = GetBorder();
            var p = border.P2 + (border.Direction * border.Length);

            var dir = new Vector3(support.Direction.X, 0, support.Direction.Y);
            const float baseH = 10f;
            const float baseR = 0.5f;
            var baseCenter = new Vector3(p.X, height + (baseH * 0.5f), p.Y);
            const float topH = 6f;
            const float topR = 1f;
            var topCenter = new Vector3(p.X, height + baseH + (topH * 0.5f), p.Y);
            const float sphR = 0.5f;
            var sphCenter1 = new Vector3(p.X, height + baseH + (topH * 0.2f), p.Y) + (dir * topR);
            var sphCenter2 = new Vector3(p.X, height + baseH + (topH * 0.5f), p.Y) + (dir * topR);
            var sphCenter3 = new Vector3(p.X, height + baseH + (topH * 0.8f), p.Y) + (dir * topR);

            var baseCyl = GeometryUtil.CreateCylinder(Topology.TriangleList, baseCenter, baseR, baseH, 16);
            var topCyl = GeometryUtil.CreateCylinder(Topology.TriangleList, topCenter, topR, topH, 16);
            var sph1 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter1, sphR, 16, 16);
            var sph2 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter2, sphR, 16, 16);
            var sph3 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter3, sphR, 16, 16);

            return
            [
                .. Convert(baseCyl, Constants.Black),
                .. Convert(topCyl, Constants.Black),
                .. Convert(sph1, Constants.White),
                .. Convert(sph2, Constants.White),
                .. Convert(sph3, Constants.White),
            ];
        }
        private static IEnumerable<VertexPositionTexture> Convert(GeometryDescriptor g, Vector2 uv)
        {
            for (int i = 0; i < g.Indices.Count(); i++)
            {
                int index = (int)g.Indices.ElementAt(i);

                yield return new()
                {
                    Position = g.Vertices.ElementAt(index),
                    Texture = uv,
                };
            }
        }
    }

    enum LightState
    {
        None,
        Green,
        Yellow,
        Red,
    }
}
