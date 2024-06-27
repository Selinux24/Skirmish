
namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Path node
    /// </summary>
    class AStarQueryData
    {
        /// <summary>
        /// Node State
        /// </summary>
        public GridNodeStates State { get; set; } = GridNodeStates.None;
        /// <summary>
        /// Accumulated cost of the path
        /// </summary>
        public float Cost { get; set; } = 0f;
        /// <summary>
        /// Next node
        /// </summary>
        public GridNode NextNode { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AStarQueryData()
        {

        }

        /// <summary>
        /// Resets the route calculation data
        /// </summary>
        public void Reset()
        {
            Cost = 0;
            State = GridNodeStates.None;
            NextNode = null;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"State {State}; Cost {Cost};";
        }
    }
}
