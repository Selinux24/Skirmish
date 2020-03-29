using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Options for findStraightPath.
    /// </summary>
    [Flags]
    public enum StraightPathOptions
    {
        /// <summary>
        /// DT_STRAIGHTPATH_NONE. None
        /// </summary>
        None = 0x00,
        /// <summary>
        /// DT_STRAIGHTPATH_AREA_CROSSINGS. Add a vertex at every polygon edge crossing where area changes.
        /// </summary>
        AreaCrossings = 0x01,
        /// <summary>
        /// DT_STRAIGHTPATH_ALL_CROSSINGS. Add a vertex at every polygon edge crossing.
        /// </summary>
        AllCrossings = 0x02,
    }
}
