using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    abstract class Marking
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public float Width { get; }
        public float Height { get; }

        private readonly Segment2 support;
        private readonly Polygon polygon;

        protected Marking(Vector2 position, Vector2 direction, float width, float height)
        {
            Position = position;
            Direction = direction;
            Width = width;
            Height = height;

            float angle = Utils.Angle(direction.Y, direction.X);
            support = new(
                Utils.Translate(position, angle, height * 0.5f),
                Utils.Translate(position, angle, -height * 0.5f));

            polygon = new Envelope(support, width, 0).GetPolygon();
        }

        public Segment2 GetSupport()
        {
            return support;
        }
        public Polygon GetPolygon()
        {
            return polygon;
        }

        public bool ContainsPoint(Vector2 point)
        {
            return polygon.ContainsPoint(point);
        }

        public VertexPositionTexture[] Draw(float height)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, support.P1, support.P2, Width, height);
            var tris = Triangle.ComputeTriangleList(t);
            return CreateMarking(tris);
        }
        protected abstract VertexPositionTexture[] CreateMarking(IEnumerable<Triangle> tris);
    }
}
