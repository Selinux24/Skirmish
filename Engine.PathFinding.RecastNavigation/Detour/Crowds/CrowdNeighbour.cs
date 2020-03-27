
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Provides neighbor data for agents managed by the crowd.
    /// </summary>
    public class CrowdNeighbour
    {
        /// <summary>
        /// The crowd agent.
        /// </summary>
        public CrowdAgent Agent { get; set; }
        /// <summary>
        /// The distance between the current agent and the neighbor.
        /// </summary>
        public float Dist { get; set; }
    }
}
