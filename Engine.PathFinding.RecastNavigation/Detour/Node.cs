using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Node
    /// </summary>
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
        public int Ref { get; set; }
        /// <summary>
        /// Gets whether the node is open or not
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return Flags.HasFlag(NodeFlagTypes.Open);
            }
        }
        /// <summary>
        /// Gets whether the node is closed or not
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return Flags.HasFlag(NodeFlagTypes.Closed);
            }
        }

        /// <summary>
        /// Clears the node flag
        /// </summary>
        public void Clear()
        {
            Flags = NodeFlagTypes.None;
        }
        /// <summary>
        /// Removes the opened state
        /// </summary>
        public void RemoveOpened()
        {
            Flags &= ~NodeFlagTypes.Open;
        }
        /// <summary>
        /// Sets the opened state
        /// </summary>
        public void SetOpened()
        {
            Flags &= ~NodeFlagTypes.Closed;
            Flags |= NodeFlagTypes.Open;
        }
        /// <summary>
        /// Removes de closed state
        /// </summary>
        public void RemoveClosed()
        {
            Flags &= ~NodeFlagTypes.Closed;
        }
        /// <summary>
        /// Sets the closed state
        /// </summary>
        public void SetClosed()
        {
            Flags &= ~NodeFlagTypes.Open;
            Flags |= NodeFlagTypes.Closed;
        }
    }
}
