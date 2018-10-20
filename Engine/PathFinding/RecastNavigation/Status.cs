using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum Status
    {
        DT_SUCCESS = 1,
        DT_INVALID_PARAM = 2,
        DT_PARTIAL_RESULT = 3,
        DT_OUT_OF_NODES = 4,
        DT_BUFFER_TOO_SMALL = 5,
        DT_FAILURE = 6,
        DT_STATUS_DETAIL_MASK = 7,
        DT_IN_PROGRESS = 8,
    }
}
