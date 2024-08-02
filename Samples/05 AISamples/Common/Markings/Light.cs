using AISamples.Common.Persistence;
using AISamples.Common.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.Common.Markings
{
    class Light(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height, true)
    {
        public LightState LightState { get; set; } = LightState.None;
        public float RedDuration { get; set; } = 15;
        public float YellowDuration { get; set; } = 2;
        public float GreenDuration { get; set; } = 38;

        public override IMarkingFile FromMarking()
        {
            return new LightFile
            {
                Type = nameof(Light),
                Position = Vector2File.FromVector2(Position),
                Direction = Vector2File.FromVector2(Direction),
                Width = Width,
                Height = Height,
                Is3D = Is3D,
                LightState = LightState,
                RedDuration = RedDuration,
                YellowDuration = YellowDuration,
                GreenDuration = GreenDuration,
            };
        }

        public Segment2 GetBorder()
        {
            return GetPolygon().GetSegments()[2];
        }

        protected override IEnumerable<VertexPositionTexture> CreateMarking(float height)
        {
            var support = GetSupport();
            var border = GetBorder();
            var p = border.P2 + border.Direction * border.Length;

            var dir = new Vector3(support.Direction.X, 0, support.Direction.Y);
            const float baseH = 10f;
            const float baseR = 0.5f;
            var baseCenter = new Vector3(p.X, height + baseH * 0.5f, p.Y);
            const float topH = 6f;
            const float topR = 1f;
            var topCenter = new Vector3(p.X, height + baseH + topH * 0.5f, p.Y);
            const float sphR = 0.5f;
            var sphCenter1 = new Vector3(p.X, height + baseH + topH * 0.8f, p.Y) + dir * topR;
            var sphCenter2 = new Vector3(p.X, height + baseH + topH * 0.5f, p.Y) + dir * topR;
            var sphCenter3 = new Vector3(p.X, height + baseH + topH * 0.2f, p.Y) + dir * topR;

            var baseCyl = GeometryUtil.CreateCylinder(Topology.TriangleList, baseCenter, baseR, baseH, 16);
            var topCyl = GeometryUtil.CreateCylinder(Topology.TriangleList, topCenter, topR, topH, 16);
            var sph1 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter1, sphR, 16, 16);
            var sph2 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter2, sphR, 16, 16);
            var sph3 = GeometryUtil.CreateSphere(Topology.TriangleList, sphCenter3, sphR, 16, 16);

            return
            [
                .. Convert(baseCyl, Constants.Gray),
                .. Convert(topCyl, Constants.DarkGreen),
                .. Convert(sph1, LightState == LightState.Red ? Constants.Red : Constants.Gray),
                .. Convert(sph2, LightState == LightState.Yellow ? Constants.Yellow : Constants.Gray),
                .. Convert(sph3, LightState == LightState.Green ? Constants.Green : Constants.Gray),
            ];
        }
        private static IEnumerable<VertexPositionTexture> Convert(GeometryDescriptor g, Vector2 uv)
        {
            // Reverse for now
            var indices = GeometryUtil.ChangeCoordinate(g.Indices).ToArray();
            var vertices = g.Vertices.ToArray();

            for (int i = 0; i < indices.Length; i++)
            {
                int index = (int)indices[i];

                yield return new()
                {
                    Position = vertices[index],
                    Texture = uv,
                };
            }
        }

        public override bool Update(IGameTime gameTime)
        {
            float totalInterval = RedDuration + YellowDuration + GreenDuration;
            float totalTime = gameTime.TotalSeconds;

            float t = totalTime % totalInterval;

            var prev = LightState;
            if (t < RedDuration)
            {
                LightState = LightState.Red;
            }
            else if (t < RedDuration + YellowDuration)
            {
                LightState = LightState.Yellow;
            }
            else
            {
                LightState = LightState.Green;
            }

            return prev != LightState;
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
