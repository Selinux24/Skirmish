
namespace Engine.PathFinding
{
    using Engine.Common;

    /// <summary>
    /// Path node
    /// </summary>
    class PathFinderData<T> where T : GraphNode<T>
    {
        /// <summary>
        /// Node State
        /// </summary>
        public GraphNodeStates State = GraphNodeStates.None;
        /// <summary>
        /// Accumulated cost of the path
        /// </summary>
        public float Cost = 0f;
        /// <summary>
        /// Next node
        /// </summary>
        public T NextNode = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public PathFinderData()
        {
            this.State = GraphNodeStates.None;
            this.Cost = 0;
            this.NextNode = null;
        }

        /// <summary>
        /// Resets the route calculation data
        /// </summary>
        public void Reset()
        {
            this.Cost = 0;
            this.State = GraphNodeStates.None;
            this.NextNode = null;
        }
        /// <summary>
        /// Gets the text representation of the node
        /// </summary>
        /// <returns>Returns the text representation of the node</returns>
        public override string ToString()
        {
            return string.Format("State {0}; Cost {1};", this.State, this.Cost);
        }
    }
}
