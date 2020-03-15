using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class PathCorridor
    {
        private static int MergeCorridorStartMoved(
            int[] path, int npath, int maxPath,
            int[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = npath - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return npath;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = nvisited - furthestVisited;
            int orig = Math.Min(furthestPath + 1, npath);
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size > 0)
            {
                Array.ConstrainedCopy(path, orig, path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path[i] = visited[(nvisited - 1) - i];
            }

            return req + size;
        }
        private static int MergeCorridorEndMoved(
            int[] path, int npath, int maxPath,
            int[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = 0; i < npath; ++i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return npath;
            }

            // Concatenate paths.
            int ppos = furthestPath + 1;
            int vpos = furthestVisited + 1;
            int count = Math.Min(nvisited - vpos, maxPath - ppos);
            if (count > 0)
            {
                Array.ConstrainedCopy(path, ppos, visited, vpos, count);
            }

            return ppos + count;
        }
        private static int MergeCorridorStartShortcut(
            int[] path, int npath, int maxPath,
            int[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = npath - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return npath;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = furthestVisited;
            if (req <= 0)
            {
                return npath;
            }

            int orig = furthestPath;
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size > 0)
            {
                Array.ConstrainedCopy(path, orig, path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path[i] = visited[i];
            }

            return req + size;
        }

        private Vector3 m_pos;
        private Vector3 m_target;
        private SimplePath m_path = null;
        private int m_maxPath = 0;

        /// Allocates the corridor's path buffer. 
        ///  @param[in]		maxPath		The maximum path size the corridor can handle.
        /// @return True if the initialization succeeded.
        public bool Init(int maxPath)
        {
            m_path = new SimplePath(maxPath);
            m_maxPath = maxPath;
            return true;
        }
        /// Resets the path corridor to the specified position.
        ///  @param[in]		ref		The polygon reference containing the position.
        ///  @param[in]		pos		The new position in the corridor. [(x, y, z)]
        public void Reset(int r, Vector3 pos)
        {
            m_pos = pos;
            m_target = pos;
            m_path.Path[0] = r;
            m_path.Count = 1;
        }
        /// Finds the corners in the corridor from the position toward the target. (The straightened path.)
        ///  @param[out]	cornerVerts		The corner vertices. [(x, y, z) * cornerCount] [Size: <= maxCorners]
        ///  @param[out]	cornerFlags		The flag for each corner. [(flag) * cornerCount] [Size: <= maxCorners]
        ///  @param[out]	cornerPolys		The polygon reference for each corner. [(polyRef) * cornerCount] 
        ///  								[Size: <= @p maxCorners]
        ///  @param[in]		maxCorners		The maximum number of corners the buffers can hold.
        ///  @param[in]		navquery		The query object used to build the corridor.
        ///  @param[in]		filter			The filter to apply to the operation.
        /// @return The number of corners returned in the corner buffers. [0 <= value <= @p maxCorners]
        public int FindCorners(
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
                    break;
                cornerPolys.Count--;
                if (cornerPolys.Count > 0)
                {
                    Array.ConstrainedCopy(cornerPolys.Flags, 1, cornerPolys.Flags, 0, cornerPolys.Count);
                    Array.ConstrainedCopy(cornerPolys.Refs, 1, cornerPolys.Refs, 0, cornerPolys.Count);
                    Array.ConstrainedCopy(cornerPolys.Path, 1, cornerPolys.Path, 0, cornerPolys.Count);
                }
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

            return cornerPolys.Count;
        }
        /// Attempts to optimize the path if the specified point is visible from the current position.
        ///  @param[in]		next					The point to search toward. [(x, y, z])
        ///  @param[in]		pathOptimizationRange	The maximum range to search. [Limit: > 0]
        ///  @param[in]		navquery				The query object used to build the corridor.
        ///  @param[in]		filter					The filter to apply to the operation.			
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
                m_path.Path[0], m_pos, goal, filter, MAX_RES,
                out float t, out Vector3 norm, out int[] res, out int nres);
            if (nres > 1 && t > 0.99f)
            {
                m_path.Count = MergeCorridorStartShortcut(m_path.Path, m_path.Count, m_maxPath, res, nres);
            }
        }
        /// Attempts to optimize the path using a local area search. (Partial replanning.) 
        ///  @param[in]		navquery	The query object used to build the corridor.
        ///  @param[in]		filter		The filter to apply to the operation.	
        public bool OptimizePathTopology(NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count < 3)
            {
                return false;
            }

            int MAX_ITER = 32;
            int MAX_RES = 32;

            navquery.InitSlicedFindPath(m_path.Path[0], m_path.Path[m_path.Count - 1], m_pos, m_target, filter, FindPathOptions.DT_FINDPATH_ANY_ANGLE);
            navquery.UpdateSlicedFindPath(MAX_ITER, out int doneIters);
            Status status = navquery.FinalizeSlicedFindPathPartial(MAX_RES, m_path.Path, m_path.Count, out SimplePath res);

            if (status == Status.DT_SUCCESS && res.Count > 0)
            {
                m_path.Count = MergeCorridorStartShortcut(m_path.Path, m_path.Count, m_maxPath, res.Path, res.Count);
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
            int polyRef = m_path.Path[0];
            int npos = 0;
            while (npos < m_path.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = m_path.Path[npos];
                npos++;
            }
            if (npos == m_path.Count)
            {
                // Could not find offMeshConRef
                return false;
            }

            // Prune path
            for (int i = npos; i < m_path.Count; ++i)
            {
                m_path.Path[i - npos] = m_path.Path[i];
            }

            m_path.Count -= npos;

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
            m_pos = safePos;
            if (m_path.Count < 3 && m_path.Count > 0)
            {
                m_path.Path[2] = m_path.Path[m_path.Count - 1];
                m_path.Path[0] = safeRef;
                m_path.Path[1] = 0;
                m_path.Count = 3;
            }
            else
            {
                m_path.Path[0] = safeRef;
                m_path.Path[1] = 0;
            }

            return true;
        }

        public bool TrimInvalidPath(int safeRef, Vector3 safePos, NavMeshQuery navquery, QueryFilter filter)
        {
            // Keep valid path as far as possible.
            int n = 0;
            while (n < m_path.Count && navquery.IsValidPolyRef(m_path.Path[n], filter))
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
                m_path.Path[0] = safeRef;
                m_path.Count = 1;
            }
            else
            {
                // The path is partially usable.
                m_path.Count = n;
            }

            // Clamp target pos to last poly
            navquery.ClosestPointOnPolyBoundary(m_path.Path[m_path.Count - 1], m_target, out m_target);

            return true;
        }
        /// Checks the current corridor path to see if its polygon references remain valid. 
        ///  @param[in]		maxLookAhead	The number of polygons from the beginning of the corridor to search.
        ///  @param[in]		navquery		The query object used to build the corridor.
        ///  @param[in]		filter			The filter to apply to the operation.	
        public bool IsValid(int maxLookAhead, NavMeshQuery navquery, QueryFilter filter)
        {
            // Check that all polygons still pass query filter.
            int n = Math.Min(m_path.Count, maxLookAhead);
            for (int i = 0; i < n; ++i)
            {
                if (!navquery.IsValidPolyRef(m_path.Path[i], filter))
                {
                    return false;
                }
            }

            return true;
        }
        /// Moves the position from the current location to the desired location, adjusting the corridor 
        /// as needed to reflect the change.
        ///  @param[in]		npos		The desired new position. [(x, y, z)]
        ///  @param[in]		navquery	The query object used to build the corridor.
        ///  @param[in]		filter		The filter to apply to the operation.
        /// @return Returns true if move succeeded.
        public bool MovePosition(Vector3 npos, NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count <= 0)
            {
                return false;
            }

            // Move along navmesh and update new position.
            int MAX_VISITED = 16;
            Status status = navquery.MoveAlongSurface(
                m_path.Path[0], m_pos, npos, filter, MAX_VISITED,
                out Vector3 result, out int[] visited, out int nvisited);

            if (status == Status.DT_SUCCESS)
            {
                m_path.Count = MergeCorridorStartMoved(m_path.Path, m_path.Count, m_maxPath, visited, nvisited);

                // Adjust the position to stay on top of the navmesh.
                navquery.GetPolyHeight(m_path.Path[0], result, out float h);
                result.Y = h;
                m_pos = result;
                return true;
            }
            return false;
        }
        /// Moves the target from the curent location to the desired location, adjusting the corridor
        /// as needed to reflect the change. 
        ///  @param[in]		npos		The desired new target position. [(x, y, z)]
        ///  @param[in]		navquery	The query object used to build the corridor.
        ///  @param[in]		filter		The filter to apply to the operation.
        /// @return Returns true if move succeeded.
        public bool MoveTargetPosition(Vector3 npos, NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_path.Count <= 0)
            {
                return false;
            }

            // Move along navmesh and update new position.
            int MAX_VISITED = 16;
            Status status = navquery.MoveAlongSurface(
                m_path.Path[m_path.Count - 1], m_target, npos, filter, MAX_VISITED,
                out Vector3 result, out int[] visited, out int nvisited);
            if (status == Status.DT_SUCCESS)
            {
                m_path.Count = MergeCorridorEndMoved(m_path.Path, m_path.Count, m_maxPath, visited, nvisited);

                m_target = result;

                return true;
            }

            return false;
        }
        /// Loads a new path and target into the corridor.
        ///  @param[in]		target		The target location within the last polygon of the path. [(x, y, z)]
        ///  @param[in]		path		The path corridor. [(polyRef) * @p npolys]
        ///  @param[in]		npath		The number of polygons in the path.
        public void SetCorridor(Vector3 target, SimplePath path)
        {
            m_target = target;

            int[] p = path?.Path ?? new int[m_maxPath];
            int count = path?.Count ?? 0;
            m_path.AddRange(p, count);
        }
        /// Gets the current position within the corridor. (In the first polygon.)
        /// @return The current position within the corridor.
        public Vector3 GetPos()
        {
            return m_pos;
        }
        /// Gets the current target within the corridor. (In the last polygon.)
        /// @return The current target within the corridor.
        public Vector3 GetTarget()
        {
            return m_target;
        }
        /// The polygon reference id of the first polygon in the corridor, the polygon containing the position.
        /// @return The polygon reference id of the first polygon in the corridor. (Or zero if there is no path.)
        public int GetFirstPoly()
        {
            return m_path.Count > 0 ? m_path.Path[0] : 0;
        }
        /// The polygon reference id of the last polygon in the corridor, the polygon containing the target.
        /// @return The polygon reference id of the last polygon in the corridor. (Or zero if there is no path.)
        public int GetLastPoly()
        {
            return m_path.Count > 0 ? m_path.Path[m_path.Count - 1] : 0;
        }
        /// The corridor's path.
        /// @return The corridor's path. [(polyRef) * #getPathCount()]
        public int[] GetPath()
        {
            return m_path.Path.ToArray();
        }
        /// The number of polygons in the current corridor path.
        /// @return The number of polygons in the current corridor path.
        public int GetPathCount()
        {
            return m_path.Count;
        }
    }
}
