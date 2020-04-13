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
        /// RC_CONTOUR_TESS_WALL_EDGES. Tessellate solid (impassable) edges during contour simplification.
        /// </summary>
        TessellateWallEdges = 0x01,
        /// <summary>
        /// RC_CONTOUR_TESS_AREA_EDGES. Tessellate edges between areas during contour simplification.
        /// </summary>
        TessellateAreaEdges = 0x02,
    }
}
