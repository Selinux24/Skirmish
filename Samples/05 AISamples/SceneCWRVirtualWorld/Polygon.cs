using SharpDX;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld
{
    class Polygon
    {
        private readonly List<Segment2> segments = [];

        public Vector2[] Vertices { get; private set; }

        public Polygon(Vector2[] vertices)
        {
            Vertices = vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                segments.Add(new Segment2(vertices[i], vertices[(i + 1) % vertices.Length]));
            }
        }

        public static IEnumerable<Segment2> Union(Polygon[] polygons)
        {
            Break(polygons);

            for (int i = 0; i < polygons.Length; i++)
            {
                foreach (var segment in polygons[i].segments)
                {
                    bool keep = KeepSegment(i, segment, polygons);
                    if (keep)
                    {
                        yield return segment;
                    }
                }
            }
        }
        private static bool KeepSegment(int polyIndex, Segment2 segment, Polygon[] polygons)
        {
            for (int j = 0; j < polygons.Length; j++)
            {
                if (polyIndex == j)
                {
                    continue;
                }

                if (polygons[j].ContainsSegment(segment))
                {
                    return false;
                }
            }

            return true;
        }
        private static void Break(Polygon poly1, Polygon poly2)
        {
            var segs1 = poly1.segments;
            var segs2 = poly2.segments;

            for (int i = 0; i < segs1.Count; i++)
            {
                for (int j = 0; j < segs2.Count; j++)
                {
                    var (exists, offset, point) = GetIntersection(
                        segs1[i].P1, segs1[i].P2,
                        segs2[j].P1, segs2[j].P2);

                    if (!exists || MathUtil.IsZero(offset) || MathUtil.IsOne(offset))
                    {
                        continue;
                    }

                    var aux = segs1[i].P2;
                    segs1[i] = new Segment2(segs1[i].P1, point);
                    segs1.Insert(i + 1, new Segment2(point, aux));

                    aux = segs2[j].P2;
                    segs2[j] = new Segment2(segs2[j].P1, point);
                    segs2.Insert(j + 1, new Segment2(point, aux));
                }
            }
        }
        private static void Break(Polygon[] polygons)
        {
            for (int i = 0; i < polygons.Length - 1; i++)
            {
                for (int j = i + 1; j < polygons.Length; j++)
                {
                    Break(polygons[i], polygons[j]);
                }
            }
        }
        private bool ContainsSegment(Segment2 segment)
        {
            var midPoint = Vector2.Lerp(segment.P1, segment.P2, 0.5f);

            return ContainsPoint(midPoint);
        }
        private bool ContainsPoint(Vector2 point)
        {
            var outerPoint = new Vector2(point.X - 1000, point.Y - 1000);
            int intersections = 0;
            for (int i = 0; i < segments.Count; i++)
            {
                var intersection = GetIntersection(outerPoint, point, segments[i].P1, segments[i].P2);
                if (intersection.Exists)
                {
                    intersections++;
                }
            }
            return intersections % 2 == 1;
        }

        private static (bool Exists, float Offset, Vector2 Point) GetIntersection(Vector2 s1p1, Vector2 s1p2, Vector2 s2p1, Vector2 s2p2)
        {
            var tTop = (s2p2.X - s2p1.X) * (s1p1.Y - s2p1.Y) - (s2p2.Y - s2p1.Y) * (s1p1.X - s2p1.X);
            var uTop = (s2p1.Y - s1p1.Y) * (s1p1.X - s1p2.X) - (s2p1.X - s1p1.X) * (s1p1.Y - s1p2.Y);
            var bottom = (s2p2.Y - s2p1.Y) * (s1p2.X - s1p1.X) - (s2p2.X - s2p1.X) * (s1p2.Y - s1p1.Y);

            if (MathUtil.IsZero(bottom))
            {
                return (false, 0, Vector2.Zero);
            }

            float t = tTop / bottom;
            float u = uTop / bottom;
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                return (true, t, new(MathUtil.Lerp(s1p1.X, s1p2.X, t), MathUtil.Lerp(s1p1.Y, s1p2.Y, t)));
            }

            return (false, 0, Vector2.Zero);
        }
    }
}
