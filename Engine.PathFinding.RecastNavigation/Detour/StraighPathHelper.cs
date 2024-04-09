using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Staight path helper
    /// </summary>
    /// <param name="path">Simple path</param>
    /// <param name="maxStraightPath">Maximum straight path nodes</param>
    /// <param name="options">Straight path options</param>
    struct StraighPathHelper(SimplePath path, int maxStraightPath, StraightPathOptions options)
    {
        /// <summary>
        /// Zero tolerance
        /// </summary>
        private const float toleranze = 0.001f * 0.001f;

        /// <summary>
        /// Result path
        /// </summary>
        private readonly StraightPath resultPath = new(maxStraightPath);
        /// <summary>
        /// Maximum nodes in the result path
        /// </summary>
        private readonly int maxStraightPath = maxStraightPath;
        /// <summary>
        /// Straight path options
        /// </summary>
        private readonly StraightPathOptions options = options;
        /// <summary>
        /// Source path node count
        /// </summary>
        private readonly int pathCount = path?.Count ?? 0;
        /// <summary>
        /// Source path nodes 
        /// </summary>
        private readonly int[] pathNodes = path?.GetPath() ?? [];

        private int apexIndex = 0;
        private Vector3 portalApex = Vector3.Zero;

        private int leftPolyRef = path?.Start ?? -1;
        private PolyTypes leftPolyType = 0;
        private int leftIndex = 0;
        private Vector3 portalLeft = Vector3.Zero;

        private int rightPolyRef = path?.Start ?? -1;
        private PolyTypes rightPolyType = 0;
        private int rightIndex = 0;
        private Vector3 portalRight = Vector3.Zero;

        /// <summary>
        /// Gets the result path
        /// </summary>
        public readonly StraightPath GetResultPath()
        {
            return resultPath;
        }

        /// <summary>
        /// Initialize the path helper
        /// </summary>
        /// <param name="closestStartPos">Closest start position</param>
        /// <param name="closestEndPos">Closest end position</param>
        public Status Initialize(Vector3 closestStartPos, Vector3 closestEndPos)
        {
            // Add start point.
            var startPStatus = AddStartPoint(closestStartPos);
            if (startPStatus != Status.DT_IN_PROGRESS)
            {
                return startPStatus;
            }

            if (path.Count <= 1)
            {
                return AddEndPoint(closestEndPos);
            }

            return Status.DT_IN_PROGRESS;
        }
        /// <summary>
        /// Adds a start point
        /// </summary>
        /// <param name="point">Point</param>
        private Status AddStartPoint(Vector3 point)
        {
            portalApex = point;
            portalLeft = point;
            portalRight = point;

            return resultPath.AppendVertex(point, StraightPathFlagTypes.DT_STRAIGHTPATH_START, path.Start, maxStraightPath);
        }
        /// <summary>
        /// Adds an end point
        /// </summary>
        /// <param name="point">Point</param>
        private readonly Status AddEndPoint(Vector3 point)
        {
            resultPath.AppendVertex(point, StraightPathFlagTypes.DT_STRAIGHTPATH_END, 0, maxStraightPath);

            // Ignore status return value as we're just about to return anyway.
            return Status.DT_SUCCESS | ((resultPath.Count >= maxStraightPath) ? Status.DT_BUFFER_TOO_SMALL : 0);
        }
        /// <summary>
        /// Appends a portal in the current segment
        /// </summary>
        /// <param name="nmQuery">Query</param>
        /// <param name="point">Point</param>
        private readonly Status AppendPortalsAlongSegment(NavMeshQuery nmQuery, Vector3 point)
        {
            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) == 0)
            {
                return Status.DT_IN_PROGRESS;
            }

            return nmQuery.AppendPortals(
                apexIndex, path.Count - 1, point, pathNodes, maxStraightPath, options,
                resultPath);
        }

        /// <summary>
        /// Gets whether the current position is too near from the specified portal
        /// </summary>
        /// <param name="left">Left corner</param>
        /// <param name="right">Right corner</param>
        private readonly bool IsStartNearPortal(Vector3 left, Vector3 right)
        {
            return Utils.DistancePtSegSqr2D(portalApex, left, right) < toleranze;
        }
        /// <summary>
        /// Gets whether the path is nearest from de right corner
        /// </summary>
        /// <param name="right">Right corner</param>
        private readonly bool IsRightVertex(Vector3 right)
        {
            return Utils.TriArea2D(portalApex, portalRight, right) <= 0.0f;
        }
        /// <summary>
        /// Gets whether the path is nearest from de left corner
        /// </summary>
        /// <param name="left">Left corner</param>
        private readonly bool IsLeftVertex(Vector3 left)
        {
            return Utils.TriArea2D(portalApex, portalLeft, left) >= 0.0f;
        }

        /// <summary>
        /// Calculates de Straigth path
        /// </summary>
        /// <param name="nmQuery">Query</param>
        /// <param name="closestEndPos">Closest end position</param>
        /// <param name="endPos">End position</param>
        public Status CalculatePath(NavMeshQuery nmQuery, Vector3 closestEndPos, Vector3 endPos)
        {
            int i = 0;
            while (i < pathCount)
            {
                var (fRes, fStatus, fRestart, left, right, toType) = FindNextPortal(i, nmQuery, closestEndPos, endPos);
                if (!fRes)
                {
                    return fStatus;
                }

                if (fRestart)
                {
                    // Restart
                    i++;
                    continue;
                }

                var (pRes, pStatus, pRestart) = ProcessPortal(i, nmQuery, left, right, toType);
                if (!pRes)
                {
                    return pStatus;
                }

                if (pRestart)
                {
                    // Restart
                    i = apexIndex + 1;
                    continue;
                }

                i++;
            }

            // Append portals along the current straight path segment.
            var aStatus = AppendPortalsAlongSegment(nmQuery, closestEndPos);
            if (aStatus != Status.DT_IN_PROGRESS)
            {
                return aStatus;
            }

            return AddEndPoint(closestEndPos);
        }
        /// <summary>
        /// Finds the next portal
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="nmQuery">Query</param>
        /// <param name="closestEndPos">Closest end position</param>
        /// <param name="endPos">End position</param>
        private readonly (bool continueProcess, Status status, bool restart, Vector3 left, Vector3 right, PolyTypes toType) FindNextPortal(int index, NavMeshQuery nmQuery, Vector3 closestEndPos, Vector3 endPos)
        {
            if (index + 1 >= pathCount)
            {
                // End of the path.
                return (true, Status.DT_IN_PROGRESS, false, closestEndPos, closestEndPos, PolyTypes.Ground);
            }

            // Next portal.
            var (pRes, pStatus, left, right, toType) = FindNextPortalPoints(index, nmQuery, endPos);
            if (!pRes)
            {
                return (false, pStatus, false, left, right, toType);
            }

            // If starting really close the portal, advance.
            bool advance = index == 0 && IsStartNearPortal(left, right);

            return (true, Status.DT_IN_PROGRESS, advance, left, right, toType);
        }
        /// <summary>
        /// Finds the next portal points
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="nmQuery">Query</param>
        /// <param name="endPos">End position</param>
        private readonly (bool continueProcess, Status status, Vector3 left, Vector3 right, PolyTypes toType) FindNextPortalPoints(int index, NavMeshQuery nmQuery, Vector3 endPos)
        {
            var ppStatus = nmQuery.GetPortalPoints(
                pathNodes[index], pathNodes[index + 1],
                out var left, out var right, out _, out var toType);
            if (!ppStatus.HasFlag(Status.DT_FAILURE))
            {
                return (true, Status.DT_SUCCESS, left, right, toType);
            }

            // Failed to get portal points, in practice this means that path[i+1] is invalid polygon.
            // Clamp the end point to path[i], and return the path so far.
            var cpBoundaryStatus = nmQuery.ClosestPointOnPolyBoundary(pathNodes[index], endPos, out var closestEndPos);
            if (cpBoundaryStatus.HasFlag(Status.DT_FAILURE))
            {
                // This should only happen when the first polygon is invalid.
                return (false, Status.DT_FAILURE | Status.DT_INVALID_PARAM, left, right, toType);
            }

            // Apeend portals along the current straight path segment.
            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                // Ignore status return value as we're just about to return anyway.
                nmQuery.AppendPortals(
                    apexIndex, index, closestEndPos, pathNodes, maxStraightPath, options,
                    resultPath);
            }

            var stat = AddEndPoint(closestEndPos);

            return (false, stat, left, right, toType);
        }
        /// <summary>
        /// Process the portal
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="nmQuery">Query</param>
        /// <param name="left">Left corner</param>
        /// <param name="right">Right corner</param>
        /// <param name="toType">Poly type</param>
        private (bool continueProcess, Status status, bool restart) ProcessPortal(int index, NavMeshQuery nmQuery, Vector3 left, Vector3 right, PolyTypes toType)
        {
            // Right vertex.
            if (IsRightVertex(right))
            {
                var (pRes, pStatus, restart) = ProcessRight(index, nmQuery, right, toType);
                if (!pRes)
                {
                    return (pRes, pStatus, restart);
                }

                if (restart)
                {
                    return (pRes, pStatus, restart);
                }
            }

            // Left vertex.
            if (IsLeftVertex(left))
            {
                var (pRes, pStatus, restart) = ProcessLeft(index, nmQuery, left, toType);
                if (!pRes)
                {
                    return (pRes, pStatus, restart);
                }

                if (restart)
                {
                    return (pRes, pStatus, restart);
                }
            }

            return (true, Status.DT_IN_PROGRESS, false);
        }
        /// <summary>
        /// Process on the right corner
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="nmQuery">Query</param>
        /// <param name="right">Right corner</param>
        /// <param name="toType">Polygon type</param>
        private (bool continueProcess, Status status, bool restart) ProcessRight(int index, NavMeshQuery nmQuery, Vector3 right, PolyTypes toType)
        {
            if (Utils.VClosest(portalApex, portalRight) || Utils.TriArea2D(portalApex, portalLeft, right) > 0.0f)
            {
                portalRight = right;
                rightPolyRef = (index + 1 < pathCount) ? pathNodes[index + 1] : 0;
                rightPolyType = toType;
                rightIndex = index;

                return (true, Status.DT_SUCCESS, false);
            }

            // Append portals along the current straight path segment.
            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                var appendStatus = nmQuery.AppendPortals(
                    apexIndex, leftIndex, portalLeft, pathNodes, maxStraightPath, options,
                    resultPath);
                if (appendStatus != Status.DT_IN_PROGRESS)
                {
                    return (false, appendStatus, false);
                }
            }

            portalApex = portalLeft;
            apexIndex = leftIndex;

            StraightPathFlagTypes flags = 0;
            if (leftPolyRef == 0)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
            }
            else if (leftPolyType == PolyTypes.OffmeshConnection)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
            }
            int r = leftPolyRef;

            // Append or update vertex
            var stat = resultPath.AppendVertex(portalApex, flags, r, maxStraightPath);
            if (stat != Status.DT_IN_PROGRESS)
            {
                return (false, stat, false);
            }

            portalLeft = portalApex;
            portalRight = portalApex;
            leftIndex = apexIndex;
            rightIndex = apexIndex;

            return (true, Status.DT_SUCCESS, true);
        }
        /// <summary>
        /// Process on the left corner
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="nmQuery">Query</param>
        /// <param name="left">Left corner</param>
        /// <param name="toType">Polygon type</param>
        private (bool continueProcess, Status status, bool restart) ProcessLeft(int index, NavMeshQuery nmQuery, Vector3 left, PolyTypes toType)
        {
            if (Utils.VClosest(portalApex, portalLeft) || Utils.TriArea2D(portalApex, portalRight, left) < 0.0f)
            {
                portalLeft = left;
                leftPolyRef = (index + 1 < pathCount) ? pathNodes[index + 1] : 0;
                leftPolyType = toType;
                leftIndex = index;

                return (true, Status.DT_SUCCESS, false);
            }

            // Append portals along the current straight path segment.
            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                var appendStatus = nmQuery.AppendPortals(
                    apexIndex, rightIndex, portalRight, pathNodes, maxStraightPath, options,
                    resultPath);

                if (appendStatus != Status.DT_IN_PROGRESS)
                {
                    return (false, appendStatus, false);
                }
            }

            portalApex = portalRight;
            apexIndex = rightIndex;

            StraightPathFlagTypes flags = 0;
            if (rightPolyRef == 0)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
            }
            else if (rightPolyType == PolyTypes.OffmeshConnection)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
            }
            int r = rightPolyRef;

            // Append or update vertex
            var stat = resultPath.AppendVertex(portalApex, flags, r, maxStraightPath);
            if (stat != Status.DT_IN_PROGRESS)
            {
                return (false, stat, false);
            }

            portalLeft = portalApex;
            portalRight = portalApex;
            leftIndex = apexIndex;
            rightIndex = apexIndex;

            return (true, Status.DT_SUCCESS, true);
        }
    }
}
