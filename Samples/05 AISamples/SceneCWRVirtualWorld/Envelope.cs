using SharpDX;
using System;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld
{
    class Envelope
    {
        private readonly Segment2 skeleton;
        private readonly Polygon polygon;

        public Envelope(Segment2 skeleton, float width, int roundness)
        {
            this.skeleton = skeleton;

            polygon = GeneratePolygon(width, roundness);
        }

        private Polygon GeneratePolygon(float width, int roundness)
        {
            var p1 = skeleton.P1;
            var p2 = skeleton.P2;

            float radius = width * 0.5f;
            float alpha = MathF.Atan2(p1.Y - p2.Y, p1.X - p2.X);
            float alpha_cw = alpha + MathF.PI * 0.5f;
            float alpha_ccw = alpha - MathF.PI * 0.5f;

            float step = MathF.PI / Math.Max(1, roundness);
            var curve1 = ComputeCurve(p1, alpha_ccw, alpha_cw, 0, radius, step);
            var curve2 = ComputeCurve(p2, alpha_ccw, alpha_cw, MathF.PI, radius, step);

            return new Polygon([.. curve1, .. curve2]);
        }
        private static Vector2 Translate(Vector2 p, float angle, float radius)
        {
            return new Vector2(
                p.X + MathF.Cos(angle) * radius,
                p.Y + MathF.Sin(angle) * radius);
        }
        private static IEnumerable<Vector2> ComputeCurve(Vector2 p, float from, float to, float angle, float radius, float step)
        {
            float eps = step * 0.5f;
            for (float i = from; i <= to + eps; i += step)
            {
                yield return Translate(p, angle + i, radius);
            }
        }

        public Polygon GetPolygon()
        {
            return polygon;
        }
        public Vector2[] GetPolygonVertices()
        {
            return polygon.Vertices;
        }
    }
}
