using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum DetailTriEdgeTypes
    {
        /// <summary>
        /// Detail triangle edge is part of the poly boundary
        /// </summary>
        DT_DETAIL_EDGE_BOUNDARY = 0x01,		
    }
}
