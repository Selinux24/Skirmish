using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache mes processor
    /// </summary>
    /// <param name="geometry">Geometry to process</param>
    public class TileCacheMeshProcess(InputGeometry geometry)
    {
        /// <summary>
        /// Input geometry
        /// </summary>
        private readonly InputGeometry geometry = geometry;

        /// <summary>
        /// Process the geometry
        /// </summary>
        /// <param name="param">Navmesh parameters</param>
        /// <param name="bc">Build context</param>
        public void Process(ref NavMeshCreateParams param, TileCacheBuildContext bc)
        {
            // Update poly flags from areas.
            for (int i = 0; i < param.PolyCount; ++i)
            {
                if ((int)bc.PolyMesh.GetArea(i) == (int)AreaTypes.RC_WALKABLE_AREA)
                {
                    bc.PolyMesh.SetArea(i, SamplePolyAreas.Ground);
                }

                bc.PolyMesh.SetFlag(i, SamplePolyFlagTypesExtents.EvaluateArea(bc.PolyMesh.GetArea(i)));
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
