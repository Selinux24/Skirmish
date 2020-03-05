
namespace Engine.PathFinding.RecastNavigation.Crowds
{
    /// <summary>
    /// Provides neighbor data for agents managed by the crowd.
    /// </summary>
    public class CrowdNeighbour
    {
        /// <summary>
        /// The index of the neighbor in the crowd.
        /// </summary>
        public int Idx { get; set; }
        /// <summary>
        /// The distance between the current agent and the neighbor.
        /// </summary>
        public float Dist { get; set; }
    }
}
