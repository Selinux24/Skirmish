using AISamples.SceneCWRVirtualWorld.Content;
using Engine;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld
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

        public Vector2? GetNearestPoint(Vector2 point, float threshold)
        {
            var thpoints = points.Where(p => Vector2.Distance(p, point) < threshold);
            if (!thpoints.Any())
            {
                return null;
            }

            return thpoints.OrderBy(p => Vector2.Distance(p, point)).First();
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

        public void LoadFromFile(string fileName)
        {
            var graphFile = SerializationHelper.DeserializeJsonFromFile<GraphFile>(fileName);
            var newGraph = GraphFile.FromGraphFile(graphFile);

            Clear();

            points.AddRange(newGraph.points);
            segments.AddRange(newGraph.segments);
     
            Version = Guid.NewGuid();
        }
        public void SaveToFile(string fileName)
        {
            var graphFile = GraphFile.FromGraph(this);
            SerializationHelper.SerializeJsonToFile(graphFile, fileName);
        }
    }
}
