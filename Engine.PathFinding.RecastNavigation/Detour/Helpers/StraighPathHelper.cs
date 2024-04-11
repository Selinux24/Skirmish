using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Staight path helper
    /// </summary>
    class StraighPathHelper : IDisposable
    {
        /// <summary>
        /// Zero tolerance
        /// </summary>
        private const float toleranze = 0.001f * 0.001f;

        /// <summary>
        /// Navigation mesh
        /// </summary>
        private readonly NavMesh m_nav;

        /// <summary>
        /// Result path
        /// </summary>
        private StraightPath m_resultPath;
        /// <summary>
        /// Maximum nodes in the result path
        /// </summary>
        private int m_maxStraightPath;
        /// <summary>
        /// Straight path options
        /// </summary>
        private StraightPathOptions m_options;
        /// <summary>
        /// Source path node count
        /// </summary>
        private int m_pathCount;
        /// <summary>
        /// Source path nodes 
        /// </summary>
        private int[] m_pathNodes;

        private int m_apexIndex;
        private Vector3 m_portalApex;

        private int m_leftPolyRef;
        private PolyTypes m_leftPolyType;
        private int leftIndex;
        private Vector3 m_portalLeft;

        private int m_rightPolyRef;
        private PolyTypes m_rightPolyType;
        private int m_rightIndex;
        private Vector3 m_portalRight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        public StraighPathHelper(NavMesh nav)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~StraighPathHelper()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_pathNodes = null;
            }
        }

        /// <summary>
        /// Gets the result path
        /// </summary>
        public StraightPath GetResultPath()
        {
            return m_resultPath;
        }

        /// <summary>
        /// Finds the straight path from the start to the end position within the polygon corridor.
        /// </summary>
        /// <param name="startPos">Path start position.</param>
        /// <param name="endPos">Path end position.</param>
        /// <param name="path">An array of polygon references that represent the path corridor.</param>
        /// <param name="maxStraightPath">The maximum number of points the straight path arrays can hold.</param>
        /// <param name="options">Query options.</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindStraightPath(Vector3 startPos, Vector3 endPos, SimplePath path, int maxStraightPath, StraightPathOptions options, out StraightPath resultPath)
        {
            m_resultPath = new(maxStraightPath);
            m_maxStraightPath = maxStraightPath;
            m_options = options;
            m_pathCount = path.Count;
            m_pathNodes = path.GetPath() ?? [];

            m_apexIndex = 0;
            m_portalApex = Vector3.Zero;

            m_leftPolyRef = path.Start;
            m_leftPolyType = 0;
            leftIndex = 0;
            m_portalLeft = Vector3.Zero;

            m_rightPolyRef = path.Start;
            m_rightPolyType = 0;
            m_rightIndex = 0;
            m_portalRight = Vector3.Zero;

            var vStatus = ValidateFindStraightPathParams(startPos, endPos, out var closestStartPos, out var closestEndPos);
            if (vStatus != Status.DT_SUCCESS)
            {
                resultPath = new(m_maxStraightPath);

                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var sStatus = Initialize(closestStartPos, closestEndPos);
            if (sStatus != Status.DT_IN_PROGRESS)
            {
                resultPath = GetResultPath();

                return sStatus;
            }

            var cStatus = CalculatePath(closestEndPos, endPos);

            resultPath = GetResultPath();

            return cStatus;
        }
        /// <summary>
        /// Validates the find straight path parameters
        /// </summary>
        /// <param name="startPos">Path start position.</param>
        /// <param name="endPos">Path end position.</param>
        /// <param name="closestStartPos">Returns the closest start position</param>
        /// <param name="closestEndPos">Returns the closest end position</param>
        private Status ValidateFindStraightPathParams(Vector3 startPos, Vector3 endPos, out Vector3 closestStartPos, out Vector3 closestEndPos)
        {
            closestStartPos = Vector3.Zero;
            closestEndPos = Vector3.Zero;

            if (startPos.IsInfinity() || endPos.IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var fpStatus = m_nav.ClosestPointOnPolyBoundary(m_pathNodes[0], startPos, out closestStartPos);
            if (fpStatus.HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var lpStatus = m_nav.ClosestPointOnPolyBoundary(m_pathNodes[m_pathCount - 1], endPos, out closestEndPos);
            if (lpStatus.HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            return Status.DT_SUCCESS;
        }

        /// <summary>
        /// Initialize the path helper
        /// </summary>
        /// <param name="closestStartPos">Closest start position</param>
        /// <param name="closestEndPos">Closest end position</param>
        private Status Initialize(Vector3 closestStartPos, Vector3 closestEndPos)
        {
            // Add start point.
            var startPStatus = AddStartPoint(closestStartPos);
            if (startPStatus != Status.DT_IN_PROGRESS)
            {
                return startPStatus;
            }

            if (m_pathCount <= 1)
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
            m_portalApex = point;
            m_portalLeft = point;
            m_portalRight = point;

            return m_resultPath.AppendVertex(point, StraightPathFlagTypes.DT_STRAIGHTPATH_START, m_pathNodes[0], m_maxStraightPath);
        }
        /// <summary>
        /// Adds an end point
        /// </summary>
        /// <param name="point">Point</param>
        private Status AddEndPoint(Vector3 point)
        {
            m_resultPath.AppendVertex(point, StraightPathFlagTypes.DT_STRAIGHTPATH_END, 0, m_maxStraightPath);

            // Ignore status return value as we're just about to return anyway.
            return Status.DT_SUCCESS | (m_resultPath.Count >= m_maxStraightPath ? Status.DT_BUFFER_TOO_SMALL : 0);
        }
        /// <summary>
        /// Appends a portal in the current segment
        /// </summary>
        /// <param name="point">Point</param>
        private Status AppendPortalsAlongSegment(Vector3 point)
        {
            if ((m_options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) == 0)
            {
                return Status.DT_IN_PROGRESS;
            }

            return AppendPortals(m_pathCount - 1, point);
        }

        /// <summary>
        /// Gets whether the current position is too near from the specified portal
        /// </summary>
        /// <param name="left">Left corner</param>
        /// <param name="right">Right corner</param>
        private bool IsStartNearPortal(Vector3 left, Vector3 right)
        {
            return Utils.DistancePtSegSqr2D(m_portalApex, left, right) < toleranze;
        }
        /// <summary>
        /// Gets whether the path is nearest from de right corner
        /// </summary>
        /// <param name="right">Right corner</param>
        private bool IsRightVertex(Vector3 right)
        {
            return Utils.TriArea2D(m_portalApex, m_portalRight, right) <= 0.0f;
        }
        /// <summary>
        /// Gets whether the path is nearest from de left corner
        /// </summary>
        /// <param name="left">Left corner</param>
        private bool IsLeftVertex(Vector3 left)
        {
            return Utils.TriArea2D(m_portalApex, m_portalLeft, left) >= 0.0f;
        }

        /// <summary>
        /// Calculates de Straigth path
        /// </summary>
        /// <param name="closestEndPos">Closest end position</param>
        /// <param name="endPos">End position</param>
        private Status CalculatePath(Vector3 closestEndPos, Vector3 endPos)
        {
            int i = 0;
            while (i < m_pathCount)
            {
                var (fRes, fStatus, fRestart, left, right, toType) = FindNextPortal(i, closestEndPos, endPos);
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

                var (pRes, pStatus, pRestart) = ProcessPortal(i, left, right, toType);
                if (!pRes)
                {
                    return pStatus;
                }

                if (pRestart)
                {
                    // Restart
                    i = m_apexIndex + 1;
                    continue;
                }

                i++;
            }

            // Append portals along the current straight path segment.
            var aStatus = AppendPortalsAlongSegment(closestEndPos);
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
        /// <param name="closestEndPos">Closest end position</param>
        /// <param name="endPos">End position</param>
        private (bool continueProcess, Status status, bool restart, Vector3 left, Vector3 right, PolyTypes toType) FindNextPortal(int index, Vector3 closestEndPos, Vector3 endPos)
        {
            if (index + 1 >= m_pathCount)
            {
                // End of the path.
                return (true, Status.DT_IN_PROGRESS, false, closestEndPos, closestEndPos, PolyTypes.Ground);
            }

            // Next portal.
            var (pRes, pStatus, left, right, toType) = FindNextPortalPoints(index, endPos);
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
        /// <param name="endPos">End position</param>
        private (bool continueProcess, Status status, Vector3 left, Vector3 right, PolyTypes toType) FindNextPortalPoints(int index, Vector3 endPos)
        {
            var ppStatus = GetPortalPoints(
                m_pathNodes[index], m_pathNodes[index + 1],
                out var left, out var right, out _, out var toType);
            if (!ppStatus.HasFlag(Status.DT_FAILURE))
            {
                return (true, Status.DT_SUCCESS, left, right, toType);
            }

            // Failed to get portal points, in practice this means that path[i+1] is invalid polygon.
            // Clamp the end point to path[i], and return the path so far.
            var cpBoundaryStatus = m_nav.ClosestPointOnPolyBoundary(m_pathNodes[index], endPos, out var closestEndPos);
            if (cpBoundaryStatus.HasFlag(Status.DT_FAILURE))
            {
                // This should only happen when the first polygon is invalid.
                return (false, Status.DT_FAILURE | Status.DT_INVALID_PARAM, left, right, toType);
            }

            // Apeend portals along the current straight path segment.
            if ((m_options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                // Ignore status return value as we're just about to return anyway.
                AppendPortals(index, closestEndPos);
            }

            var stat = AddEndPoint(closestEndPos);

            return (false, stat, left, right, toType);
        }
        /// <summary>
        /// Process the portal
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="left">Left corner</param>
        /// <param name="right">Right corner</param>
        /// <param name="toType">Poly type</param>
        private (bool continueProcess, Status status, bool restart) ProcessPortal(int index, Vector3 left, Vector3 right, PolyTypes toType)
        {
            // Right vertex.
            if (IsRightVertex(right))
            {
                var (pRes, pStatus, restart) = ProcessRight(index, right, toType);
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
                var (pRes, pStatus, restart) = ProcessLeft(index, left, toType);
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
        /// <param name="right">Right corner</param>
        /// <param name="toType">Polygon type</param>
        private (bool continueProcess, Status status, bool restart) ProcessRight(int index, Vector3 right, PolyTypes toType)
        {
            if (Utils.VClosest(m_portalApex, m_portalRight) || Utils.TriArea2D(m_portalApex, m_portalLeft, right) > 0.0f)
            {
                m_portalRight = right;
                m_rightPolyRef = index + 1 < m_pathCount ? m_pathNodes[index + 1] : 0;
                m_rightPolyType = toType;
                m_rightIndex = index;

                return (true, Status.DT_SUCCESS, false);
            }

            // Append portals along the current straight path segment.
            if ((m_options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                var appendStatus = AppendPortals(leftIndex, m_portalLeft);
                if (appendStatus != Status.DT_IN_PROGRESS)
                {
                    return (false, appendStatus, false);
                }
            }

            m_portalApex = m_portalLeft;
            m_apexIndex = leftIndex;

            StraightPathFlagTypes flags = 0;
            if (m_leftPolyRef == 0)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
            }
            else if (m_leftPolyType == PolyTypes.OffmeshConnection)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
            }
            int r = m_leftPolyRef;

            // Append or update vertex
            var stat = m_resultPath.AppendVertex(m_portalApex, flags, r, m_maxStraightPath);
            if (stat != Status.DT_IN_PROGRESS)
            {
                return (false, stat, false);
            }

            m_portalLeft = m_portalApex;
            m_portalRight = m_portalApex;
            leftIndex = m_apexIndex;
            m_rightIndex = m_apexIndex;

            return (true, Status.DT_SUCCESS, true);
        }
        /// <summary>
        /// Process on the left corner
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="left">Left corner</param>
        /// <param name="toType">Polygon type</param>
        private (bool continueProcess, Status status, bool restart) ProcessLeft(int index, Vector3 left, PolyTypes toType)
        {
            if (Utils.VClosest(m_portalApex, m_portalLeft) || Utils.TriArea2D(m_portalApex, m_portalRight, left) < 0.0f)
            {
                m_portalLeft = left;
                m_leftPolyRef = index + 1 < m_pathCount ? m_pathNodes[index + 1] : 0;
                m_leftPolyType = toType;
                leftIndex = index;

                return (true, Status.DT_SUCCESS, false);
            }

            // Append portals along the current straight path segment.
            if ((m_options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
            {
                var appendStatus = AppendPortals(m_rightIndex, m_portalRight);
                if (appendStatus != Status.DT_IN_PROGRESS)
                {
                    return (false, appendStatus, false);
                }
            }

            m_portalApex = m_portalRight;
            m_apexIndex = m_rightIndex;

            StraightPathFlagTypes flags = 0;
            if (m_rightPolyRef == 0)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
            }
            else if (m_rightPolyType == PolyTypes.OffmeshConnection)
            {
                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
            }
            int r = m_rightPolyRef;

            // Append or update vertex
            var stat = m_resultPath.AppendVertex(m_portalApex, flags, r, m_maxStraightPath);
            if (stat != Status.DT_IN_PROGRESS)
            {
                return (false, stat, false);
            }

            m_portalLeft = m_portalApex;
            m_portalRight = m_portalApex;
            leftIndex = m_apexIndex;
            m_rightIndex = m_apexIndex;

            return (true, Status.DT_SUCCESS, true);
        }

        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        private Status GetPortalPoints(int from, int to, out Vector3 left, out Vector3 right, out PolyTypes fromType, out PolyTypes toType)
        {
            left = new();
            right = new();
            fromType = PolyTypes.Ground;
            toType = PolyTypes.Ground;

            var fromT = m_nav.GetTileAndPolyByRef(from);
            if (fromT.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            fromType = fromT.Poly.Type;

            var toT = m_nav.GetTileAndPolyByRef(to);
            if (toT.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            toType = toT.Poly.Type;

            return TileRef.GetPortalPoints(fromT, toT, out left, out right);
        }
        /// <summary>
        /// Appends intermediate portal points to a straight path.
        /// </summary>
        private Status AppendPortals(int endIdx, Vector3 endPos)
        {
            var startPos = m_resultPath.EndPath;

            // Append or update last vertex
            for (int i = m_apexIndex; i < endIdx; i++)
            {
                // Calculate portal
                int from = m_pathNodes[i];
                var fromT = m_nav.GetTileAndPolyByRef(from);
                if (fromT.Ref == 0)
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                int to = m_pathNodes[i + 1];
                var toT = m_nav.GetTileAndPolyByRef(to);
                if (toT.Ref == 0)
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                if (TileRef.GetPortalPoints(fromT, toT, out var left, out var right).HasFlag(Status.DT_FAILURE))
                {
                    break;
                }

                if (m_options.HasFlag(StraightPathOptions.AreaCrossings) && fromT.Poly.Area == toT.Poly.Area)
                {
                    // Skip intersection if only area crossings are requested.
                    continue;
                }

                // Append intersection
                if (Utils.IntersectSegments2D(startPos, endPos, left, right, out _, out float t))
                {
                    var pt = Vector3.Lerp(left, right, t);

                    var stat = m_resultPath.AppendVertex(pt, 0, m_pathNodes[i + 1], m_maxStraightPath);
                    if (stat != Status.DT_IN_PROGRESS)
                    {
                        return stat;
                    }
                }
            }
            return Status.DT_IN_PROGRESS;
        }
    }
}
