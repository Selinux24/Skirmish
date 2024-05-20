
namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd parameters interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    public interface ICrowdParameters<TAgent>
        where TAgent : AgentType
    {
        /// <summary>
        /// Agent type
        /// </summary>
        TAgent Agent { get; }
        /// <summary>
        /// The maximum number of agents the crowd can manage.
        /// </summary>
        int MaxAgents { get; set; }
        /// <summary>
        /// The maximum radius of any agent that will be added to the crowd.
        /// </summary>
        float MaxAgentRadius { get; set; }
        /// <summary>
        /// Maximum path result
        /// </summary>
        int MaxPathResult { get; set; }
        /// <summary>
        /// Sample velocity adaptative
        /// </summary>
        bool SampleVelocityAdaptative { get; set; }
        /// <summary>
        /// Collistion resolve iterations
        /// </summary>
        int CollisionResolveIterations { get; set; }
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        float CollisionResolveFactor { get; set; }
    }
}
