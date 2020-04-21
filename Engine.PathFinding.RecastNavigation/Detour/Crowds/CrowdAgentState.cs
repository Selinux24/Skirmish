
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// The type of navigation mesh polygon the agent is currently traversing.
    /// </summary>
    public enum CrowdAgentState
    {
        /// <summary>
        /// The agent is not in a valid state.
        /// </summary>
        DT_CROWDAGENT_STATE_INVALID,
        /// <summary>
        /// The agent is traversing a normal navigation mesh polygon.
        /// </summary>
        DT_CROWDAGENT_STATE_WALKING,
        /// <summary>
        /// The agent is traversing an off-mesh connection.
        /// </summary>
        DT_CROWDAGENT_STATE_OFFMESH,
    }
}
