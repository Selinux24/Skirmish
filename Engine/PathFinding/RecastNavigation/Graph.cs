using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class Graph : IGraph, ISerializable, IDisposable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        public static Graph Build(Triangle[] triangles, BuildSettings settings)
        {
            Graph res = new Graph()
            {
                maxNodes = settings.MaxNodes,
            };

            InputGeometry geom = new InputGeometry(triangles);

            foreach (var agent in settings.Agents)
            {
                var nm = NavMesh.Build(geom, settings, agent);
                var mmQuery = new NavMeshQuery();
                mmQuery.Init(nm, settings.MaxNodes);

                res.navMeshQDictionary.Add(agent, mmQuery);
            }

            return res;
        }
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
                MAX_STEER_POINTS, StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS);

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
                    !PolyUtils.InRange(steerPath[ns], startPos, minTargetDist, 1000.0f))
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
        /// <param name="path"></param>
        /// <param name="npath"></param>
        /// <param name="navQuery"></param>
        /// <returns></returns>
        private static int FixupShortcuts(int[] path, int npath, NavMeshQuery navQuery)
        {
            if (npath < 3)
            {
                return npath;
            }

            // Get connected polygons
            int maxNeis = 16;
            int[] neis = new int[maxNeis];
            int nneis = 0;

            if (navQuery.GetAttachedNavMesh().GetTileAndPolyByRef(path[0], out MeshTile tile, out Poly poly))
            {
                return npath;
            }

            for (int k = poly.firstLink; k != Constants.DT_NULL_LINK; k = tile.links[k].next)
            {
                Link link = tile.links[k];
                if (link.nref != 0)
                {
                    if (nneis < maxNeis)
                    {
                        neis[nneis++] = link.nref;
                    }
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < nneis; j++)
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

        private int maxNodes;
        Dictionary<AgentType, NavMeshQuery> navMeshQDictionary = new Dictionary<AgentType, NavMeshQuery>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Graph()
        {

        }
        protected Graph(SerializationInfo info, StreamingContext context)
        {
            int count = info.GetInt32("count");
            int maxNodes = info.GetInt32("maxNodes");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var agent = info.GetValue<Agent>("agent");
                    var nm = info.GetValue<NavMesh>("navmesh");
                    var mmQuery = new NavMeshQuery();
                    mmQuery.Init(nm, maxNodes);

                    navMeshQDictionary.Add(agent, mmQuery);
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", navMeshQDictionary.Keys.Count);
            info.AddValue("maxNodes", maxNodes);
            foreach (var agent in navMeshQDictionary.Keys)
            {
                info.AddValue("agent", agent);
                info.AddValue("navmesh", navMeshQDictionary[agent].GetAttachedNavMesh());
            }
        }

        public void Dispose()
        {
            Helper.Dispose(navMeshQDictionary);
        }

        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var filter = new QueryFilter()
            {
                m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
            };

            return CalcPath(
                navMeshQDictionary[agent],
                filter, new Vector3(2, 4, 2), PathFindingMode.TOOLMODE_PATHFIND_FOLLOW,
                from, to);
        }
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            var filter = new QueryFilter()
            {
                m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
            };

            var status = navMeshQDictionary[agent].FindNearestPoly(
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
        public IGraphNode[] GetNodes(AgentType agent)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            nodes.AddRange(GraphNode.Build(navMeshQDictionary[agent].GetAttachedNavMesh()));

            return nodes.ToArray();
        }
        public void Load(string fileName)
        {
            byte[] buffer = File.ReadAllBytes(fileName);

            var file = buffer.Decompress<Graph>();

            navMeshQDictionary = file.navMeshQDictionary;
        }
        public void Save(string fileName)
        {
            File.WriteAllBytes(fileName, this.Compress());
        }

        private Vector3[] CalcPath(
            NavMeshQuery m_navQuery, QueryFilter m_filter, Vector3 m_polyPickExt,
            PathFindingMode m_toolMode,
            Vector3 m_spos, Vector3 m_epos)
        {
            m_navQuery.FindNearestPoly(m_spos, m_polyPickExt, m_filter, out int m_startRef, out Vector3 nsp);

            m_navQuery.FindNearestPoly(m_epos, m_polyPickExt, m_filter, out int m_endRef, out Vector3 nep);

            Status m_pathFindStatus = Status.DT_FAILURE;

            if (m_toolMode == PathFindingMode.TOOLMODE_PATHFIND_FOLLOW)
            {
                Vector3[] m_smoothPath = null;
                int m_nsmoothPath = 0;
                int[] m_polys = null;
                int m_npolys = 0;

                if (m_startRef != 0 && m_endRef != 0)
                {
                    m_navQuery.FindPath(
                        m_startRef, m_endRef, m_spos, m_epos, m_filter,
                        out m_polys, out m_npolys, MAX_POLYS);

                    m_smoothPath = new Vector3[MAX_SMOOTH];
                    m_nsmoothPath = 0;

                    if (m_npolys != 0)
                    {
                        // Iterate over the path to find smooth path on the detail mesh surface.
                        int[] polys = new int[MAX_POLYS];
                        Array.Copy(m_polys, polys, m_npolys);
                        int npolys = m_npolys;

                        m_navQuery.ClosestPointOnPoly(m_startRef, m_spos, out Vector3 iterPos, out bool iOver);
                        m_navQuery.ClosestPointOnPoly(polys[npolys - 1], m_epos, out Vector3 targetPos, out bool eOver);

                        float STEP_SIZE = 0.5f;
                        float SLOP = 0.01f;

                        m_nsmoothPath = 0;

                        m_smoothPath[m_nsmoothPath] = iterPos;
                        m_nsmoothPath++;

                        // Move towards target a small advancement at a time until target reached or
                        // when ran out of memory to store the path.
                        while (npolys != 0 && m_nsmoothPath < MAX_SMOOTH)
                        {
                            // Find location to steer towards.
                            if (!GetSteerTarget(
                                m_navQuery, iterPos, targetPos, SLOP,
                                polys, npolys, out Vector3 steerPos, out StraightPathFlags steerPosFlag, out int steerPosRef,
                                out Vector3[] points, out int nPoints))
                            {
                                break;
                            }

                            bool endOfPath = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_END) != 0 ? true : false;
                            bool offMeshConnection = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;

                            // Find movement delta.
                            Vector3 delta = Vector3.Subtract(steerPos, iterPos);
                            float len = Vector3.Distance(delta, delta);
                            // If the steer target is end of path or off-mesh link, do not move past the location.
                            if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                            {
                                len = 1;
                            }
                            else
                            {
                                len = STEP_SIZE / len;
                            }
                            Vector3 moveTgt = Vector3.Add(iterPos, delta) * len;

                            // Move
                            m_navQuery.MoveAlongSurface(
                                polys[0], iterPos, moveTgt, m_filter,
                                out Vector3 result, out int[] visited, out int nvisited, 16);

                            npolys = FixupCorridor(polys, npolys, MAX_POLYS, visited, nvisited);
                            npolys = FixupShortcuts(polys, npolys, m_navQuery);

                            m_navQuery.GetPolyHeight(polys[0], result, out float h);
                            result.Y = h;
                            iterPos = result;

                            // Handle end of path and off-mesh links when close enough.
                            if (endOfPath && PolyUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached end of path.
                                iterPos = targetPos;
                                if (m_nsmoothPath < MAX_SMOOTH)
                                {
                                    m_smoothPath[m_nsmoothPath] = iterPos;
                                    m_nsmoothPath++;
                                }
                                break;
                            }
                            else if (offMeshConnection && PolyUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached off-mesh connection.

                                // Advance the path up to and over the off-mesh connection.
                                int prevRef = 0;
                                int polyRef = polys[0];
                                int npos = 0;
                                while (npos < npolys && polyRef != steerPosRef)
                                {
                                    prevRef = polyRef;
                                    polyRef = polys[npos];
                                    npos++;
                                }
                                for (int i = npos; i < npolys; ++i)
                                {
                                    polys[i - npos] = polys[i];
                                }
                                npolys -= npos;

                                // Handle the connection.
                                if (m_navQuery.GetAttachedNavMesh().GetOffMeshConnectionPolyEndPoints(
                                    prevRef, polyRef, out Vector3 startPos, out Vector3 endPos))
                                {
                                    if (m_nsmoothPath < MAX_SMOOTH)
                                    {
                                        m_smoothPath[m_nsmoothPath] = startPos;
                                        m_nsmoothPath++;
                                        // Hack to make the dotted path not visible during off-mesh connection.
                                        if ((m_nsmoothPath & 1) != 0)
                                        {
                                            m_smoothPath[m_nsmoothPath] = startPos;
                                            m_nsmoothPath++;
                                        }
                                    }
                                    // Move position at the other side of the off-mesh link.
                                    iterPos = endPos;
                                    m_navQuery.GetPolyHeight(polys[0], iterPos, out float eh);
                                    iterPos.Y = eh;
                                }
                            }

                            // Store results.
                            if (m_nsmoothPath < MAX_SMOOTH)
                            {
                                m_smoothPath[m_nsmoothPath] = iterPos;
                                m_nsmoothPath++;
                            }
                        }
                    }
                }
                else
                {
                    m_npolys = 0;
                    m_nsmoothPath = 0;
                }

                if(m_nsmoothPath > 0)
                {
                    Vector3[] res = new Vector3[m_nsmoothPath];
                    Array.Copy(m_smoothPath, res, m_nsmoothPath);
                    return res;
                }
            }
            else if (m_toolMode == PathFindingMode.TOOLMODE_PATHFIND_STRAIGHT)
            {
                Vector3[] m_straightPath = null;
                StraightPathFlags[] m_straightPathFlags = null;
                int[] m_straightPathPolys = null;
                int m_nstraightPath = 0;
                StraightPathOptions m_straightPathOptions = StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS;
                int[] m_polys = null;
                int m_npolys = 0;

                if (m_startRef != 0 && m_endRef != 0)
                {
                    m_navQuery.FindPath(
                        m_startRef, m_endRef, m_spos, m_epos, m_filter,
                        out m_polys, out m_npolys, MAX_POLYS);

                    m_nstraightPath = 0;
                    if (m_npolys != 0)
                    {
                        // In case of partial path, make sure the end point is clamped to the last polygon.
                        Vector3 epos = m_epos;
                        if (m_polys[m_npolys - 1] != m_endRef)
                        {
                            m_navQuery.ClosestPointOnPoly(m_polys[m_npolys - 1], m_epos, out epos, out bool eOver);
                        }

                        m_navQuery.FindStraightPath(
                            m_spos, epos, m_polys, m_npolys,
                            out m_straightPath, out m_straightPathFlags,
                            out m_straightPathPolys, out m_nstraightPath, MAX_POLYS, m_straightPathOptions);
                    }
                }
                else
                {
                    m_npolys = 0;
                    m_nstraightPath = 0;
                }

                if (m_nstraightPath > 0)
                {
                    Vector3[] res = new Vector3[m_nstraightPath];
                    Array.Copy(m_straightPath, res, m_nstraightPath);
                    return res;
                }
            }
            else if (m_toolMode == PathFindingMode.TOOLMODE_PATHFIND_SLICED)
            {
                if (m_startRef != 0 && m_endRef != 0)
                {
                    m_pathFindStatus = m_navQuery.InitSlicedFindPath(
                        m_startRef, m_endRef, m_spos, m_epos, m_filter,
                        FindPathOptions.DT_FINDPATH_ANY_ANGLE);
                }
            }

            return null;
        }
    }
}
