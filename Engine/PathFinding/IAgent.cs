namespace Engine.PathFinding
{
    using Engine.PathFinding.NavMesh;

    /// <summary>
    /// Agent interface
    /// </summary>
    public interface IAgent : IControllable, ITransformable3D
    {
        /// <summary>
        /// Agent type
        /// </summary>
        NavigationMeshAgentType AgentType { get; }
    }
}
