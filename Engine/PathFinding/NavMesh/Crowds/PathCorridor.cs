using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Path corridor
    /// </summary>
    class PathCorridor
    {
        private const int MaxIterations = 32;
        private const int MaxVisited = 16;
        private const float MinTargetDist = 0.01f;

        /// <summary>
        /// Current position
        /// </summary>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// Current target
        /// </summary>
        public Vector3 Target { get; private set; }
        /// <summary>
        /// Path
        /// </summary>
        public Path NavigationPath { get; private set; }
        /// <summary>
        /// Gets the first path polygon
        /// </summary>
        public PolyId FirstPoly
        {
            get
            {
                return (this.NavigationPath.Count != 0) ? this.NavigationPath[0] : PolyId.Null;
            }
        }
        /// <summary>
        /// Gets the last path polygon
        /// </summary>
        public PolyId LastPoly
        {
            get
            {
                return (this.NavigationPath.Count != 0) ? this.NavigationPath[this.NavigationPath.Count - 1] : PolyId.Null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PathCorridor()
        {
            this.NavigationPath = new Path();
        }

        /// <summary>
        /// The current corridor position is expected to be within the first polygon in the path.
        /// The target is expected to be in the last polygon.
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="path">The polygon path</param>
        public void SetCorridor(Vector3 target, Path path)
        {
            this.Target = target;
            this.NavigationPath = path;
        }
        /// <summary>
        /// Resets the path to the first polygon.
        /// </summary>
        /// <param name="reference">The starting polygon reference</param>
        /// <param name="position">Starting position</param>
        public void Reset(PolyId reference, Vector3 position)
        {
            this.Position = position;
            this.Target = position;
            this.NavigationPath.Clear();
            this.NavigationPath.Add(reference);
        }
        /// <summary>
        /// Move along the query and update the position
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="navQuery">The query</param>
        /// <returns>True if position changed, false if not</returns>
        public bool MovePosition(Vector3 newPosition, NavigationMeshQuery navQuery)
        {
            PathPoint startPoint = new PathPoint(this.NavigationPath[0], this.Position);
            Vector3 result = new Vector3();
            List<PolyId> visited = new List<PolyId>(MaxVisited);

            //move along navmesh and update new position
            bool status = navQuery.MoveAlongSurface(ref startPoint, ref newPosition, out result, visited);
            if (status == true)
            {
                this.MergeCorridorStartMoved(this.NavigationPath, visited);

                //adjust the position to stay on top of the navmesh
                float h = this.Position.Y;
                navQuery.GetPolyHeight(this.NavigationPath[0], result, ref h);
                result.Y = h;

                this.Position = result;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Move over offmesh connection
        /// </summary>
        /// <param name="offMeshConRef">Offmesh connection polygon reference</param>
        /// <param name="refs">Offmesh connection endpoints</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="navQuery">The query</param>
        /// <returns>True if position changed, false if not</returns>
        public bool MoveOverOffmeshConnection(PolyId offMeshConRef, PolyId[] refs, ref Vector3 startPos, ref Vector3 endPos, NavigationMeshQuery navQuery)
        {
            //advance the path up to and over the off-mesh connection
            PolyId prevRef = PolyId.Null;
            PolyId polyRef = this.NavigationPath[0];
            int npos = 0;
            while (npos < this.NavigationPath.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = this.NavigationPath[npos];
                npos++;
            }

            if (npos == this.NavigationPath.Count)
            {
                //could not find offMeshConRef
                return false;
            }

            //prune path
            this.NavigationPath.RemoveRange(0, npos);

            refs[0] = prevRef;
            refs[1] = polyRef;

            if (navQuery.GetOffMeshConnectionPolyEndPoints(refs[0], refs[1], ref startPos, ref endPos) == true)
            {
                this.Position = endPos;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Finds the corners in the corridor
        /// </summary>
        /// <param name="navQuery">Query</param>
        /// <returns>Returns the corners in the corridor</returns>
        public StraightPath FindCorners(NavigationMeshQuery navQuery)
        {
            StraightPath corners;
            navQuery.FindStraightPath(this.Position, this.Target, this.NavigationPath, 0, out corners);

            //prune points in the beginning of the path which are too close
            while (corners.Count > 0)
            {
                var firstCorner = corners[0];

                if (((firstCorner.Flags & StraightPathFlags.OffMeshConnection) != 0) ||
                    Helper.Distance2D(firstCorner.Point.Position, this.Position) > MinTargetDist)
                {
                    break;
                }

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
        /// Adjust the beginning of the path
        /// </summary>
        /// <param name="safeRef">The starting polygon reference</param>
        /// <param name="safePos">The starting position</param>
        /// <returns>True if path start changed, false if not</returns>
        public bool FixPathStart(PolyId safeRef, Vector3 safePos)
        {
            this.Position = safePos;

            if (this.NavigationPath.Count < 3 && this.NavigationPath.Count > 0)
            {
                PolyId lastPathId = this.NavigationPath[this.NavigationPath.Count - 1];

                this.NavigationPath.Clear();
                this.NavigationPath.Add(safeRef);
                this.NavigationPath.Add(PolyId.Null);
                this.NavigationPath.Add(lastPathId);
            }
            else
            {
                this.NavigationPath[0] = safeRef;
                this.NavigationPath[1] = PolyId.Null;
            }

            return true;
        }
        /// <summary>
        /// Use a local area path search to try to reoptimize this corridor
        /// </summary>
        /// <param name="navQuery">The query</param>
        /// <param name="filter">Query filter</param>
        /// <returns>True if optimized, false if not</returns>
        public bool OptimizePathTopology(NavigationMeshQuery navQuery, NavigationMeshQueryFilter filter)
        {
            if (this.NavigationPath.Count < 3)
            {
                return false;
            }

            Path res = new Path();
            int numRes = 0;
            int tempInt = 0;
            PathPoint startPoint = new PathPoint(this.NavigationPath[0], this.Position);
            PathPoint endPoint = new PathPoint(this.NavigationPath[this.NavigationPath.Count - 1], this.Target);
            navQuery.InitSlicedFindPath(ref startPoint, ref endPoint, filter, FindPathOptions.None);
            navQuery.UpdateSlicedFindPath(MaxIterations, ref tempInt);
            bool status = navQuery.FinalizedSlicedPathPartial(this.NavigationPath, res);

            if (status == true && numRes > 0)
            {
                MergeCorridorStartShortcut(this.NavigationPath, res);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Use an efficient local visibility search to try to optimize the corridor between the current position and the next.
        /// </summary>
        /// <param name="next">The next postion</param>
        /// <param name="pathOptimizationRange">The range</param>
        /// <param name="navQuery">The query</param>
        public void OptimizePathVisibility(Vector3 next, float pathOptimizationRange, NavigationMeshQuery navQuery)
        {
            //clamp the ray to max distance
            Vector3 goal = next;
            float dist = Helper.Distance2D(this.Position, goal);

            //if too close to the goal, do not try to optimize
            if (dist < 0.01f)
            {
                return;
            }

            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            //adjust ray length
            Vector3 delta = goal - this.Position;
            goal = this.Position + delta * (pathOptimizationRange / dist);

            PathPoint startPoint = new PathPoint(this.NavigationPath[0], this.Position);
            Path raycastPath = new Path();
            RaycastHit hit;
            navQuery.Raycast(ref startPoint, ref goal, RaycastOptions.None, out hit, raycastPath);
            if (raycastPath.Count > 1 && hit.T > 0.99f)
            {
                MergeCorridorStartShortcut(raycastPath, raycastPath);
            }
        }
        /// <summary>
        /// Merge two paths after the start is changed
        /// </summary>
        /// <param name="path">The current path</param>
        /// <param name="visited">The visited polygons</param>
        /// <returns>New path length</returns>
        private int MergeCorridorStartMoved(Path path, List<PolyId> visited)
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
                {
                    break;
                }
            }

            //if no intersection found just return current path
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return path.Count;
            }

            //concatenate paths

            //adjust beginning of buffer to include the visited
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            if (req + size > path.Count)
            {
                size = path.Count - req;
            }

            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    path[req + i] = path[orig + i];
                }
            }

            //store visited
            for (int i = 0; i < req; i++)
            {
                path[i] = visited[(visited.Count - 1) - i];
            }

            return req + size;
        }
        /// <summary>
        /// Merge two paths when a shorter path is found
        /// </summary>
        /// <param name="path">The current path</param>
        /// <param name="visited">The visited polygons</param>
        /// <returns>New path length</returns>
        private int MergeCorridorStartShortcut(Path path, Path visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            //find furthest common polygon
            for (int i = path.Count - 1; i >= 0; i--)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; j--)
                {
                    if (this.NavigationPath[i] == visited[j])
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

            //if no intersection found, return current path
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return path.Count;
            }

            //concatenate paths
            //adjust beginning of the buffer to include the visited
            int req = furthestVisited;
            if (req <= 0)
            {
                return path.Count;
            }

            int orig = furthestPath;
            int size = Math.Max(0, path.Count - orig);
            if (req + size > this.NavigationPath.Count)
            {
                size = this.NavigationPath.Count - req;
            }

            for (int i = 0; i < size; i++)
            {
                this.NavigationPath[req + i] = this.NavigationPath[orig + i];
            }

            //store visited
            for (int i = 0; i < req; i++)
            {
                this.NavigationPath[i] = visited[i];
            }

            return req + size;
        }

        /// <summary>
        /// Determines whether all the polygons in the path are valid
        /// </summary>
        /// <param name="maxLookAhead">The amount of polygons to examine</param>
        /// <param name="navQuery">The query</param>
        /// <returns>True if all valid, false if otherwise</returns>
        public bool IsValid(int maxLookAhead, NavigationMeshQuery navQuery)
        {
            int n = Math.Min(this.NavigationPath.Count, maxLookAhead);
            for (int i = 0; i < n; i++)
            {
                if (!navQuery.IsValidPolyRef(this.NavigationPath[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
