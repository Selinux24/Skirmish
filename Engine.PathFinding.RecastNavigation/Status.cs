using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum Status
    {
        /// <summary>
        /// DT_SUCCESS
        /// </summary>
        Success = 1,
        /// <summary>
        /// DT_INVALID_PARAM
        /// </summary>
        InvalidParam = 2,
        /// <summary>
        /// DT_PARTIAL_RESULT
        /// </summary>
        PartialResult = 3,
        /// <summary>
        /// DT_OUT_OF_NODES
        /// </summary>
        OutOfNodes = 4,
        /// <summary>
        /// DT_BUFFER_TOO_SMALL
        /// </summary>
        BufferTooSmall = 5,
        /// <summary>
        /// DT_FAILURE
        /// </summary>
        Failure = 6,
        /// <summary>
        /// DT_STATUS_DETAIL_MASK
        /// </summary>
        StatusDetailMask = 7,
        /// <summary>
        /// DT_IN_PROGRESS
        /// </summary>
        InProgress = 8,
    }
}
