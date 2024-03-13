using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Path queue
    /// </summary>
    public class PathQueue
    {
        public const int DT_PATHQ_INVALID = 0;
        public const int MAX_QUEUE = 8;

        struct PathQuery
        {
            /// <summary>
            /// Queue reference
            /// </summary>
            public int R { get; set; }
            /// <summary>
            /// Path find start location.
            /// </summary>
            public Vector3 StartPos { get; set; }
            /// <summary>
            /// Path find end location.
            /// </summary>
            public Vector3 EndPos { get; set; }
            /// <summary>
            /// Start polygon reference
            /// </summary>
            public int StartRef { get; set; }
            /// <summary>
            /// End polygon reference
            /// </summary>
            public int EndRef { get; set; }
            /// <summary>
            /// Result.
            /// </summary>
            public SimplePath Path { get; set; }
            /// <summary>
            /// State.
            /// </summary>
            public Status Status { get; set; }
            /// <summary>
            /// Keep query alive
            /// </summary>
            public int KeepAlive { get; set; }
            /// <summary>
            /// Query filter
            /// </summary>
            public QueryFilter Filter { get; set; }
        };

        /// <summary>
        /// Navigation mesh query
        /// </summary>
        private readonly NavMeshQuery m_navquery;
        /// <summary>
        /// Queue
        /// </summary>
        private readonly PathQuery[] m_queue = new PathQuery[MAX_QUEUE];
        /// <summary>
        /// Maximum path size
        /// </summary>
        private readonly int m_maxPathSize;
        /// <summary>
        /// Next reference handle
        /// </summary>
        private int m_nextHandle;
        /// <summary>
        /// Queue head
        /// </summary>
        private int m_queueHead;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        /// <param name="maxPathSize">Maximum path size</param>
        /// <param name="maxSearchNodeCount">Maximum search node count</param>
        public PathQueue(NavMesh nav, int maxPathSize, int maxSearchNodeCount)
        {
            m_nextHandle = 1;

            m_navquery = new NavMeshQuery(nav, maxSearchNodeCount);

            m_maxPathSize = maxPathSize;
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                m_queue[i].R = DT_PATHQ_INVALID;
                m_queue[i].Path = new SimplePath(m_maxPathSize);
            }

            m_queueHead = 0;
        }

        /// <summary>
        /// Updates the path queue
        /// </summary>
        /// <param name="maxIters">Maximum number of iterations</param>
        public void Update(int maxIters)
        {
            int MAX_KEEP_ALIVE = 2; // in update ticks.

            // Update path request until there is nothing to update
            // or upto maxIters pathfinder iterations has been consumed.
            int iterCount = maxIters;

            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                PathQuery q = m_queue[m_queueHead % MAX_QUEUE];

                if (!UpdateHeadQuery(ref q, ref iterCount, MAX_KEEP_ALIVE))
                {
                    m_queueHead++;

                    continue;
                }

                if (iterCount <= 0)
                {
                    break;
                }

                m_queueHead++;
            }
        }
        /// <summary>
        /// Updates a path query
        /// </summary>
        /// <param name="q">Path query</param>
        /// <param name="iterCount">Iterations count</param>
        /// <param name="maxKeepAlive">Maximum keep query alive value</param>
        /// <returns>Returns true if the query returns a valid path (DT_IN_PROGRESS or DT_SUCCESS)</returns>
        private bool UpdateHeadQuery(ref PathQuery q, ref int iterCount, int maxKeepAlive)
        {
            // Skip inactive requests.
            if (q.R == DT_PATHQ_INVALID)
            {
                return false;
            }

            // Handle completed request.
            if (q.Status == Status.DT_SUCCESS || q.Status == Status.DT_FAILURE)
            {
                // If the path result has not been read in few frames, free the slot.
                q.KeepAlive++;
                if (q.KeepAlive > maxKeepAlive)
                {
                    q.R = DT_PATHQ_INVALID;
                    q.Status = 0;
                }

                return false;
            }

            // Handle query start.
            if (q.Status == 0)
            {
                PathPoint start = new() { Ref = q.StartRef, Pos = q.StartPos };
                PathPoint end = new() { Ref = q.EndRef, Pos = q.EndPos };
                q.Status = m_navquery.InitSlicedFindPath(q.Filter, start, end);
            }

            // Handle query in progress.
            if (q.Status == Status.DT_IN_PROGRESS)
            {
                q.Status = m_navquery.UpdateSlicedFindPath(iterCount, out int iters);
                iterCount -= iters;
            }

            // Handle success query
            if (q.Status == Status.DT_SUCCESS)
            {
                q.Status = m_navquery.FinalizeSlicedFindPath(m_maxPathSize, out var resPath);
                if (q.Status == Status.DT_SUCCESS)
                {
                    q.Path = resPath;
                }
            }

            return true;
        }
        /// <summary>
        /// Requests a new path
        /// </summary>
        /// <param name="startRef">Starting reference</param>
        /// <param name="endRef">Ending reference</param>
        /// <param name="startPos">Starting position</param>
        /// <param name="endPos">Ending position</param>
        /// <param name="filter">Query filter</param>
        /// <returns>Returns the path index</returns>
        public int Request(int startRef, int endRef, Vector3 startPos, Vector3 endPos, QueryFilter filter)
        {
            // Find empty slot
            int slot = -1;
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                if (m_queue[i].R == DT_PATHQ_INVALID)
                {
                    slot = i;
                    break;
                }
            }
            // Could not find slot.
            if (slot == -1)
            {
                return DT_PATHQ_INVALID;
            }

            int r = m_nextHandle++;
            if (m_nextHandle == DT_PATHQ_INVALID)
            {
                m_nextHandle++;
            }

            PathQuery q = m_queue[slot];
            q.R = r;
            q.StartPos = startPos;
            q.StartRef = startRef;
            q.EndPos = endPos;
            q.EndRef = endRef;

            q.Status = 0;
            q.Path.Clear();
            q.Filter = filter;
            q.KeepAlive = 0;

            return r;
        }
        /// <summary>
        /// Gets the request status
        /// </summary>
        /// <param name="r">Reference</param>
        /// <returns>Returns the request status</returns>
        public Status GetRequestStatus(int r)
        {
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                if (m_queue[i].R == r)
                {
                    return m_queue[i].Status;
                }
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Gets the path result
        /// </summary>
        /// <param name="r">Reference</param>
        /// <param name="maxPath">Maximum path count</param>
        /// <param name="path">The resulting path</param>
        /// <returns>Returns the resulting status</returns>
        public Status GetPathResult(int r, int maxPath, out SimplePath path)
        {
            path = null;

            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                if (m_queue[i].R == r)
                {
                    PathQuery q = m_queue[i];

                    Status details = q.Status & Status.DT_STATUS_DETAIL_MASK;

                    // Free request for reuse.
                    q.R = DT_PATHQ_INVALID;
                    q.Status = 0;

                    // Copy path
                    path = q.Path.Copy(maxPath);

                    return details | Status.DT_SUCCESS;
                }
            }

            return Status.DT_FAILURE;
        }
    }
}
