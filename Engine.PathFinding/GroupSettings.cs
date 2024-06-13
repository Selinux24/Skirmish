
namespace Engine.PathFinding
{
    /// <summary>
    /// Group settings
    /// </summary>
    /// <param name="agent">Agent type</param>
    /// <param name="maxAgents">Maximum number of agents</param>
    public class GroupSettings<T>(T agent, int maxAgents) where T : AgentType
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public T Agent { get; private set; } = agent;
        /// <summary>
        /// The maximum number of agents the group can manage.
        /// </summary>
        public int MaxAgents { get; set; } = maxAgents;
    }
}
