
namespace Engine.PathFinding
{
    /// <summary>
    /// Graph node sate
    /// </summary>
    public enum GraphNodeStates : int
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
