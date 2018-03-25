
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Options for findStraightPath.
    /// </summary>
    public enum StraightPathOptions
    {
        /// <summary>
        /// Add a vertex at every polygon edge crossing where area changes.
        /// </summary>
        DT_STRAIGHTPATH_AREA_CROSSINGS = 0x01,
        /// <summary>
        /// Add a vertex at every polygon edge crossing.
        /// </summary>
        DT_STRAIGHTPATH_ALL_CROSSINGS = 0x02,
    }
}
