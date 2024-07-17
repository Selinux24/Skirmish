using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Content
{
    struct GraphFile
    {
        public Vector2File[] Points { get; set; }
        public Segment2File[] Segments { get; set; }

        public static GraphFile FromGraph(Graph graph)
        {
            var points = graph.GetPoints().Select(Vector2File.FromVector2).ToArray();
            var segments = graph.GetSegments().Select(Segment2File.FromSegment).ToArray();

            return new()
            {
                Points = points,
                Segments = segments,
            };
        }

        public static Graph FromGraphFile(GraphFile graph)
        {
            var points = graph.Points.Select(Vector2File.FromVector2File).ToArray();
            var segments = graph.Segments.Select(Segment2File.FromSegmentFile).ToArray();

            return new(points, segments);
        }
    }
}
