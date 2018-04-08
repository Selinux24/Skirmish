using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation graph
    /// </summary>
    [Serializable]
    public class Graph : IGraph, ISerializable, IDisposable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        /// <summary>
        /// Builds a graph from a triangle array
        /// </summary>
        /// <param name="triangles">Triangle array</param>
        /// <param name="settings">Build settings</param>
        /// <returns>Returns the new navigation graph</returns>
        public static Graph Build(Triangle[] triangles, BuildSettings settings)
        {
            Graph res = new Graph
            {
                settings = settings,
            };

            var geom = new InputGeometry(triangles);

            foreach (var agent in settings.Agents)
            {
                var nm = NavMesh.Build(geom, settings, agent);
                var mmQuery = new NavMeshQuery();
                mmQuery.Init(nm, settings.MaxNodes);

                res.navMeshQDictionary.Add(agent, mmQuery);
            }

            return res;
        }

        /// <summary>
        /// Calcs a path
        /// </summary>
        /// <param name="navQuery">Navigation query</param>
        /// <param name="filter">Filter</param>
        /// <param name="polyPickExt">Extensions</param>
        /// <param name="mode">Path mode</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>Returns the status of the path calculation</returns>
        private static Status CalcPath(
            NavMeshQuery navQuery, QueryFilter filter, Vector3 polyPickExt,
            PathFindingMode mode,
            Vector3 startPos, Vector3 endPos, out Vector3[] resultPath)
        {
            resultPath = null;

            navQuery.FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out Vector3 nsp);

            navQuery.FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out Vector3 nep);

            Status pathFindStatus = Status.DT_FAILURE;

            if (mode == PathFindingMode.TOOLMODE_PATHFIND_FOLLOW)
            {
                List<Vector3> smoothPath = new List<Vector3>();

                if (startRef != 0 && endRef != 0)
                {
                    navQuery.FindPath(
                        startRef, endRef, startPos, endPos, filter,
                        out int[] polys, out int npolys, MAX_POLYS);

                    if (npolys != 0)
                    {
                        // Iterate over the path to find smooth path on the detail mesh surface.
                        int[] iterPolys = new int[MAX_POLYS];
                        Array.Copy(polys, iterPolys, npolys);
                        int nIterPolys = npolys;

                        navQuery.ClosestPointOnPoly(startRef, startPos, out Vector3 iterPos, out bool iOver);
                        navQuery.ClosestPointOnPoly(iterPolys[nIterPolys - 1], endPos, out Vector3 targetPos, out bool eOver);

                        float STEP_SIZE = 0.5f;
                        float SLOP = 0.01f;

                        smoothPath.Add(iterPos);

                        // Move towards target a small advancement at a time until target reached or
                        // when ran out of memory to store the path.
                        while (nIterPolys != 0 && smoothPath.Count < MAX_SMOOTH)
                        {
                            // Find location to steer towards.
                            if (!GetSteerTarget(
                                navQuery, iterPos, targetPos, SLOP,
                                iterPolys, nIterPolys, out Vector3 steerPos, out StraightPathFlags steerPosFlag, out int steerPosRef,
                                out Vector3[] points, out int nPoints))
                            {
                                break;
                            }

                            bool endOfPath = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_END) != 0 ? true : false;
                            bool offMeshConnection = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;

                            // Find movement delta.
                            Vector3 delta = Vector3.Subtract(steerPos, iterPos);
                            float len = delta.Length();
                            // If the steer target is end of path or off-mesh link, do not move past the location.
                            if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                            {
                                len = 1;
                            }
                            else
                            {
                                len = STEP_SIZE / len;
                            }
                            Vector3 moveTgt = Vector3.Add(iterPos, delta * len);

                            // Move
                            navQuery.MoveAlongSurface(
                                iterPolys[0], iterPos, moveTgt, filter,
                                out Vector3 result, out int[] visited, out int nvisited, 16);

                            nIterPolys = FixupCorridor(iterPolys, nIterPolys, MAX_POLYS, visited, nvisited);
                            nIterPolys = FixupShortcuts(iterPolys, nIterPolys, navQuery);

                            navQuery.GetPolyHeight(iterPolys[0], result, out float h);
                            result.Y = h;
                            iterPos = result;

                            // Handle end of path and off-mesh links when close enough.
                            if (endOfPath && InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached end of path.
                                iterPos = targetPos;
                                if (smoothPath.Count < MAX_SMOOTH)
                                {
                                    smoothPath.Add(iterPos);
                                }
                                break;
                            }
                            else if (offMeshConnection && InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached off-mesh connection.

                                // Advance the path up to and over the off-mesh connection.
                                int prevRef = 0;
                                int polyRef = iterPolys[0];
                                int npos = 0;
                                while (npos < nIterPolys && polyRef != steerPosRef)
                                {
                                    prevRef = polyRef;
                                    polyRef = iterPolys[npos];
                                    npos++;
                                }
                                for (int i = npos; i < nIterPolys; ++i)
                                {
                                    iterPolys[i - npos] = iterPolys[i];
                                }
                                nIterPolys -= npos;

                                // Handle the connection.
                                if (navQuery.GetAttachedNavMesh().GetOffMeshConnectionPolyEndPoints(
                                    prevRef, polyRef, out Vector3 sPos, out Vector3 ePos))
                                {
                                    if (smoothPath.Count < MAX_SMOOTH)
                                    {
                                        smoothPath.Add(sPos);
                                        // Hack to make the dotted path not visible during off-mesh connection.
                                        if ((smoothPath.Count & 1) != 0)
                                        {
                                            smoothPath.Add(sPos);
                                        }
                                    }
                                    // Move position at the other side of the off-mesh link.
                                    iterPos = ePos;
                                    navQuery.GetPolyHeight(iterPolys[0], iterPos, out float eh);
                                    iterPos.Y = eh;
                                }
                            }

                            // Store results.
                            if (smoothPath.Count < MAX_SMOOTH)
                            {
                                smoothPath.Add(iterPos);
                            }
                        }
                    }
                }

                if (smoothPath.Count > 0)
                {
                    resultPath = smoothPath.ToArray();

                    return pathFindStatus;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_STRAIGHT)
            {
                Vector3[] straightPath = null;
                StraightPathFlags[] straightPathFlags = null;
                int[] straightPathPolys = null;
                int nstraightPath = 0;

                StraightPathOptions m_straightPathOptions = StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS;

                if (startRef != 0 && endRef != 0)
                {
                    navQuery.FindPath(
                        startRef, endRef, startPos, endPos, filter,
                        out int[] polys, out int npolys, MAX_POLYS);

                    nstraightPath = 0;
                    if (npolys != 0)
                    {
                        // In case of partial path, make sure the end point is clamped to the last polygon.
                        Vector3 epos = endPos;
                        if (polys[npolys - 1] != endRef)
                        {
                            navQuery.ClosestPointOnPoly(polys[npolys - 1], endPos, out epos, out bool eOver);
                        }

                        navQuery.FindStraightPath(
                            startPos, epos, polys, npolys,
                            out straightPath, out straightPathFlags,
                            out straightPathPolys, out nstraightPath, MAX_POLYS, m_straightPathOptions);
                    }
                }

                if (nstraightPath > 0)
                {
                    resultPath = new Vector3[nstraightPath];
                    Array.Copy(straightPath, resultPath, nstraightPath);

                    return pathFindStatus;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_SLICED)
            {
                if (startRef != 0 && endRef != 0)
                {
                    pathFindStatus = navQuery.InitSlicedFindPath(
                        startRef, endRef, startPos, endPos, filter,
                        FindPathOptions.DT_FINDPATH_ANY_ANGLE);

                    return pathFindStatus;
                }
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Gets a steer target
        /// </summary>
        /// <param name="navQuery">Navigation query</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="minTargetDist">Miminum tangent distance</param>
        /// <param name="path">Current path</param>
        /// <param name="pathSize">Current path size</param>
        /// <param name="steerPos">Returns the steer position</param>
        /// <param name="steerPosFlag">Returns the steer position flag</param>
        /// <param name="steerPosRef">Returns the steer position reference</param>
        /// <param name="outPoints">Returns the point list</param>
        /// <param name="outPointCount">Returns the point count</param>
        /// <returns></returns>
        private static bool GetSteerTarget(
            NavMeshQuery navQuery, Vector3 startPos, Vector3 endPos,
            float minTargetDist,
            int[] path, int pathSize,
            out Vector3 steerPos, out StraightPathFlags steerPosFlag, out int steerPosRef,
            out Vector3[] outPoints, out int outPointCount)
        {
            steerPos = Vector3.Zero;
            steerPosFlag = 0;
            steerPosRef = 0;
            outPoints = null;
            outPointCount = 0;

            // Find steer target.
            int MAX_STEER_POINTS = 3;
            navQuery.FindStraightPath(
                startPos, endPos, path, pathSize,
                out Vector3[] steerPath, out StraightPathFlags[] steerPathFlags, out int[] steerPathPolys, out int nsteerPath,
                MAX_STEER_POINTS, StraightPathOptions.DT_STRAIGHTPATH_NONE);

            if (nsteerPath == 0)
            {
                return false;
            }

            outPointCount = nsteerPath;
            outPoints = steerPath;

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < nsteerPath)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPathFlags[ns] & StraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath[ns], startPos, minTargetDist, 1000.0f))
                {
                    break;
                }
                ns++;
            }
            // Failed to find good point to steer to.
            if (ns >= nsteerPath)
            {
                return false;
            }

            steerPos = steerPath[ns];
            steerPos.Y = startPos.Y;
            steerPosFlag = steerPathFlags[ns];
            steerPosRef = steerPathPolys[ns];

            return true;
        }
        /// <summary>
        /// Fix ups corridor
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="maxPath">Maximum path size</param>
        /// <param name="visited">Visted references</param>
        /// <param name="nvisited">Number of visited references</param>
        /// <returns>Returns the new size of the path</returns>
        private static int FixupCorridor(int[] path, int npath, int maxPath, int[] visited, int nvisited)
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
            if (size != 0)
            {
                Array.Copy(path, orig, path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path[i] = visited[(nvisited - 1) - i];
            }

            return req + size;
        }
        /// <summary>
        /// This function checks if the path has a small U-turn, that is,
        /// a polygon further in the path is adjacent to the first polygon
        /// in the path. If that happens, a shortcut is taken.
        /// This can happen if the target (T) location is at tile boundary,
        /// and we're (S) approaching it parallel to the tile edge.
        /// The choice at the vertex can be arbitrary, 
        ///  +---+---+
        ///  |:::|:::|
        ///  +-S-+-T-+
        ///  |:::|   | -- the step can end up in here, resulting U-turn path.
        ///  +---+---+
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="navQuery">Navigation query</param>
        /// <returns>Returns the new size of the path</returns>
        private static int FixupShortcuts(int[] path, int npath, NavMeshQuery navQuery)
        {
            if (npath < 3)
            {
                return npath;
            }

            // Get connected polygons
            int maxNeis = 16;
            List<int> neis = new List<int>();

            if (navQuery.GetAttachedNavMesh().GetTileAndPolyByRef(path[0], out MeshTile tile, out Poly poly))
            {
                return npath;
            }

            for (int k = poly.firstLink; k != Detour.DT_NULL_LINK; k = tile.links[k].next)
            {
                var link = tile.links[k];
                if (link.nref != 0)
                {
                    if (neis.Count < maxNeis)
                    {
                        neis.Add(link.nref);
                    }
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < neis.Count; j++)
                {
                    if (path[i] == neis[j])
                    {
                        cut = i;
                        break;
                    }
                }
            }
            if (cut > 1)
            {
                int offset = cut - 1;
                npath -= offset;
                for (int i = 1; i < npath; i++)
                {
                    path[i] = path[i + offset];
                }
            }

            return npath;
        }

        private static bool InRange(Vector3 v1, Vector3 v2, float r, float h)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;

            return (dx * dx + dz * dz) < r * r && Math.Abs(dy) < h;
        }

        /// <summary>
        /// Navigation mesh query dictionary (by agent type)
        /// </summary>
        private Dictionary<Agent, NavMeshQuery> navMeshQDictionary = new Dictionary<Agent, NavMeshQuery>();
        /// <summary>
        /// Build settings
        /// </summary>
        private BuildSettings settings = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Graph()
        {

        }
        /// <summary>
        /// Instance dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(navMeshQDictionary);
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected Graph(SerializationInfo info, StreamingContext context)
        {
            settings = info.GetValue<BuildSettings>("settings");

            int count = info.GetInt32("count");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var agent = info.GetValue<Agent>(string.Format("agent.{0}", i));
                    var nm = info.GetValue<NavMesh>(string.Format("navmesh.{0}", i));
                    var mmQuery = new NavMeshQuery();
                    mmQuery.Init(nm, settings.MaxNodes);

                    navMeshQDictionary.Add(agent, mmQuery);
                }
            }
        }
        /// <summary>
        /// Gets the object data for serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("settings", settings);

            info.AddValue("count", navMeshQDictionary.Keys.Count);
            int index = 0;
            foreach (var agent in navMeshQDictionary.Keys)
            {
                info.AddValue(string.Format("agent.{0}", index), agent);
                info.AddValue(string.Format("navmesh.{0}", index), navMeshQDictionary[agent].GetAttachedNavMesh());
                index++;
            }
        }

        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            nodes.AddRange(GraphNode.Build(navMeshQDictionary[(Agent)agent].GetAttachedNavMesh()));

            return nodes.ToArray();
        }
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var filter = new QueryFilter()
            {
                m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
            };

            CalcPath(
                navMeshQDictionary[(Agent)agent],
                filter, new Vector3(2, 4, 2), PathFindingMode.TOOLMODE_PATHFIND_FOLLOW,
                from, to, out Vector3[] result);

            return result;
        }
        /// <summary>
        /// Gets wether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            var filter = new QueryFilter()
            {
                m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
            };

            var status = navMeshQDictionary[(Agent)agent].FindNearestPoly(
                position, new Vector3(2, 4, 2), filter,
                out int nRef, out Vector3 nPoint);

            if (!status.HasFlag(Status.DT_FAILURE))
            {
                nearest = nPoint;

                return nPoint.X == position.X && nPoint.Z == position.Z;
            }
            else
            {
                nearest = null;

                return false;
            }
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Load(string fileName)
        {
            byte[] buffer = File.ReadAllBytes(fileName);

            var file = buffer.Decompress<Graph>();

            navMeshQDictionary = file.navMeshQDictionary;
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Save(string fileName)
        {
            File.WriteAllBytes(fileName, this.Compress());
        }

        /// <summary>
        /// Builds the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="geom">Input geometry</param>
        public void BuildTile(Vector3 position, InputGeometry geom)
        {
            foreach (var agent in navMeshQDictionary.Keys)
            {
                navMeshQDictionary[agent].GetAttachedNavMesh().BuildTile(position, geom, settings, agent);
            }
        }
        /// <summary>
        /// Removes the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="geom">Input geometry</param>
        public void RemoveTile(Vector3 position, InputGeometry geom)
        {
            foreach (var agent in navMeshQDictionary.Keys)
            {
                navMeshQDictionary[agent].GetAttachedNavMesh().RemoveTile(position, geom, settings);
            }
        }
    }
}
