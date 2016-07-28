using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Determine which list the node is in.
    /// </summary>
    [Flags]
    public enum NodeFlags
    {
        /// <summary>
        /// Open list contains nodes to examine.
        /// </summary>
        Open = 0x01,
        /// <summary>
        /// Closed list stores path.
        /// </summary>
        Closed = 0x02
    }
}
