using System;

namespace AISamples.SceneCWRVirtualWorld.Content
{
    struct WorldFile
    {
        public GraphFile Graph { get; set; }
        public float Height { get; set; }
        public Guid Version { get;  set; }
        public EnvelopeFile[] Envelopes { get; set; }
        public EnvelopeFile[] RoadEnvelopes { get; set; }
        public Segment2File[] RoadBorders { get; set; }
        public BuildingFile[] Buildings { get; set; }
        public TreeFile[] Trees { get; set; }
        public Segment2File[] LaneGuides { get; set; }
        public IMarkingFile[] Markings { get; set; }
    }
}
