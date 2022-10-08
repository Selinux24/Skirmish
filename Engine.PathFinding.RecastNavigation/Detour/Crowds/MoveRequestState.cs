﻿
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public enum MoveRequestState
    {
        DT_CROWDAGENT_TARGET_NONE = 0,
        DT_CROWDAGENT_TARGET_FAILED,
        DT_CROWDAGENT_TARGET_VALID,
        DT_CROWDAGENT_TARGET_REQUESTING,
        DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE,
        DT_CROWDAGENT_TARGET_WAITING_FOR_PATH,
        DT_CROWDAGENT_TARGET_VELOCITY,
    }
}
