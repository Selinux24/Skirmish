
namespace Engine.PathFinding
{
    /// <summary>
    /// Agent interface
    /// </summary>
    /// <typeparam name="T">Agent type</typeparam>
    public interface IAgent<out T> where T : AgentType
    {
        /// <summary>
        /// Agent type
        /// </summary>
        T AgentType { get; }
    }
}
