
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// The type of navigation mesh polygon the agent is currently traversing.
    /// </summary>
    public enum CrowdAgentState
    {
        /// <summary>
        /// DT_CROWDAGENT_STATE_INVALID. The agent is not in a valid state.
        /// </summary>
        Invalid,
        /// <summary>
        /// DT_CROWDAGENT_STATE_WALKING. The agent is traversing a normal navigation mesh polygon.
        /// </summary>
        Walking,
        /// <summary>
        /// DT_CROWDAGENT_STATE_OFFMESH. The agent is traversing an off-mesh connection.
        /// </summary>
        Offmesh,
    }
}
