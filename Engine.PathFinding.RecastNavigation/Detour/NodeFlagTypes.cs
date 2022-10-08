using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    [Flags]
    public enum NodeFlagTypes
    {
        /// <summary>
        /// DT_NODE_NONE
        /// </summary>
        None = 0x00,
        /// <summary>
        /// DT_NODE_OPEN
        /// </summary>
        Open = 0x01,
        /// <summary>
        /// DT_NODE_CLOSED
        /// </summary>
        Closed = 0x02,
        /// <summary>
        /// DT_NODE_PARENT_DETACHED. Parent of the node is not adjacent. Found using raycast.
        /// </summary>
        ParentDetached = 0x04,
    }
}
