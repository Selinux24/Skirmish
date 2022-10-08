using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    [Flags]
    public enum DetailTriEdgeFlagTypes
    {
        /// <summary>
        /// DT_DETAIL_EDGE_BOUNDARY. Detail triangle edge is part of the poly boundary
        /// </summary>
        Boundary = 0x01,		
    }
}
