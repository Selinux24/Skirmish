
namespace Engine.PathFinding
{
    /// <summary>
    /// Agent interface
    /// </summary>
    /// <typeparam name="T">Agent type</typeparam>
    public interface IAgent<T> : IControllable, ITransformable3D
        where T : AgentType
    {
        /// <summary>
        /// Agent type
        /// </summary>
        T AgentType { get; }
    }
}
