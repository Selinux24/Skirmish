using SharpDX;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld
{
    class Graph
    {
        private readonly List<Vector2> points = [];
        private readonly List<Segment2> segments = [];

        public Graph(Vector2[] points, Segment2[] segments)
        {
            this.points.AddRange(points);
            this.segments.AddRange(segments);
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
        public Vector2 GetPoint(int index) => points[index];
        public Vector2[] GetPoints() => [.. points];
        public void RemovePoint(Vector2 point)
        {
            points.Remove(point);

            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].P1 == point || segments[i].P2 == point)
                {
                    segments.RemoveAt(i);
                    i--;
                }
            }
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
        }

        public void Clear()
        {
            points.Clear();
            segments.Clear();
        }
    }
}
