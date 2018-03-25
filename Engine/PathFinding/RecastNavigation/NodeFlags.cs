
namespace Engine.PathFinding.RecastNavigation
{
    public enum NodeFlags
    {
        DT_NODE_NONE = 0x00,
        DT_NODE_OPEN = 0x01,
        DT_NODE_CLOSED = 0x02,
        /// <summary>
        /// parent of the node is not adjacent. Found using raycast.
        /// </summary>
        DT_NODE_PARENT_DETACHED = 0x04,
    }
}
