
namespace Engine.PathFinding
{
    using Engine.Common;

    /// <summary>
    /// Path node
    /// </summary>
    class PathFinderData
    {
        /// <summary>
        /// Node State
        /// </summary>
        public GridNodeStates State = GridNodeStates.None;
        /// <summary>
        /// Accumulated cost of the path
        /// </summary>
        public float Cost = 0f;
        /// <summary>
        /// Next node
        /// </summary>
        public GridNode NextNode = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public PathFinderData()
        {
            this.State = GridNodeStates.None;
            this.Cost = 0;
            this.NextNode = null;
        }

        /// <summary>
        /// Resets the route calculation data
        /// </summary>
        public void Reset()
        {
            this.Cost = 0;
            this.State = GridNodeStates.None;
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
