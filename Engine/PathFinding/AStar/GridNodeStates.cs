
namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Graph node sate
    /// </summary>
    public enum GridNodeStates : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Clear for walk
        /// </summary>
        Clear = 1,
        /// <summary>
        /// Closed for walf
        /// </summary>
        Closed = 2,
    }
}
