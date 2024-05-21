
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd settings
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="agent">Agent</param>
    /// <param name="maxAgents">Max agents</param>
    public class CrowdParameters(GraphAgentType agent, int maxAgents)
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public GraphAgentType Agent { get; private set; } = agent;
        /// <summary>
        /// The maximum number of agents the crowd can manage.
        /// </summary>
        public int MaxAgents { get; set; } = maxAgents;
        /// <summary>
        /// The maximum radius of any agent that will be added to the crowd.
        /// </summary>
        public float MaxAgentRadius { get; set; } = agent.Radius;
        /// <summary>
        /// Maximum path result
        /// </summary>
        public int MaxPathResult { get; set; } = 256;
        /// <summary>
        /// Sample velocity adaptative
        /// </summary>
        public bool SampleVelocityAdaptative { get; set; } = true;
        /// <summary>
        /// Collistion resolve iterations
        /// </summary>
        public int CollisionResolveIterations { get; set; } = 4;
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        public float CollisionResolveFactor { get; set; } = 0.7f;
    }
}
