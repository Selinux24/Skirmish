
namespace Engine.PathFinding.RecastNavigation
{
    public class TileCacheMeshProcess
    {
        private readonly InputGeometry m_geom = null;

        public TileCacheMeshProcess(InputGeometry geometry)
        {
            this.m_geom = geometry;
        }

        public void Process(ref NavMeshCreateParams param, ref SamplePolyAreas[] polyAreas, ref SamplePolyFlagTypes[] polyFlags)
        {
            // Update poly flags from areas.
            for (int i = 0; i < param.polyCount; ++i)
            {
                if ((int)polyAreas[i] == (int)TileCacheAreas.RC_WALKABLE_AREA)
                {
                    polyAreas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                }

                if (polyAreas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                    polyAreas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                    polyAreas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                {
                    polyFlags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                }
                else if (polyAreas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                {
                    polyFlags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                }
                else if (polyAreas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                {
                    polyFlags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
                }
            }

            // Pass in off-mesh connections.
            if (m_geom != null)
            {
                param.offMeshCon = m_geom.GetConnections();
                param.offMeshConCount = m_geom.GetConnectionCount();
            }
        }
    }
}
