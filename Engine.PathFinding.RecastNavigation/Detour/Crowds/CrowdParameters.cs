
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
    public class CrowdParameters(GraphAgentType agent, int maxAgents) : ICrowdParameters<GraphAgentType>
    {
        /// <inheritdoc/>
        public GraphAgentType Agent { get; private set; } = agent;
        /// <inheritdoc/>
        public int MaxAgents { get; set; } = maxAgents;
        /// <inheritdoc/>
        public float MaxAgentRadius { get; set; } = agent.Radius;
        /// <inheritdoc/>
        public int MaxPathResult { get; set; } = 256;
        /// <inheritdoc/>
        public bool SampleVelocityAdaptative { get; set; } = true;
        /// <inheritdoc/>
        public int CollisionResolveIterations { get; set; } = 4;
        /// <inheritdoc/>
        public float CollisionResolveFactor { get; set; } = 0.7f;
    }
}
