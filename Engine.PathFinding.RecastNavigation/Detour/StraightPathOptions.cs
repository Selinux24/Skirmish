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
        /// None
        /// </summary>
        DT_STRAIGHTPATH_NONE = 0x00,
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
