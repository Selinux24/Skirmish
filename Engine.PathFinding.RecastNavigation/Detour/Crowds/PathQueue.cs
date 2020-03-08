using SharpDX;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class PathQueue
    {
        public const int DT_PATHQ_INVALID = 0;
        public const int MAX_QUEUE = 8;

        struct PathQuery
        {
            public int R { get; set; }
            /// <summary>
            /// Path find start and end location.
            /// </summary>
            public Vector3 StartPos { get; set; }
            public Vector3 EndPos { get; set; }
            public int StartRef { get; set; }
            public int EndRef { get; set; }
            /// <summary>
            /// Result.
            /// </summary>
            public SimplePath Path { get; set; }
            /// <summary>
            /// State.
            /// </summary>
            public Status Status { get; set; }
            public int KeepAlive { get; set; }
            public QueryFilter Filter { get; set; }
        };

        private readonly PathQuery[] m_queue = new PathQuery[MAX_QUEUE];
        private int m_nextHandle;
        private int m_maxPathSize;
        private int m_queueHead;
        private NavMeshQuery m_navquery;


        public PathQueue()
        {
            m_nextHandle = 1;
            m_maxPathSize = 0;
            m_queueHead = 0;
            m_navquery = null;

            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                m_queue[i].Path = null;
            }
        }

        private void Purge()
        {
            m_navquery?.Dispose();
            m_navquery = null;
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                m_queue[i].Path = null;
            }
        }
        public bool Init(int maxPathSize, int maxSearchNodeCount, NavMesh nav)
        {
            Purge();

            m_navquery = new NavMeshQuery();
            if (m_navquery.Init(nav, maxSearchNodeCount) != Status.DT_SUCCESS)
            {
                return false;
            }

            m_maxPathSize = maxPathSize;
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                m_queue[i].R = DT_PATHQ_INVALID;
                m_queue[i].Path = new SimplePath(m_maxPathSize);
            }

            m_queueHead = 0;

            return true;
        }
        public void Update(int maxIters)
        {
            int MAX_KEEP_ALIVE = 2; // in update ticks.

            // Update path request until there is nothing to update
            // or upto maxIters pathfinder iterations has been consumed.
            int iterCount = maxIters;

            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                PathQuery q = m_queue[m_queueHead % MAX_QUEUE];

                // Skip inactive requests.
                if (q.R == DT_PATHQ_INVALID)
                {
                    m_queueHead++;
                    continue;
                }

                // Handle completed request.
                if (q.Status == Status.DT_SUCCESS || q.Status == Status.DT_FAILURE)
                {
                    // If the path result has not been read in few frames, free the slot.
                    q.KeepAlive++;
                    if (q.KeepAlive > MAX_KEEP_ALIVE)
                    {
                        q.R = DT_PATHQ_INVALID;
                        q.Status = 0;
                    }

                    m_queueHead++;
                    continue;
                }

                // Handle query start.
                if (q.Status == 0)
                {
                    q.Status = m_navquery.InitSlicedFindPath(q.StartRef, q.EndRef, q.StartPos, q.EndPos, q.Filter);
                }
                // Handle query in progress.
                if (q.Status == Status.DT_IN_PROGRESS)
                {
                    q.Status = m_navquery.UpdateSlicedFindPath(iterCount, out int iters);
                    iterCount -= iters;
                }
                if (q.Status == Status.DT_SUCCESS)
                {
                    q.Status = m_navquery.FinalizeSlicedFindPath(m_maxPathSize, out var resPath);
                    if (q.Status == Status.DT_SUCCESS)
                    {
                        q.Path = resPath;
                    }
                }

                if (iterCount <= 0)
                {
                    break;
                }

                m_queueHead++;
            }
        }
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
            q.Path.Count = 0;
            q.Filter = filter;
            q.KeepAlive = 0;

            return r;
        }
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
                    path = q.Path;

                    if (path.Count > maxPath)
                    {
                        path.Path = path.Path.Take(maxPath).ToArray();
                        path.Count = maxPath;
                    }

                    return details | Status.DT_SUCCESS;
                }
            }

            return Status.DT_FAILURE;
        }
        public NavMeshQuery GetNavQuery()
        {
            return m_navquery;
        }
    }
}
