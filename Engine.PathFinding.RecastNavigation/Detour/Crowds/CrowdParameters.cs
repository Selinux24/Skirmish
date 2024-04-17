
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd settings
    /// </summary>
    public class CrowdParameters
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public GraphAgentType Agent { get; private set; }
        /// <summary>
        /// The maximum number of agents the crowd can manage.
        /// </summary>
        public int MaxAgents { get; set; }
        /// <summary>
        /// The maximum radius of any agent that will be added to the crowd.
        /// </summary>
        public float MaxAgentRadius { get; set; }
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="maxAgents">Max agents</param>
        public CrowdParameters(GraphAgentType agent, int maxAgents)
        {
            Agent = agent;
            MaxAgentRadius = agent.Radius;
            MaxAgents = maxAgents;
        }
    }
}
