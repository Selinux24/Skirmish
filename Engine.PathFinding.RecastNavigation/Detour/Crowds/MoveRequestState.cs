
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public enum MoveRequestState
    {
        /// <summary>
        /// DT_CROWDAGENT_TARGET_NONE
        /// </summary>
        None,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_FAILED
        /// </summary>
        TargetFailed,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_VALID
        /// </summary>
        TargetValid,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_REQUESTING
        /// </summary>
        TargetRequesting,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE
        /// </summary>
        TargetWaitingForQueue,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_WAITING_FOR_PATH
        /// </summary>
        TargetWaitingForPath,
        /// <summary>
        /// DT_CROWDAGENT_TARGET_VELOCITY
        /// </summary>
        TargetVelocity,
    }
}
