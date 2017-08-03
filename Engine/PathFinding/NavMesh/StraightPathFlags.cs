using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Straight path flags
    /// </summary>
    [Flags]
    public enum StraightPathFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// vertex is in start position of path
        /// </summary>
        Start = 0x01,
        /// <summary>
        /// vertex is in end position of path
        /// </summary>
        End = 0x02,
        /// <summary>
        /// vertex is at start of an off-mesh connection
        /// </summary>
        OffMeshConnection = 0x04,
    }
}
