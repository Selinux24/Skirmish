using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contour build flags.
    /// </summary>
    [Flags]
    public enum BuildContoursFlagTypes
    {
        /// <summary>
        /// Tessellate solid (impassable) edges during contour simplification.
        /// </summary>
        RC_CONTOUR_TESS_WALL_EDGES = 0x01,
        /// <summary>
        /// Tessellate edges between areas during contour simplification.
        /// </summary>
        RC_CONTOUR_TESS_AREA_EDGES = 0x02,
    }
}
