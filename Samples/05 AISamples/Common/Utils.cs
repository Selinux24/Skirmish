using AISamples.Common.Primitives;
using Engine;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AISamples.Common
{
    static class Utils
    {
        public static bool Segment2DIntersectsSegment2D(Segment s1, Segment s2, out Vector3 position, out float distance)
        {
            var a = s1.Point1;
            var b = s1.Point2;
            var c = s2.Point1;
            var d = s2.Point2;

            float bot = (d.Z - c.Z) * (b.X - a.X) - (d.X - c.X) * (b.Z - a.Z);
            if (MathUtil.IsZero(bot))
            {
                position = Vector3.Zero;
                distance = float.MaxValue;
                return false;
            }

            float tTop = (d.X - c.X) * (a.Z - c.Z) - (d.Z - c.Z) * (a.X - c.X);
            float t = tTop / bot;
            if (t < 0 || t > 1)
            {
                position = Vector3.Zero;
                distance = float.MaxValue;
                return false;
            }

            float uTop = (c.Z - a.Z) * (a.X - b.X) - (c.X - a.X) * (a.Z - b.Z);
            float u = uTop / bot;
            if (u < 0 || u > 1)
            {
                position = Vector3.Zero;
                distance = float.MaxValue;
                return false;
            }

            position = new Vector3(MathUtil.Lerp(a.X, b.X, t), 0f, MathUtil.Lerp(a.Z, b.Z, t));
            distance = t;
            return true;
        }

        public static bool Segment2DIntersectsPoly2D(Segment segment, Vector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Length];

                if (Segment2DIntersectsSegment2D(segment, new(p0, p1), out _, out _))
                {
                    return true;
                }
            }

            return false;
        }
      
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
