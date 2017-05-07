
namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Navigation mesh agent type
    /// </summary>
    public class NavigationMeshAgentType : AgentType
    {
        /// <summary>
        /// Gets or sets the maximum climb height.
        /// </summary>
        public float MaxClimb { get; set; }
        /// <summary>
        /// Gets or sets the radius of the agent
        /// </summary>
        public float Radius { get; set; }
    }
}
