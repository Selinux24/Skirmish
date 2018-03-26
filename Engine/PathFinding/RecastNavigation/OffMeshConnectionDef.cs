using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class OffMeshConnectionDef
    {
        public Vector3 Start;
        public Vector3 End;
        public float Radius;
        public int Direction;
        public SamplePolyAreas Area;
        public SamplePolyFlags Flags;
        public int Id;
    }
}
