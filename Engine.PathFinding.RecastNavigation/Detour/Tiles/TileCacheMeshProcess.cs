using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TileCacheMeshProcess
    {
        private readonly InputGeometry geometry = null;

        public TileCacheMeshProcess(InputGeometry geometry)
        {
            this.geometry = geometry;
        }

        public void Process(ref NavMeshCreateParams param, NavMeshTileBuildContext bc)
        {
            // Update poly flags from areas.
            for (int i = 0; i < param.PolyCount; ++i)
            {
                if ((int)bc.LMesh.Areas[i] == (int)AreaTypes.Walkable)
                {
                    bc.LMesh.Areas[i] = SamplePolyAreas.Ground;
                }

                bc.LMesh.Flags[i] = QueryFilter.EvaluateArea(bc.LMesh.Areas[i]);
            }

            // Pass in off-mesh connections.
            if (geometry != null)
            {
                param.OffMeshCon = geometry.GetConnections().ToArray();
                param.OffMeshConCount = geometry.GetConnectionCount();
            }
        }
    }
}
