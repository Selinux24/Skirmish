using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A set of flags that define properties about an off-mesh connection.
    /// </summary>
    [Flags]
    public enum OffMeshConnectionFlags : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The connection is bi-directional.
        /// </summary>
        Bidirectional = 0x1
    }
}
