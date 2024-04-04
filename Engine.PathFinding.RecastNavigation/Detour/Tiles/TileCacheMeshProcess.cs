using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TileCacheMeshProcess
    {
        private readonly InputGeometry m_geom = null;

        public TileCacheMeshProcess(InputGeometry geometry)
        {
            m_geom = geometry;
        }

        public void Process(ref NavMeshCreateParams param, TileCacheBuildContext bc)
        {
            // Update poly flags from areas.
            for (int i = 0; i < param.PolyCount; ++i)
            {
                if ((int)bc.PolyMesh.Areas[i] == (int)AreaTypes.RC_WALKABLE_AREA)
                {
                    bc.PolyMesh.Areas[i] = SamplePolyAreas.Ground;
                }

                bc.PolyMesh.Flags[i] = SamplePolyFlagTypesExtents.EvaluateArea(bc.PolyMesh.Areas[i]);
            }

            // Pass in off-mesh connections.
            if (m_geom != null)
            {
                param.OffMeshCon = m_geom.GetConnections().ToArray();
                param.OffMeshConCount = m_geom.GetConnectionCount();
            }
        }
    }
}
