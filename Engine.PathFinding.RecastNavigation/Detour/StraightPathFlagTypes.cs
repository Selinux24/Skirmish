﻿using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Vertex flags returned by dtNavMeshQuery::findStraightPath.
    /// </summary>
    [Flags]
    public enum StraightPathFlagTypes
    {
        /// <summary>
        /// The vertex is the start position in the path.
        /// </summary>
        DT_STRAIGHTPATH_START = 0x01,
        /// <summary>
        /// The vertex is the end position in the path.
        /// </summary>
        DT_STRAIGHTPATH_END = 0x02,
        /// <summary>
        /// The vertex is the start of an off-mesh connection.
        /// </summary>
        DT_STRAIGHTPATH_OFFMESH_CONNECTION = 0x04,
    }
}
