
namespace Engine.PathFinding.RecastNavigation
{
    public class TileCacheMeshProcess
    {
        private readonly InputGeometry m_geom = null;

        public TileCacheMeshProcess(InputGeometry geometry)
        {
            this.m_geom = geometry;
        }

        public void Process(ref NavMeshCreateParams param, NavMeshTileBuildContext bc)
        {
            // Update poly flags from areas.
            for (int i = 0; i < param.polyCount; ++i)
            {
                if ((int)bc.LMesh.Areas[i] == (int)TileCacheAreas.RC_WALKABLE_AREA)
                {
                    bc.LMesh.Areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                }

                if (bc.LMesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                    bc.LMesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                    bc.LMesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                {
                    bc.LMesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                }
                else if (bc.LMesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                {
                    bc.LMesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                }
                else if (bc.LMesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                {
                    bc.LMesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
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
