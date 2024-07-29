using AISamples.SceneCWRVirtualWorld.Primitives;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld
{
    static class Utils
    {
        public static float Angle(float y, float x)
        {
            return MathF.Atan2(y, x);
        }

        public static Vector2 Translate(Vector2 p, float angle, float radius)
        {
            return new Vector2(
                p.X + MathF.Cos(angle) * radius,
                p.Y + MathF.Sin(angle) * radius);
        }

        public static Vector3[] ScaleFromCenter(Vector3[] points, float factor)
        {
            var center = points.Aggregate((a, b) => a + b) / points.Length;

            return points.Select(p => center + (p - center) * factor).ToArray();
        }

        public static Segment2[] Divide(Segment2 segment, float dashSize, float gapSize)
        {
            var length = segment.Length;
            var direction = segment.Direction;
            var dashCount = (int)(length / (dashSize + gapSize));
            var dashLength = dashSize * direction;
            var gapLength = gapSize * direction;
            var dashStart = segment.P1;
            var dashEnd = dashStart + dashLength;

            var dashes = new List<Segment2>();
            for (int i = 0; i < dashCount; i++)
            {
                dashes.Add(new(dashStart, dashEnd));
                dashStart = dashEnd + gapLength;
                dashEnd = dashStart + dashLength;
            }
            dashes.Add(new(dashStart, segment.P2));

            return [.. dashes];
        }

        public static Vector2? GetNearestPoint(Vector2 point, Vector2[] points, float threshold)
        {
            var thpoints = points
                .Select<Vector2, (Vector2 Point, float Distance)>(p => (p, Vector2.Distance(p, point)))
                .Where(p => p.Distance < threshold);
            if (!thpoints.Any())
            {
                return null;
            }

            return thpoints.OrderBy(p => p.Distance).First().Point;
        }

        public static Segment2? GetNearestSegment(Vector2 point, Segment2[] segments, float threshold)
        {
            var thsegments = segments
                .Select<Segment2, (Segment2 Segment, float Distance)>(s => (s, s.DistanceToPoint(point)))
                .Where(s => s.Distance < threshold);
            if (!thsegments.Any())
            {
                return null;
            }

            return thsegments.OrderBy(s => s.Distance).First().Segment;
        }
    }
}
