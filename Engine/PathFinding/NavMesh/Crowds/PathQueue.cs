namespace Engine.PathFinding.NavMesh.Crowds
{
    class PathQueue
    {
        /// <summary>
        /// Invalid queue
        /// </summary>
        public const int Invalid = 0;
        /// <summary>
        /// Maximum queue length
        /// </summary>
        private const int MaxQueue = 8;
        /// <summary>
        /// Number of times Update() is called
        /// </summary>
        private const int MaxKeepAlive = 2;

        struct PathQuery
        {
            public int Index;

            //path find start and end location
            public PathPoint Start;
            public PathPoint End;

            //result
            public PolygonPath Path;
            public int PathCount;

            //state
            public Status Status;
            public int KeepAlive;
        }

        private PathQuery[] queue;
        private int nextHandle = 1;
        private int queueHead;
        private NavigationMeshQuery navQuery;
        private NavigationMeshQueryFilter navQueryFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSearchNodeCount">Maximum search node count</param>
        /// <param name="nav">Tiled navigation mesh</param>
        public PathQueue(int maxSearchNodeCount, ref TiledNavigationMesh nav)
        {
            this.navQuery = new NavigationMeshQuery(nav, maxSearchNodeCount);
            this.navQueryFilter = new NavigationMeshQueryFilter();

            this.queue = new PathQuery[MaxQueue];
            for (int i = 0; i < MaxQueue; i++)
            {
                queue[i].Index = 0;
                queue[i].Path = new PolygonPath();
            }

            this.queueHead = 0;
        }

        /// <summary>
        /// Updates the queue
        /// </summary>
        /// <param name="maxIters">Maximum iterations</param>
        public void Update(int maxIters)
        {
            //update path request until there is nothing left to update
            //or up to maxIters pathfinder iterations have been consumed
            int iterCount = maxIters;

            for (int i = 0; i < MaxQueue; i++)
            {
                var q = this.queue[this.queueHead % MaxQueue];

                //skip inactive requests
                if (q.Index != 0)
                {
                    //handle completed request
                    if (q.Status != Status.Success && q.Status != Status.Failure)
                    {
                        //handle query start
                        if (q.Status == 0)
                        {
                            q.Status = navQuery.InitSlicedFindPath(q.Start, q.End, navQueryFilter, FindPathOptions.None).ToStatus();
                        }

                        //handle query in progress
                        if (q.Status == Status.InProgress)
                        {
                            int iters = 0;
                            q.Status = navQuery.UpdateSlicedFindPath(iterCount, ref iters).ToStatus();

                            iterCount -= iters;
                        }

                        if (q.Status == Status.Success)
                        {
                            q.Status = navQuery.FinalizeSlicedFindPath(q.Path).ToStatus();
                        }

                        if (iterCount <= 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        q.KeepAlive++;

                        if (q.KeepAlive > MaxKeepAlive)
                        {
                            q.Index = 0;
                            q.Status = 0;
                        }
                    }
                }

                this.queueHead++;
            }
        }
        /// <summary>
        /// Request an empty slot in the path queue
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns>Index of empty slot</returns>
        public int Request(PathPoint start, PathPoint end)
        {
            //find empty slot
            int slot = -1;
            for (int i = 0; i < MaxQueue; i++)
            {
                if (this.queue[i].Index == 0)
                {
                    slot = i;
                    break;
                }
            }

            //could not find slot
            if (slot == -1)
            {
                return PathQueue.Invalid;
            }

            int index = this.nextHandle++;
            if (this.nextHandle == 0)
            {
                this.nextHandle++;
            }

            PathQuery q = this.queue[slot];
            q.Index = index;
            q.Start = start;
            q.End = end;

            q.Status = 0;
            q.PathCount = 0;
            q.KeepAlive = 0;

            this.queue[slot] = q;

            return index;
        }
        /// <summary>
        /// Get the status of the polygon in the path queue
        /// </summary>
        /// <param name="reference">The polygon reference</param>
        /// <returns>The status in the queue</returns>
        public Status GetRequestStatus(int index)
        {
            for (int i = 0; i < MaxQueue; i++)
            {
                if (this.queue[i].Index == index)
                {
                    return this.queue[i].Status;
                }
            }

            return Status.Failure;
        }
        /// <summary>
        /// Gets the path result by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="path">Polygon path result</param>
        /// <returns>Returns the path result by index</returns>
        public bool GetPathResult(int index, out PolygonPath path)
        {
            path = null;

            for (int i = 0; i < MaxQueue; i++)
            {
                if (this.queue[i].Index == index)
                {
                    PathQuery q = this.queue[i];

                    //free request for reuse
                    q.Index = 0;
                    q.Status = 0;

                    path = new PolygonPath(q.Path);

                    this.queue[i] = q;

                    return true;
                }
            }

            return false;
        }
    }
}
