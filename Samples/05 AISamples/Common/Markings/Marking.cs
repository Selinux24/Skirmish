using AISamples.Common.Persistence;
using AISamples.Common.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.Common.Markings
{
    abstract class Marking
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public float Width { get; }
        public float Height { get; }
        public bool Is3D { get; }

        private readonly Segment2 support;
        private readonly Polygon polygon;

        protected Marking(Vector2 position, Vector2 direction, float width, float height, bool is3d = false)
        {
            Position = position;
            Direction = direction;
            Width = width;
            Height = height;
            Is3D = is3d;

            float angle = Utils.Angle(direction.Y, direction.X);
            support = new(
                Utils.Translate(position, angle, height * 0.5f),
                Utils.Translate(position, angle, -height * 0.5f));

            polygon = new Envelope(support, width, 0).GetPolygon();
        }

        public abstract IMarkingFile FromMarking();

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

        public abstract bool Update(IGameTime gameTime);

        public IEnumerable<VertexPositionTexture> Draw(float height)
        {
            return CreateMarking(height);
        }
        protected abstract IEnumerable<VertexPositionTexture> CreateMarking(float height);

        protected static VertexPositionTexture[] CreateQuadFromSupport(float width, float height, Segment2 support, Vector2[] uvs, float scale)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, support.P1, support.P2, width, height);
            var tris = Triangle.ComputeTriangleList(t);

            Vector3[] allPoints = tris.SelectMany(t => new Vector3[] { t.Point3, t.Point2, t.Point1 }).ToArray();
            if (!MathUtil.IsOne(scale))
            {
                allPoints = Utils.ScaleFromCenter(allPoints, scale);
            }

            Vector3[] points = [.. allPoints.Distinct()];

            VertexPositionTexture[] vertices = new VertexPositionTexture[allPoints.Length];
            for (int i = 0; i < allPoints.Length; i++)
            {
                var p = allPoints[i];
                var uv = uvs[Array.IndexOf(points, p)];

                vertices[i] = new VertexPositionTexture() { Position = p, Texture = uv };
            }
            return vertices;
        }
    }
}
