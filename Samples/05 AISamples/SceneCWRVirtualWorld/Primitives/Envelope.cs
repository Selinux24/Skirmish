using SharpDX;
using System;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld.Primitives
{
    class Envelope
    {
        private readonly Segment2 skeleton;
        private readonly Polygon polygon;
        private readonly float width;
        private readonly int roundness;

        public Envelope(Segment2 skeleton, float width, int roundness)
        {
            this.skeleton = skeleton;
            this.width = width;
            this.roundness = roundness;

            polygon = GeneratePolygon();
        }

        private Polygon GeneratePolygon()
        {
            var p1 = skeleton.P1;
            var p2 = skeleton.P2;

            float radius = width * 0.5f;
            float alpha = Utils.Angle(p1.Y - p2.Y, p1.X - p2.X);
            float alpha_cw = alpha + MathF.PI * 0.5f;
            float alpha_ccw = alpha - MathF.PI * 0.5f;

            float step = MathF.PI / Math.Max(1, roundness);
            var curve1 = ComputeCurve(p1, alpha_ccw, alpha_cw, 0, radius, step);
            var curve2 = ComputeCurve(p2, alpha_ccw, alpha_cw, MathF.PI, radius, step);

            return new Polygon([.. curve1, .. curve2]);
        }
        private static IEnumerable<Vector2> ComputeCurve(Vector2 p, float from, float to, float angle, float radius, float step)
        {
            float eps = step * 0.5f;
            for (float i = from; i <= to + eps; i += step)
            {
                yield return Utils.Translate(p, angle + i, radius);
            }
        }

        public Envelope Scale(float scale)
        {
            return new(skeleton, width * scale, roundness);
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
