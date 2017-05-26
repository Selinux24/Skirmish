using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Flags that can be applied to a region.
    /// </summary>
    [Flags]
    enum RegionFlags
    {
        /// <summary>
        /// The border flag
        /// </summary>
        Border = 0x20000000,
        /// <summary>
        /// The vertex border flag
        /// </summary>
        VertexBorder = 0x40000000,
        /// <summary>
        /// The area border flag
        /// </summary>
        AreaBorder = unchecked((int)0x80000000)
    }
}
