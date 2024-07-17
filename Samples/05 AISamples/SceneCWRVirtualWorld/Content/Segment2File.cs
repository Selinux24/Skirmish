
namespace AISamples.SceneCWRVirtualWorld.Content
{
    struct Segment2File
    {
        public Vector2File P1 { get; set; }
        public Vector2File P2 { get; set; }

        public static Segment2File FromSegment(Segment2 segment)
        {
            return new()
            {
                P1 = Vector2File.FromVector2(segment.P1),
                P2 = Vector2File.FromVector2(segment.P2),
            };
        }

        public static Segment2 FromSegmentFile(Segment2File segment)
        {
            return new(Vector2File.FromVector2File(segment.P1), Vector2File.FromVector2File(segment.P2));
        }
    }
}
