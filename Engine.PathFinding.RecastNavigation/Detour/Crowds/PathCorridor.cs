using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class PathCorridor
    {
        private Vector3 m_pos;
        private Vector3 m_target;
        private SimplePath m_path = null;
        private int m_maxPath = 0;

        /// <summary>
        /// Allocates the corridor's path buffer.
        /// </summary>
        /// <param name="maxPath">The maximum path size the corridor can handle.</param>
        /// <returns>True if the initialization succeeded.</returns>
        public bool Init(int maxPath)
        {
            m_path = new SimplePath(maxPath);
            m_maxPath = maxPath;
            return true;
        }
        /// <summary>
        /// Resets the path corridor to the specified position.
        /// </summary>
        /// <param name="r">The polygon reference containing the position.</param>
        /// <param name="pos">The new position in the corridor. [(x, y, z)]</param>
        public void Reset(int r, Vector3 pos)
        {
            m_pos = pos;
            m_target = pos;
            m_path.StartPath(r);
        }
        /// <summary>
        /// Finds the corners in the corridor from the position toward the target. (The straightened path.)
        /// </summary>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        /// <param name="maxCorners">The maximum number of corners the buffers can hold.</param>
        /// <param name="cornerPolys">The corner list.</param>
        public void FindCorners(
            NavMeshQuery navquery, QueryFilter filter, int maxCorners,
            out StraightPath cornerPolys)
        {
            float MIN_TARGET_DIST = 0.01f;

            navquery.FindStraightPath(
                m_pos, m_target, m_path, maxCorners, StraightPathOptions.DT_STRAIGHTPATH_NONE,
                out cornerPolys);

            // Prune points in the beginning of the path which are too close.
            while (cornerPolys.Count > 0)
            {
                if (cornerPolys.Flags[0].HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
                    Vector2.DistanceSquared(cornerPolys.Path[0].XZ(), m_pos.XZ()) > (MIN_TARGET_DIST * MIN_TARGET_DIST))
                {
                    break;
                }

                cornerPolys.RemoveFirst();
            }

            // Prune points after an off-mesh connection.
            for (int i = 0; i < cornerPolys.Count; ++i)
            {
                if (cornerPolys.Flags[i].HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION))
                {
                    cornerPolys.Count = i + 1;
                    break;
                }
            }
        }
        /// <summary>
        /// Attempts to optimize the path if the specified point is visible from the current position.
        /// </summary>
        /// <param name="next">The point to search toward. [(x, y, z])</param>
        /// <param name="pathOptimizationRange">The maximum range to search. [Limit: > 0]</param>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        public void OptimizePathVisibility(
            Vector3 next, float pathOptimizationRange,
            NavMeshQuery navquery, QueryFilter filter)
        {
            // Clamp the ray to max distance.
            Vector3 goal = next;
            float dist = Vector2.Distance(m_pos.XZ(), goal.XZ());

            // If too close to the goal, do not try to optimize.
            if (dist < 0.01f)
            {
                return;
            }

            // Overshoot a little. This helps to optimize open fields in tiled meshes.
            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            // Adjust ray length.
            Vector3 delta = goal - m_pos;
            goal = m_pos + delta * pathOptimizationRange / dist;

            int MAX_RES = 32;
            navquery.Raycast(
                m_path.Start, m_pos, goal, filter, MAX_RES,
                out var t, out var norm, out var res);
            if (res.Count > 1 && t > 0.99f)
            {
                SimplePath.MergeCorridorStartShortcut(m_path, m_maxPath, res);
            }
        }
        /// <summary>
        /// Attempts to optimize the path using a local area search. (Partial replanning.) 
        /// </summary>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        /// <returns></returns>
        public bool OptimizePathTopology(NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count < 3)
            {
                return false;
            }

            int MAX_ITER = 32;
            int MAX_RES = 32;

            navquery.InitSlicedFindPath(m_path.Start, m_path.End, m_pos, m_target, filter, FindPathOptions.DT_FINDPATH_ANY_ANGLE);
            navquery.UpdateSlicedFindPath(MAX_ITER, out int doneIters);
            Status status = navquery.FinalizeSlicedFindPathPartial(MAX_RES, m_path.GetPath(), m_path.Count, out var res);

            if (status == Status.DT_SUCCESS && res.Count > 0)
            {
                SimplePath.MergeCorridorStartShortcut(m_path, m_maxPath, res);
                return true;
            }

            return false;
        }

        public bool MoveOverOffmeshConnection(
            NavMeshQuery navquery, int offMeshConRef, int[] refs,
            out Vector3 startPos, out Vector3 endPos)
        {
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;

            // Advance the path up to and over the off-mesh connection.
            int prevRef = 0;
            int polyRef = m_path.Start;
            int npos = 0;
            int[] path = m_path.GetPath();
            while (npos < m_path.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = path[npos];
                npos++;
            }
            if (npos == m_path.Count)
            {
                // Could not find offMeshConRef
                return false;
            }

            // Prune path
            m_path.Prune(npos);

            refs[0] = prevRef;
            refs[1] = polyRef;

            NavMesh nav = navquery.GetAttachedNavMesh();

            bool status = nav.GetOffMeshConnectionPolyEndPoints(refs[0], refs[1], out startPos, out endPos);
            if (status)
            {
                m_pos = endPos;
                return true;
            }

            return false;
        }

        public bool FixPathStart(int safeRef, Vector3 safePos)
        {
            m_path.FixStart(safeRef);
            m_pos = safePos;

            return true;
        }

        public bool TrimInvalidPath(int safeRef, Vector3 safePos, NavMeshQuery navquery, QueryFilter filter)
        {
            // Keep valid path as far as possible.
            int n = 0;
            int[] path = m_path.GetPath();
            while (n < m_path.Count && navquery.IsValidPolyRef(path[n], filter))
            {
                n++;
            }

            if (n == m_path.Count)
            {
                // All valid, no need to fix.
                return true;
            }
            else if (n == 0)
            {
                // The first polyref is bad, use current safe values.
                m_pos = safePos;
                m_path.StartPath(safeRef);
            }
            else
            {
                // The path is partially usable.
                m_path.SetLength(n);
            }

            // Clamp target pos to last poly
            navquery.ClosestPointOnPolyBoundary(m_path.End, m_target, out m_target);

            return true;
        }
        /// <summary>
        /// Checks the current corridor path to see if its polygon references remain valid.
        /// </summary>
        /// <param name="maxLookAhead">The number of polygons from the beginning of the corridor to search.</param>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        /// <returns></returns>
        public bool IsValid(int maxLookAhead, NavMeshQuery navquery, QueryFilter filter)
        {
            // Check that all polygons still pass query filter.
            int n = Math.Min(m_path.Count, maxLookAhead);
            int[] path = m_path.GetPath();
            for (int i = 0; i < n; ++i)
            {
                if (!navquery.IsValidPolyRef(path[i], filter))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Moves the position from the current location to the desired location, adjusting the corridor as needed to reflect the change.
        /// </summary>
        /// <param name="npos">The desired new position. [(x, y, z)]</param>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        /// <returns>Returns true if move succeeded.</returns>
        public bool MovePosition(Vector3 npos, NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count <= 0)
            {
                return false;
            }

            // Move along navmesh and update new position.
            int MAX_VISITED = 16;
            Status status = navquery.MoveAlongSurface(
                m_path.Start, m_pos, npos, filter, MAX_VISITED,
                out var result, out var visited);

            if (status == Status.DT_SUCCESS)
            {
                SimplePath.MergeCorridorStartMoved(m_path, m_maxPath, visited);

                // Adjust the position to stay on top of the navmesh.
                navquery.GetPolyHeight(m_path.Start, result, out float h);
                result.Y = h;
                m_pos = result;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Moves the target from the curent location to the desired location, adjusting the corridor as needed to reflect the change.
        /// </summary>
        /// <param name="npos">The desired new target position. [(x, y, z)]</param>
        /// <param name="navquery">The query object used to build the corridor.</param>
        /// <param name="filter">The filter to apply to the operation.</param>
        /// <returns>Returns true if move succeeded.</returns>
        public bool MoveTargetPosition(Vector3 npos, NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count <= 0)
            {
                return false;
            }

            // Move along navmesh and update new position.
            int MAX_VISITED = 16;
            Status status = navquery.MoveAlongSurface(
                m_path.End, m_target, npos, filter, MAX_VISITED,
                out var result, out var visited);
            if (status == Status.DT_SUCCESS)
            {
                SimplePath.MergeCorridorEndMoved(m_path, m_maxPath, visited);

                m_target = result;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Loads a new path and target into the corridor.
        /// </summary>
        /// <param name="target">The target location within the last polygon of the path. [(x, y, z)]</param>
        /// <param name="path">The path corridor. [(polyRef) * @p npolys]</param>
        public void SetCorridor(Vector3 target, SimplePath path)
        {
            m_target = target;

            int[] p = path?.GetPath() ?? new int[m_maxPath];
            int count = path?.Count ?? 0;
            m_path.StartPath(p, count);
        }
        /// <summary>
        /// Gets the current position within the corridor. (In the first polygon.)
        /// </summary>
        /// <returns>The current position within the corridor.</returns>
        public Vector3 GetPos()
        {
            return m_pos;
        }
        /// <summary>
        /// Gets the current target within the corridor. (In the last polygon.)
        /// </summary>
        /// <returns>The current target within the corridor.</returns>
        public Vector3 GetTarget()
        {
            return m_target;
        }
        /// <summary>
        /// The polygon reference id of the first polygon in the corridor, the polygon containing the position.
        /// </summary>
        /// <returns>The polygon reference id of the first polygon in the corridor. (Or zero if there is no path.)</returns>
        public int GetFirstPoly()
        {
            return m_path.Start;
        }
        /// <summary>
        /// The polygon reference id of the last polygon in the corridor, the polygon containing the target.
        /// </summary>
        /// <returns>The polygon reference id of the last polygon in the corridor. (Or zero if there is no path.)</returns>
        public int GetLastPoly()
        {
            return m_path.End;
        }
        /// <summary>
        /// The corridor's path.
        /// </summary>
        /// <returns>The corridor's path. [(polyRef) * #getPathCount()]</returns>
        public int[] GetPath()
        {
            return m_path.GetPath();
        }
        /// <summary>
        /// The number of polygons in the current corridor path.
        /// </summary>
        /// <returns>The number of polygons in the current corridor path.</returns>
        public int GetPathCount()
        {
            return m_path.Count;
        }
    }
}
