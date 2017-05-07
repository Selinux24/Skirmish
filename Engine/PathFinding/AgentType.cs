
namespace Engine.PathFinding
{
    /// <summary>
    /// Agent type
    /// </summary>
    public abstract class AgentType
    {
        /// <summary>
        /// Agent name
        /// </summary>
        public string Name;
        /// <summary>
        /// Gets or sets the height of the agent
        /// </summary>
        public float Height { get; set; }
    }
}
