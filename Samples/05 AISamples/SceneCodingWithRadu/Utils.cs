using Engine;
using SharpDX;

namespace AISamples.SceneCodingWithRadu
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
    }
}
