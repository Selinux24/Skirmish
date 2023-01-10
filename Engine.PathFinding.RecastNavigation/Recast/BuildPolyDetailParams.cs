
namespace Engine.PathFinding.RecastNavigation.Recast
{
    struct BuildPolyDetailParams
    {
        public const int MAX_VERTS = 127;
        public const int MAX_VERTS_PER_EDGE = 32;

        public float SampleDist { get; set; }
        public float SampleMaxError { get; set; }
        public int HeightSearchRadius { get; set; }
    }
}
