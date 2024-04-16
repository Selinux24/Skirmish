
namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Agent actions
    /// </summary>
    enum AgentAcionTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Walk
        /// </summary>
        Walk,
        /// <summary>
        /// Jump
        /// </summary>
        Jump,
        /// <summary>
        /// All actions
        /// </summary>
        All = Walk | Jump,
    }
}
