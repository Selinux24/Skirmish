
namespace Engine.PathFinding
{
    /// <summary>
    /// Agent interface
    /// </summary>
    public interface IAgent : IControllable, ITransformable3D
    {
        /// <summary>
        /// Agent type
        /// </summary>
        AgentType AgentType { get; }
    }
}
