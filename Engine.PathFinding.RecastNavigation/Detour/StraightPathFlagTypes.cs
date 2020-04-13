using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Vertex flags returned by dtNavMeshQuery::findStraightPath.
    /// </summary>
    [Flags]
    public enum StraightPathFlagTypes
    {
        /// <summary>
        /// DT_STRAIGHTPATH_START. The vertex is the start position in the path.
        /// </summary>
        Start = 0x01,
        /// <summary>
        /// DT_STRAIGHTPATH_END. The vertex is the end position in the path.
        /// </summary>
        End = 0x02,
        /// <summary>
        /// DT_STRAIGHTPATH_OFFMESH_CONNECTION. The vertex is the start of an off-mesh connection.
        /// </summary>
        Offmesh = 0x04,
    }
}
