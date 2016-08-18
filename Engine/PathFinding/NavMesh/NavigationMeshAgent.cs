
namespace Engine.PathFinding.NavMesh
{
    public class NavigationMeshAgent : Agent
    {
        /// <summary>
        /// Gets or sets the maximum climb height.
        /// </summary>
        public float MaxClimb { get; set; }
        /// <summary>
        /// Gets or sets the height of the agents traversing the <see cref="NavMesh"/>.
        /// </summary>
        public float AgentHeight { get; set; }
        /// <summary>
        /// Gets or sets the radius of the agents traversing the <see cref="NavMesh"/>.
        /// </summary>
        public float AgentRadius { get; set; }
    }
}
