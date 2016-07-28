using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A set of flags that control the way contours are built.
    /// </summary>
    [Flags]
    public enum ContourBuildFlags
    {
        /// <summary>
        /// Build normally.
        /// </summary>
        None = 0,
        /// <summary>
        /// Tessellate solid edges during contour simplification.
        /// </summary>
        TessellateWallEdges = 0x01,
        /// <summary>
        /// Tessellate edges between areas during contour simplification.
        /// </summary>
        TessellateAreaEdges = 0x02
    }
}
