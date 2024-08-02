using AISamples.Common.Persistence;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.Common.Primitives
{
    class Polygon
    {
        private readonly Vector2[] vertices;
        private readonly List<Segment2> segments;

        public Polygon(Vector2[] vertices)
        {
            this.vertices = vertices;

            segments = [];
            for (int i = 0; i < vertices.Length; i++)
            {
                segments.Add(new Segment2(vertices[i], vertices[(i + 1) % vertices.Length]));
            }
        }
        private Polygon(Vector2[] vertices, List<Segment2> segments)
        {
            this.vertices = vertices ?? [];
            this.segments = segments ?? [];
        }

        public static PolygonFile FromPolygon(Polygon polygon)
        {
            return new()
            {
                Vertices = polygon.vertices.Select(Vector2File.FromVector2).ToArray(),
                Segments = polygon.segments.Select(Segment2.FromSegment).ToArray(),
            };
        }
        public static Polygon FromPolygonFile(PolygonFile polygon)
        {
            var vertices = polygon.Vertices.Select(Vector2File.FromVector2File).ToArray();
            var segments = polygon.Segments.Select(Segment2.FromSegmentFile).ToList();

            return new(vertices, segments);
        }

        public Vector2[] GetVertices()
        {
            return [.. vertices];
        }
        public Segment2[] GetSegments()
        {
            return [.. segments];
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
        public bool ContainsSegment(Segment2 segment)
        {
            var midPoint = Vector2.Lerp(segment.P1, segment.P2, 0.5f);

            return ContainsPoint(midPoint);
        }
        public bool ContainsPoint(Vector2 point)
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
        public bool IntersectsPolygonSegments(Polygon poly)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                for (int j = 0; j < poly.segments.Count; j++)
                {
                    var intersection = GetIntersection(segments[i].P1, segments[i].P2, poly.segments[j].P1, poly.segments[j].P2);
                    if (intersection.Exists)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public float DistanceToPoint(Vector2 point)
        {
            float minDistance = float.MaxValue;
            for (int i = 0; i < segments.Count; i++)
            {
                minDistance = MathF.Min(minDistance, segments[i].DistanceToPoint(point));
            }
            return minDistance;
        }
        public float DistanceToPolygon(Polygon poly)
        {
            float minDistance = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                minDistance = MathF.Min(minDistance, poly.DistanceToPoint(vertices[i]));
            }
            return minDistance;
        }

        private static (bool Exists, float Offset, Vector2 Point) GetIntersection(Vector2 s1p1, Vector2 s1p2, Vector2 s2p1, Vector2 s2p2)
        {
            var bottom = (s2p2.Y - s2p1.Y) * (s1p2.X - s1p1.X) - (s2p2.X - s2p1.X) * (s1p2.Y - s1p1.Y);
            if (!MathUtil.WithinEpsilon(0f, bottom, 0.01f))
            {
                var tTop = (s2p2.X - s2p1.X) * (s1p1.Y - s2p1.Y) - (s2p2.Y - s2p1.Y) * (s1p1.X - s2p1.X);
                var uTop = (s2p1.Y - s1p1.Y) * (s1p1.X - s1p2.X) - (s2p1.X - s1p1.X) * (s1p1.Y - s1p2.Y);

                float t = tTop / bottom;
                float u = uTop / bottom;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    return (true, t, new(MathUtil.Lerp(s1p1.X, s1p2.X, t), MathUtil.Lerp(s1p1.Y, s1p2.Y, t)));
                }
            }

            return (false, 0, Vector2.Zero);
        }

        public Vector3[] Extrude(float baseHeight, float height)
        {
            var points = new Vector3[vertices.Length * 2];

            for (int i = 0; i < vertices.Length; i++)
            {
                points[i] = new(vertices[i].X, baseHeight, vertices[i].Y);
                points[i + vertices.Length] = new(vertices[i].X, baseHeight + height, vertices[i].Y);
            }

            return points;
        }
    }
}
