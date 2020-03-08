using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    [Flags]
    public enum DetailTriEdgeFlagTypes
    {
        /// <summary>
        /// Detail triangle edge is part of the poly boundary
        /// </summary>
        DT_DETAIL_EDGE_BOUNDARY = 0x01,		
    }
}
