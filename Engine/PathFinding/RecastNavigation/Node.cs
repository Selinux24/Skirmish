using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class Node
    {
        /// <summary>
        /// Position of the node.
        /// </summary>
        public Vector3 Pos { get; set; }
        /// <summary>
        /// Cost from previous node to current node.
        /// </summary>
        public float Cost { get; set; }
        /// <summary>
        /// Cost up to the node.
        /// </summary>
        public float Total { get; set; }
        /// <summary>
        /// Index to parent node.
        /// </summary>
        public int PIdx { get; set; }
        /// <summary>
        /// Extra state information. A polyRef can have multiple nodes with different extra info. see DT_MAX_STATES_PER_NODE
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Node flags. A combination of dtNodeFlags.
        /// </summary>
        public NodeFlagTypes Flags { get; set; }
        /// <summary>
        /// Polygon ref the node corresponds to.
        /// </summary>
        public int Id { get; set; }
    }
}
