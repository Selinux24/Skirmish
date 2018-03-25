using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class Node
    {
        /// <summary>
        /// Position of the node.
        /// </summary>
        public Vector3 pos;
        /// <summary>
        /// Cost from previous node to current node.
        /// </summary>
        public float cost;
        /// <summary>
        /// Cost up to the node.
        /// </summary>
        public float total;
        /// <summary>
        /// Index to parent node.
        /// </summary>
        public int pidx;
        /// <summary>
        /// Extra state information. A polyRef can have multiple nodes with different extra info. see DT_MAX_STATES_PER_NODE
        /// </summary>
        public int state;
        /// <summary>
        /// Node flags. A combination of dtNodeFlags.
        /// </summary>
        public NodeFlags flags;
        /// <summary>
        /// Polygon ref the node corresponds to.
        /// </summary>
        public int id;
    }
}
