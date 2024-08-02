using AISamples.Common.Persistence;
using AISamples.Common.Primitives;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.Common
{
    class Graph
    {
        private readonly List<Vector2> points = [];
        private readonly List<Segment2> segments = [];

        public Guid Version { get; private set; } = Guid.NewGuid();

        public Graph(Vector2[] points, Segment2[] segments)
        {
            this.points.AddRange(points);
            this.segments.AddRange(segments);
        }

        public static GraphFile FromGraph(Graph graph)
        {
            var points = graph.GetPoints().Select(Vector2File.FromVector2).ToArray();
            var segments = graph.GetSegments().Select(Segment2.FromSegment).ToArray();

            return new()
            {
                Points = points,
                Segments = segments,
            };
        }
        public static Graph FromGraphFile(GraphFile graph)
        {
            var points = graph.Points.Select(Vector2File.FromVector2File).ToArray();
            var segments = graph.Segments.Select(Segment2.FromSegmentFile).ToArray();

            return new(points, segments);
        }

        public bool TryAddPoint(Vector2 point)
        {
            if (ContainsPoint(point))
            {
                return false;
            }

            AddPoint(point);

            return true;
        }
        public void AddPoint(Vector2 point)
        {
            points.Add(point);

            Version = Guid.NewGuid();
        }
        public bool ContainsPoint(Vector2 point)
        {
            foreach (var p in points)
            {
                if (p == point)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetPointCount() => points.Count;
        public int GetPointIndex(Vector2 point) => points.IndexOf(point);
        public Vector2 GetPoint(int index) => points[index];
        public Vector2[] GetPoints() => [.. points];
        public void RemovePoint(Vector2 point)
        {
            points.Remove(point);

            segments.RemoveAll(s => s.P1 == point || s.P2 == point);

            Version = Guid.NewGuid();
        }

        public bool TryAddSegment(Segment2 segment)
        {
            if (ContainsSegment(segment))
            {
                return false;
            }

            AddSegment(segment);

            return true;
        }
        public void AddSegment(Segment2 segment)
        {
            segments.Add(segment);

            Version = Guid.NewGuid();
        }
        public bool ContainsSegment(Segment2 segment)
        {
            foreach (var s in segments)
            {
                if (s == segment)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetSegmentCount() => segments.Count;
        public Segment2 GetSegment(int index) => segments[index];
        public Segment2[] GetSegments() => [.. segments];
        public void RemoveSegment(Segment2 segment)
        {
            segments.Remove(segment);

            Version = Guid.NewGuid();
        }

        public void Clear()
        {
            points.Clear();
            segments.Clear();

            Version = Guid.NewGuid();
        }

        public void UpdatePoint(int index, Vector2 point)
        {
            if (index < 0)
            {
                return;
            }

            var prev = points[index];
            points[index] = point;

            for (int s = 0; s < segments.Count; s++)
            {
                var seg = segments[s];

                if (seg.P1 == prev)
                {
                    seg.P1 = point;
                }
                if (seg.P2 == prev)
                {
                    seg.P2 = point;
                }

                segments[s] = seg;
            }

            Version = Guid.NewGuid();
        }
    }
}
