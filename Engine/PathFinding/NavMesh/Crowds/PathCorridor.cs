using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    class PathCorridor
    {
        private Vector3 pos;
        private Vector3 target;
        private Path path;

        public Vector3 Pos
        {
            get
            {
                return pos;
            }
        }
        public Vector3 Target
        {
            get
            {
                return target;
            }
        }
        public Path NavPath
        {
            get
            {
                return path;
            }
        }

        public PathCorridor()
        {
            this.path = new Path();
        }

        /// <summary>
        /// Resets the path to the first polygon.
        /// </summary>
        /// <param name="reference">The starting polygon reference</param>
        /// <param name="pos">Starting position</param>
        public void Reset(PolyId reference, Vector3 pos)
        {
            this.pos = pos;
            this.target = pos;
            path.Clear();
            path.Add(reference);
        }
        /// <summary>
        /// The current corridor position is expected to be within the first polygon in the path. The target
        /// is expected to be in the last polygon.
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="path">The polygon path</param>
        public void SetCorridor(Vector3 target, Path path)
        {
            this.target = target;
            this.path = path;
        }
        /// <summary>
        /// Move along the NavMeshQuery and update the position
        /// </summary>
        /// <param name="npos">Current position</param>
        /// <param name="navquery">The NavMeshQuery</param>
        /// <returns>True if position changed, false if not</returns>
        public bool MovePosition(Vector3 npos, NavigationMeshQuery navquery)
        {
            const int MaxVisited = 16;

            Vector3 result = new Vector3();
            List<PolyId> visited = new List<PolyId>(MaxVisited);
            PathPoint startPoint = new PathPoint(path[0], pos);


            //move along navmesh and update new position
            bool status = navquery.MoveAlongSurface(ref startPoint, ref npos, out result, visited);

            if (status == true)
            {
                MergeCorridorStartMoved(path, visited);

                //adjust the position to stay on top of the navmesh
                float h = pos.Y;
                navquery.GetPolyHeight(path[0], result, ref h);
                result.Y = h;
                pos = result;
                return true;
            }

            return false;
        }
        public StraightPath FindCorners(NavigationMeshQuery navquery)
        {
            const float MinTargetDist = 0.01f;

            StraightPath corners;
            navquery.FindStraightPath(pos, target, path, 0, out corners);

            //prune points in the beginning of the path which are too close
            while (corners.Count > 0)
            {
                if (((corners[0].Flags & StraightPathFlags.OffMeshConnection) != 0) ||
                    Helper.Distance2D(corners[0].Point.Position, pos) > MinTargetDist)
                    break;

                corners.RemoveAt(0);
            }

            //prune points after an off-mesh connection
            for (int i = 0; i < corners.Count; i++)
            {
                if ((corners[i].Flags & StraightPathFlags.OffMeshConnection) != 0)
                {
                    corners.RemoveRange(i + 1, corners.Count - i);
                    break;
                }
            }

            return corners;
        }
        /// <summary>
        /// Use a local area path search to try to reoptimize this corridor
        /// </summary>
        /// <param name="navquery">The NavMeshQuery</param>
        /// <returns>True if optimized, false if not</returns>
        public bool OptimizePathTopology(NavigationMeshQuery navquery, NavigationMeshQueryFilter filter)
        {
            if (path.Count < 3)
                return false;

            const int MaxIter = 32;
            const int MaxRes = 32;

            Path res = new Path();
            int numRes = 0;
            int tempInt = 0;
            PathPoint startPoint = new PathPoint(path[0], pos);
            PathPoint endPoint = new PathPoint(path[path.Count - 1], target);
            navquery.InitSlicedFindPath(ref startPoint, ref endPoint, filter, FindPathOptions.None);
            navquery.UpdateSlicedFindPath(MaxIter, ref tempInt);
            bool status = navquery.FinalizedSlicedPathPartial(path, res);

            if (status == true && numRes > 0)
            {
                MergeCorridorStartShortcut(path, res);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Use an efficient local visibility search to try to optimize the corridor between the current position and the next.
        /// </summary>
        /// <param name="next">The next postion</param>
        /// <param name="pathOptimizationRange">The range</param>
        /// <param name="navquery">The NavMeshQuery</param>
        public void OptimizePathVisibility(Vector3 next, float pathOptimizationRange, NavigationMeshQuery navquery)
        {
            //clamp the ray to max distance
            Vector3 goal = next;
            float dist = Helper.Distance2D(pos, goal);

            //if too close to the goal, do not try to optimize
            if (dist < 0.01f)
                return;

            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            //adjust ray length
            Vector3 delta = goal - pos;
            goal = pos + delta * (pathOptimizationRange / dist);

            PathPoint startPoint = new PathPoint(path[0], pos);
            Path raycastPath = new Path();
            RaycastHit hit;
            navquery.Raycast(ref startPoint, ref goal, RaycastOptions.None, out hit, raycastPath);
            if (raycastPath.Count > 1 && hit.T > 0.99f)
            {
                MergeCorridorStartShortcut(raycastPath, raycastPath);
            }
        }
        /// <summary>
        /// Merge two paths after the start is changed
        /// </summary>
        /// <param name="path">The current path</param>
        /// <param name="npath">Current path length</param>
        /// <param name="maxPath">Maximum path length allowed</param>
        /// <param name="visited">The visited polygons</param>
        /// <param name="nvisited">Visited path length</param>
        /// <returns>New path length</returns>
        public int MergeCorridorStartMoved(Path path, List<PolyId> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            //find furthest common polygon
            for (int i = path.Count - 1; i >= 0; i--)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; j--)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }

                if (found)
                    break;
            }

            //if no intersection found just return current path
            if (furthestPath == -1 || furthestVisited == -1)
                return path.Count;

            //concatenate paths

            //adjust beginning of buffer to include the visited
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            if (req + size > path.Count)
                size = path.Count - req;
            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                    path[req + i] = path[orig + i];
            }

            //store visited
            for (int i = 0; i < req; i++)
                path[i] = visited[(visited.Count - 1) - i];

            return req + size;
        }
        /// <summary>
        /// Merge two paths when a shorter path is found
        /// </summary>
        /// <param name="path">The current path</param>
        /// <param name="npath">Current path length</param>
        /// <param name="maxPath">Maximum path length allowed</param>
        /// <param name="visited">The visited polygons</param>
        /// <param name="nvisited">Visited path length</param>
        /// <returns>New path length</returns>
        public int MergeCorridorStartShortcut(Path corridorPath, Path visitedPath)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            //find furthest common polygon
            for (int i = corridorPath.Count - 1; i >= 0; i--)
            {
                bool found = false;
                for (int j = visitedPath.Count - 1; j >= 0; j--)
                {
                    if (path[i] == visitedPath[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }

                if (found)
                    break;
            }

            //if no intersection found, return current path
            if (furthestPath == -1 || furthestVisited == -1)
                return corridorPath.Count;

            //concatenate paths
            //adjust beginning of the buffer to include the visited
            int req = furthestVisited;
            if (req <= 0)
                return corridorPath.Count;

            int orig = furthestPath;
            int size = Math.Max(0, corridorPath.Count - orig);
            if (req + size > path.Count)
                size = path.Count - req;
            for (int i = 0; i < size; i++)
                path[req + i] = path[orig + i];

            //store visited
            for (int i = 0; i < req; i++)
                path[i] = visitedPath[i];

            return req + size;
        }
        public bool MoveOverOffmeshConnection(PolyId offMeshConRef, PolyId[] refs, ref Vector3 startPos, ref Vector3 endPos, NavigationMeshQuery navquery)
        {
            //advance the path up to and over the off-mesh connection
            PolyId prevRef = PolyId.Null;
            PolyId polyRef = path[0];
            int npos = 0;
            while (npos < path.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = path[npos];
                npos++;
            }

            if (npos == path.Count)
            {
                //could not find offMeshConRef
                return false;
            }

            //prune path
            path.RemoveRange(0, npos);

            refs[0] = prevRef;
            refs[1] = polyRef;

            if (navquery.GetOffMeshConnectionPolyEndPoints(refs[0], refs[1], ref startPos, ref endPos) == true)
            {
                pos = endPos;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Adjust the beginning of the path
        /// </summary>
        /// <param name="safeRef">The starting polygon reference</param>
        /// <param name="safePos">The starting position</param>
        /// <returns>True if path start changed, false if not</returns>
        public bool FixPathStart(PolyId safeRef, Vector3 safePos)
        {
            this.pos = safePos;
            if (path.Count < 3 && path.Count > 0)
            {
                PolyId lastPathId = path[path.Count - 1];

                path.Clear();
                path.Add(safeRef);
                path.Add(PolyId.Null);
                path.Add(lastPathId);
            }
            else
            {
                path[0] = safeRef;
                path[1] = PolyId.Null;
            }

            return true;
        }
        /// <summary>
        /// Determines whether all the polygons in the path are valid
        /// </summary>
        /// <param name="maxLookAhead">The amount of polygons to examine</param>
        /// <param name="navquery">The NavMeshQuery</param>
        /// <returns>True if all valid, false if otherwise</returns>
        public bool IsValid(int maxLookAhead, NavigationMeshQuery navquery)
        {
            int n = Math.Min(path.Count, maxLookAhead);
            for (int i = 0; i < n; i++)
            {
                if (!navquery.IsValidPolyRef(path[i]))
                    return false;
            }

            return true;
        }
        public PolyId GetFirstPoly()
        {
            return (path.Count != 0) ? path[0] : PolyId.Null;
        }
        public PolyId GetLastPoly()
        {
            return (path.Count != 0) ? path[path.Count - 1] : PolyId.Null;
        }
    }
}
