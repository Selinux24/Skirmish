using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.PathFinding.RecastNavigation.Detour.Helpers;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides the ability to perform pathfinding related queries against a navigation mesh.
    /// </summary>
    public class NavMeshQuery : IDisposable
    {
        /// <summary>
        /// Maximum polygon count in the query
        /// </summary>
        const int MAX_POLYS = 256;
        /// <summary>
        /// Maximum smooth points in the query
        /// </summary>
        const int MAX_SMOOTH = 2048;

        /// <summary>
        /// Navmesh data.
        /// </summary>
        private readonly NavMesh m_nav = null;
        /// <summary>
        /// Maximum nodes
        /// </summary>
        private readonly int m_maxNodes = 0;

        /// <summary>
        /// Find path helper
        /// </summary>
        private FindPathHelper findPathHelper;
        /// <summary>
        /// Sliced path helper
        /// </summary>
        private SlicedPathHelper slicedFindPathHelper;
        /// <summary>
        /// Straight path helper
        /// </summary>
        private StraighPathHelper straighPathHelper;
        /// <summary>
        /// Find polygon helper
        /// </summary>
        private FindPolysHelper findPolysHelper;
        /// <summary>
        /// Find local neighbourhood helper
        /// </summary>
        private FindLocalNeighbourhoodHelper findLocalNeighbourhoodHelper;
        /// <summary>
        /// Move along surface helper
        /// </summary>
        private MoveAlongSurfaceHelper moveAlongSurfaceHelper;
        /// <summary>
        /// Random point around circle helper
        /// </summary>
        private RandomPointAroundCircleHelper randomPointAroundCircleHelper;
        /// <summary>
        /// Wall helper
        /// </summary>
        private WallHelper wallHelper;

        /// <summary>
        /// Find path helper
        /// </summary>
        private FindPathHelper FindPathHelper { get => findPathHelper ??= new(m_nav, m_maxNodes); }
        /// <summary>
        /// Sliced path helper
        /// </summary>
        private SlicedPathHelper SlicedPathHelper { get => slicedFindPathHelper ??= new(m_nav, m_maxNodes); }
        /// <summary>
        /// Straight path helper
        /// </summary>
        private StraighPathHelper StraighPathHelper { get => straighPathHelper ??= new(m_nav); }
        /// <summary>
        /// Find polygon helper
        /// </summary>
        private FindPolysHelper FindPolysHelper { get => findPolysHelper ??= new(m_nav, m_maxNodes); }
        /// <summary>
        /// Find local neighbourhood helper
        /// </summary>
        private FindLocalNeighbourhoodHelper FindLocalNeighbourhoodHelper { get => findLocalNeighbourhoodHelper ??= new(m_nav); }
        /// <summary>
        /// Move along surface helper
        /// </summary>
        private MoveAlongSurfaceHelper MoveAlongSurfaceHelper { get => moveAlongSurfaceHelper ??= new(m_nav); }
        /// <summary>
        /// Random point around circle helper
        /// </summary>
        private RandomPointAroundCircleHelper RandomPointAroundCircleHelper { get => randomPointAroundCircleHelper ??= new(m_nav, m_maxNodes); }
        /// <summary>
        /// Wall helper
        /// </summary>
        private WallHelper WallHelper { get => wallHelper ??= new(m_nav, m_maxNodes); }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Pointer to the dtNavMesh object to use for all queries.</param>
        /// <param name="maxNodes">Maximum number of search nodes.</param>
        public NavMeshQuery(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nameof(nav));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxNodes, 0);

            m_nav = nav;
            m_maxNodes = maxNodes;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~NavMeshQuery()
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
                findPathHelper?.Dispose();
                slicedFindPathHelper?.Dispose();
                straighPathHelper?.Dispose();
                findPolysHelper?.Dispose();
                findLocalNeighbourhoodHelper?.Dispose();
                moveAlongSurfaceHelper?.Dispose();
                randomPointAroundCircleHelper?.Dispose();
                wallHelper?.Dispose();
            }
        }

        /// <summary>
        /// Gets the navigation mesh the query object is using.
        /// </summary>
        /// <returns>The navigation mesh the query object is using.</returns>
        public NavMesh GetAttachedNavMesh() { return m_nav; }

        /// <summary>
        /// Finds a path from the start polygon to the end polygon.
        /// </summary>
        /// <param name="startRef">The refrence id of the start polygon.</param>
        /// <param name="endRef">The reference id of the end polygon.</param>
        /// <param name="startPos">A position within the start polygon.</param>
        /// <param name="endPos">A position within the end polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxPath">The maximum number of polygons the @p path array can hold.</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPath(PathPoint start, PathPoint end, IGraphQueryFilter filter, int maxPath, out SimplePath resultPath)
        {
            return FindPathHelper.FindPath(start, end, filter, maxPath, out resultPath);
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
            return StraighPathHelper.FindStraightPath(startPos, endPos, path, maxStraightPath, options, out resultPath);
        }

        /// <summary>
        /// Intializes a sliced path query.
        /// </summary>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="start">A position within the start polygon.</param>
        /// <param name="end">A position within the end polygon.</param>
        /// <param name="options">Query options</param>
        /// <returns>The status flags for the query.</returns>
        /// <example>
        /// Common use case:
        /// -# Call InitSlicedFindPath() to initialize the sliced path query.
        /// -# Call UpdateSlicedFindPath() until it returns complete.
        /// -# Call FinalizeSlicedFindPath() to get the path.
        /// </example>
        public Status InitSlicedFindPath(IGraphQueryFilter filter, PathPoint start, PathPoint end, FindPathOptions options = FindPathOptions.AnyAngle)
        {
            return SlicedPathHelper.InitSlicedFindPath(filter, start, end, options);
        }
        /// <summary>
        /// Updates an in-progress sliced path query.
        /// </summary>
        /// <param name="maxIter">The maximum number of iterations to perform.</param>
        /// <param name="doneIters">The actual number of iterations completed.</param>
        /// <returns>The status flags for the query.</returns>
        public Status UpdateSlicedFindPath(int maxIter, out int doneIters)
        {
            return SlicedPathHelper.UpdateSlicedFindPath(maxIter, out doneIters);
        }
        /// <summary>
        /// Finalizes and returns the results of a sliced path query.
        /// </summary>
        /// <param name="maxPath">The max number of polygons the path array can hold.</param>
        /// <param name="path">An ordered list of polygon references representing the path. (Start to end.)</param>
        /// <returns>The status flags for the query.</returns>
        public Status FinalizeSlicedFindPath(int maxPath, out SimplePath path)
        {
            return SlicedPathHelper.FinalizeSlicedFindPath(maxPath, out path);
        }
        /// <summary>
        /// Finalizes and returns the results of an incomplete sliced path query, returning the path to the furthest polygon on the existing path that was visited during the search.
        /// </summary>
        /// <param name="existing">An array of polygon references for the existing path.</param>
        /// <param name="maxPath">The max number of polygons the @p path array can hold.</param>
        /// <param name="path">An ordered list of polygon references representing the path. (Start to end.)</param>
        /// <returns>The status flags for the query.</returns>
        public Status FinalizeSlicedFindPathPartial(int maxPath, int[] existing, out SimplePath path)
        {
            return SlicedPathHelper.FinalizeSlicedFindPathPartial(maxPath, existing, out path);
        }

        /// <summary>
        /// Finds the polygons along the navigation graph that touch the specified circle.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="radius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons touched by the circle.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPolysAroundCircle(int startRef, Vector3 centerPos, float radius, IGraphQueryFilter filter, int maxResult, out PolyRefs result)
        {
            return FindPolysHelper.FindPolysAroundCircle(startRef, centerPos, radius, filter, maxResult, out result);
        }
        /// <summary>
        /// Finds the polygons along the naviation graph that touch the specified convex polygon.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="verts">The vertices describing the convex polygon.</param>
        /// <param name="nverts">The number of vertices in the polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons touched by the circle.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPolysAroundShape(int startRef, Vector3[] verts, IGraphQueryFilter filter, int maxResult, out PolyRefs result)
        {
            return FindPolysHelper.FindPolysAroundShape(startRef, verts, filter, maxResult, out result);
        }
        /// <summary>
        /// Finds the polygon nearest to the specified center point.
        /// </summary>
        /// <param name="center">The center of the search box.</param>
        /// <param name="halfExtents">The search distance along each axis.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="nearestRef">The reference id of the nearest polygon.</param>
        /// <param name="nearestPt">The nearest point on the polygon.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindNearestPoly(Vector3 center, Vector3 halfExtents, IGraphQueryFilter filter, out int nearestRef, out Vector3 nearestPt)
        {
            return FindPolysHelper.FindNearestPoly(center, halfExtents, filter, out nearestRef, out nearestPt);
        }

        /// <summary>
        /// Finds the non-overlapping navigation polygons in the local neighbourhood around the center position.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons in the local neighbourhood.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindLocalNeighbourhood(int startRef, Vector3 centerPos, float radius, IGraphQueryFilter filter, int maxResult, out PolyRefs result)
        {
            return FindLocalNeighbourhoodHelper.FindLocalNeighbourhood(startRef, centerPos, radius, filter, maxResult, out result);
        }

        /// <summary>
        /// Returns random location on navmesh.
        /// </summary>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location. </param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// </remarks>
        public Status FindRandomPoint(IGraphQueryFilter filter, out int randomRef, out Vector3 randomPt)
        {
            randomRef = -1;
            randomPt = Vector3.Zero;

            if (filter == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Randomly pick one tile. Assume that all tiles cover roughly the same area.
            var tile = m_nav.PickTile();
            if (tile == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick one polygon weighted by polygon area.
            Poly bestPoly = null;
            int bestPolyRef = 0;
            int bse = m_nav.GetTileRef(tile);

            float areaSum = 0.0f;
            var polys = tile
                .GetPolys()
                .Where(p => p.Type != PolyTypes.OffmeshConnection && filter.PassFilter(p.Flags))
                .ToArray();

            for (int i = 0; i < polys.Length; ++i)
            {
                var poly = polys[i];
                int r = bse | i;

                // Calc area of the polygon.
                float polyArea = tile.GetPolyArea(poly);

                // Choose random polygon weighted by area, using reservoi sampling.
                areaSum += polyArea;
                float u = Helper.RandomGenerator.NextFloat(0, 1);
                if (u * areaSum <= polyArea)
                {
                    bestPoly = poly;
                    bestPolyRef = r;
                }
            }

            if (bestPoly == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            var verts = tile.GetPolyVerts(bestPoly);

            var pt = Utils.RandomPointInConvexPoly(verts);

            Status status = m_nav.GetPolyHeight(bestPolyRef, pt, out float h);
            if (status.HasFlag(Status.DT_FAILURE))
            {
                return status;
            }
            pt.Y = h;

            randomPt = pt;
            randomRef = bestPolyRef;

            return Status.DT_SUCCESS;
        }

        /// <summary>
        /// Gets whether the local boundary is valid
        /// </summary>
        /// <param name="boundary">Local boundary</param>
        /// <param name="filter">Query filter</param>
        public bool IsValid(LocalBoundary boundary, IGraphQueryFilter filter)
        {
            if (boundary.PolyCount <= 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            for (int i = 0; i < boundary.PolyCount; ++i)
            {
                if (!IsValidPolyRef(boundary.GetPolygonReference(i), filter))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Returns true if the polygon reference is valid and passes the filter restrictions.
        /// </summary>
        /// <param name="r">The polygon reference to check.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>Returns true if the polygon reference is valid and passes the filter restrictions.</returns>
        public bool IsValidPolyRef(int r, IGraphQueryFilter filter)
        {
            var cur = m_nav.GetTileAndPolyByRef(r);
            if (cur.Ref == 0)
            {
                // If cannot get polygon, assume it does not exists and boundary is invalid.
                return false;
            }
            if (!filter.PassFilter(cur.Poly.Flags))
            {
                // If cannot pass filter, assume flags has changed and boundary is invalid.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Moves from the start to the end position constrained to the navigation mesh.
        /// </summary>
        /// <param name="startRef">The reference id of the start polygon.</param>
        /// <param name="startPos">A position of the mover within the start polygon.</param>
        /// <param name="endPos">The desired end position of the mover.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="resultPos">The result position of the mover.</param>
        /// <param name="visited">The reference ids of the polygons visited during the move.</param>
        /// <param name="visitedCount">The number of polygons visited during the move.</param>
        /// <param name="maxVisitedSize">The maximum number of polygons the visited array can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status MoveAlongSurface(int startRef, Vector3 startPos, Vector3 endPos, IGraphQueryFilter filter, int maxVisitedSize, out Vector3 resultPos, out SimplePath visited)
        {
            return MoveAlongSurfaceHelper.MoveAlongSurface(startRef, startPos, endPos, filter, maxVisitedSize, out resultPos, out visited);
        }

        /// <summary>
        /// Returns random location on navmesh within the reach of specified location.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="maxRadius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location.</param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// The location is not exactly constrained by the circle, but it limits the visited polygons.
        /// </remarks>
        public Status FindRandomPointAroundCircle(int startRef, Vector3 centerPos, float maxRadius, IGraphQueryFilter filter, out int randomRef, out Vector3 randomPt)
        {
            return RandomPointAroundCircleHelper.FindRandomPointAroundCircle(startRef, centerPos, maxRadius, filter, out randomRef, out randomPt);
        }

        /// <summary>
        /// Finds the distance from the specified position to the nearest polygon wall.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon containing centerPos.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="maxRadius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="hitDist">The distance to the nearest wall from centerPos.</param>
        /// <param name="hitPos">The nearest position on the wall that was hit.</param>
        /// <param name="hitNormal">The normalized ray formed from the wall point to the source point.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindDistanceToWall(int startRef, Vector3 centerPos, float maxRadius, IGraphQueryFilter filter, out float hitDist, out Vector3 hitPos, out Vector3 hitNormal)
        {
            return WallHelper.FindDistanceToWall(startRef, centerPos, maxRadius, filter, out hitDist, out hitPos, out hitNormal);
        }
        /// <summary>
        /// Returns the segments for the specified polygon, optionally including portals.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="segmentVerts">The segments.</param>
        /// <param name="segmentRefs">The reference ids of each segment's neighbor polygon. Or zero if the segment is a wall.</param>
        /// <param name="segmentCount">The number of segments returned.</param>
        /// <param name="maxSegments">The maximum number of segments the result arrays can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status GetPolyWallSegments(int startRef, IGraphQueryFilter filter, int maxSegments, out Segment[] segmentsRes)
        {
            return WallHelper.GetPolyWallSegments(startRef, filter, maxSegments, out segmentsRes);
        }

        /// <summary>
        /// Calcs a path
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="polyPickExt">Extensions</param>
        /// <param name="mode">Path mode</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>Returns the status of the path calculation</returns>
        public Status CalcPath(IGraphQueryFilter filter, Vector3 polyPickExt, PathFindingMode mode, Vector3 startPos, Vector3 endPos, out IEnumerable<Vector3> resultPath)
        {
            resultPath = null;

            FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out _);
            FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out _);

            var endPointsDefined = startRef != 0 && endRef != 0;
            if (!endPointsDefined)
            {
                return Status.DT_FAILURE;
            }

            PathPoint start = new() { Ref = startRef, Pos = startPos };
            PathPoint end = new() { Ref = endRef, Pos = endPos };

            if (mode == PathFindingMode.Follow)
            {
                if (CalcPathFollow(filter, start, end, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Straight)
            {
                if (CalcPathStraigh(filter, start, end, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Sliced)
            {
                var status = InitSlicedFindPath(filter, start, end);
                if (status != Status.DT_SUCCESS)
                {
                    return status;
                }

                return UpdateSlicedFindPath(20, out _);
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Calculates the result path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="resultPath">Resulting path</param>
        private bool CalcPathFollow(IGraphQueryFilter filter, PathPoint start, PathPoint end, out List<Vector3> resultPath)
        {
            resultPath = null;

            FindPath(start, end, filter, MAX_POLYS, out var iterPath);
            if (iterPath.Count <= 0)
            {
                return false;
            }

            // Iterate over the path to find smooth path on the detail mesh surface.
            m_nav.ClosestPointOnPoly(start.Ref, start.Pos, out var iterPos, out _);
            m_nav.ClosestPointOnPoly(iterPath.End, end.Pos, out var targetPos, out _);

            List<Vector3> smoothPath = [iterPos];

            // Move towards target a small advancement at a time until target reached or
            // when ran out of memory to store the path.
            while (iterPath.Count != 0 && smoothPath.Count < MAX_SMOOTH)
            {
                if (IterPathFollow(filter, targetPos, smoothPath, iterPath, ref iterPos))
                {
                    //End reached
                    break;
                }
            }

            resultPath = smoothPath;

            return smoothPath.Count > 0;
        }
        /// <summary>
        /// Smooths the path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="smoothPath">Smooth path</param>
        /// <param name="iterPath">Path to iterate</param>
        /// <param name="iterPos">Current iteration position</param>
        private bool IterPathFollow(IGraphQueryFilter filter, Vector3 targetPos, List<Vector3> smoothPath, SimplePath iterPath, ref Vector3 iterPos)
        {
            float SLOP = 0.01f;

            // Find location to steer towards.
            if (!GetSteerTarget(iterPos, targetPos, SLOP, iterPath, out var target))
            {
                return true;
            }

            bool endOfPath = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_END) != 0;
            bool offMeshConnection = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0;

            // Find movement delta.
            Vector3 moveTgt = FindMovementDelta(iterPos, target.Position, endOfPath || offMeshConnection);

            // Move
            MoveAlongSurface(
                iterPath.Start, iterPos, moveTgt, filter, 16,
                out var result, out var visited);

            SimplePath.FixupCorridor(iterPath, visited);
            SimplePath.FixupShortcuts(iterPath, this);

            m_nav.GetPolyHeight(iterPath.Start, result, out float h);
            result.Y = h;
            iterPos = result;

            bool inRange = Utils.InRange(iterPos, target.Position, SLOP, 1.0f);
            if (!inRange)
            {
                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Handle end of path and off-mesh links when close enough.
            if (endOfPath)
            {
                // Reached end of path.
                iterPos = targetPos;

                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return true;
            }

            if (offMeshConnection)
            {
                // Reached off-mesh connection.
                HandleOffMeshConnection(target, smoothPath, iterPath, ref iterPos);

                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Store results.
            if (smoothPath.Count < MAX_SMOOTH)
            {
                smoothPath.Add(iterPos);
            }

            return false;
        }
        /// <summary>
        /// Finds movement delta
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="overMesh">Over mesh flag</param>
        private static Vector3 FindMovementDelta(Vector3 position, Vector3 targetPos, bool overMesh)
        {
            const float STEP_SIZE = 0.5f;

            // Find movement delta.
            var delta = Vector3.Subtract(targetPos, position);
            float len = delta.Length();

            // If the steer target is end of path or off-mesh link, do not move past the location.
            if (overMesh && len < STEP_SIZE)
            {
                len = 1;
            }
            else
            {
                len = STEP_SIZE / len;
            }

            return Vector3.Add(position, delta * len);
        }
        /// <summary>
        /// Handle off-mesh connection
        /// </summary>
        /// <param name="target">Target position</param>
        /// <param name="smoothPath">Smooth path</param>
        /// <param name="iterPath">Path to iterate</param>
        /// <param name="iterPos">Current iteration position</param>
        private void HandleOffMeshConnection(SteerTarget target, List<Vector3> smoothPath, SimplePath iterPath, ref Vector3 iterPos)
        {
            // Advance the path up to and over the off-mesh connection.
            int prevRef = 0;
            int polyRef = iterPath.Start;
            int npos = 0;
            var iterNodes = iterPath.GetPath();
            while (npos < iterPath.Count && polyRef != target.Ref)
            {
                prevRef = polyRef;
                polyRef = iterNodes[npos];
                npos++;
            }
            iterPath.Prune(npos);

            // Handle the connection.
            if (m_nav.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, out var sPos, out var ePos))
            {
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(sPos);
                }

                // Move position at the other side of the off-mesh link.
                iterPos = ePos;
                m_nav.GetPolyHeight(iterPath.Start, iterPos, out float eh);
                iterPos.Y = eh;
            }
        }
        /// <summary>
        /// Calculates straigh path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="resultPath">Resulting path</param>
        private bool CalcPathStraigh(IGraphQueryFilter filter, PathPoint start, PathPoint end, out Vector3[] resultPath)
        {
            FindPath(start, end, filter, MAX_POLYS, out var polys);
            if (polys.Count < 0)
            {
                resultPath = [];

                return false;
            }

            // In case of partial path, make sure the end point is clamped to the last polygon.
            var epos = end.Pos;
            if (polys.End != end.Ref)
            {
                m_nav.ClosestPointOnPoly(polys.End, end.Pos, out epos, out _);
            }

            FindStraightPath(start.Pos, epos, polys, MAX_POLYS, StraightPathOptions.AllCrossings, out var straightPath);

            resultPath = straightPath.GetPath();

            return straightPath.Count > 0;
        }
        /// <summary>
        /// Gets a steer target
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="minTargetDist">Miminum tangent distance</param>
        /// <param name="path">Current path</param>
        /// <param name="target">Out target</param>
        private bool GetSteerTarget(Vector3 startPos, Vector3 endPos, float minTargetDist, SimplePath path, out SteerTarget target)
        {
            target = new SteerTarget
            {
                Position = Vector3.Zero,
                Flag = 0,
                Ref = 0,
                Points = null,
                PointCount = 0
            };

            // Find steer target.
            int MAX_STEER_POINTS = 3;
            FindStraightPath(startPos, endPos, path, MAX_STEER_POINTS, StraightPathOptions.None, out var steerPath);

            if (steerPath.Count == 0)
            {
                return false;
            }

            target.PointCount = steerPath.Count;
            target.Points = steerPath.GetPath();

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < steerPath.Count)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPath.GetFlag(ns) & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !Utils.InRange(steerPath.GetPathPosition(ns), startPos, minTargetDist, 1000.0f))
                {
                    break;
                }
                ns++;
            }
            // Failed to find good point to steer to.
            if (ns >= steerPath.Count)
            {
                return false;
            }

            var pos = steerPath.GetPathPosition(ns);
            pos.Y = startPos.Y;

            target.Position = pos;
            target.Flag = steerPath.GetFlag(ns);
            target.Ref = steerPath.GetRef(ns);

            return true;
        }
    }
}
