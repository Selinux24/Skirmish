using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Common;

    /// <summary>
    /// Navigation Mesh
    /// </summary>
    public class NavigationMesh : IGraph
    {
        /// <summary>
        /// 
        /// </summary>
        protected TiledNavMesh TiledNavigationMesh = null;
        /// <summary>
        /// 
        /// </summary>
        protected NavigationMeshQuery Query = null;
        /// <summary>
        /// 
        /// </summary>
        protected NavigationMeshNode[] Nodes = null;

        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavigationMesh Build(VertexData[] vertices, uint[] indices, NavigationMeshGenerationSettings settings)
        {
            int tris = indices.Length / 3;

            Triangle[] triangles = new Triangle[tris];

            int index = 0;
            for (int i = 0; i < tris; i++)
            {
                triangles[i] = new Triangle(
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value);
            }

            return Build(triangles, settings);
        }
        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="triangles">List of triangles</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavigationMesh Build(Triangle[] triangles, NavigationMeshGenerationSettings settings)
        {
            BoundingBox bbox = BoundingBox.FromPoints(triangles[0].GetCorners());
            Array.ForEach(triangles, tri => bbox = BoundingBox.Merge(bbox, BoundingBox.FromPoints(tri.GetCorners())));

            var fh = new Heightfield(bbox, settings.CellSize, settings.CellHeight);
            fh.RasterizeTriangles(triangles, Area.Default);
            fh.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            fh.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            fh.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            var ch = new CompactHeightfield(fh, settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            ch.Erode(settings.VoxelAgentRadius);
            ch.BuildDistanceField();
            ch.BuildRegions(0, settings.MinRegionSize, settings.MergedRegionSize);

            var cs = ch.BuildContourSet(settings.MaxEdgeError, settings.MaxEdgeLength, settings.ContourFlags);

            var pm = new PolyMesh(cs, settings.CellSize, settings.CellHeight, 0, settings.VertsPerPoly);

            var pmd = new PolyMeshDetail(pm, ch, settings.SampleDistance, settings.MaxSampleError);

            var builder = new NavigationMeshBuilder(
                pm,
                pmd,
                new OffMeshConnection[0],
                settings.CellSize,
                settings.CellHeight,
                settings.VertsPerPoly,
                settings.MaxClimb,
                settings.BuildBoundingVolumeTree,
                settings.AgentHeight,
                settings.AgentRadius);

            var nm = new NavigationMesh();
            nm.TiledNavigationMesh = new TiledNavMesh(builder);
            nm.Query = new NavigationMeshQuery(nm.TiledNavigationMesh, 2048);
            nm.Nodes = new NavigationMeshNode[pmd.MeshCount];

            for (int i = 0; i < pmd.MeshCount; i++)
            {
                var mesh = pmd.Meshes[i];

                nm.Nodes[i] = new NavigationMeshNode(nm, new Polygon(mesh.VertexCount));
                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    nm.Nodes[i].Poly.Points[v] = pmd.Verts[mesh.VertexIndex + v];
                }
            }

            return nm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IGraphNode[] GetNodes()
        {
            var nodes = Array.ConvertAll(this.Nodes, (n) => { return (IGraphNode)n; });

            return nodes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public PathFindingPath FindPath(Vector3 from, Vector3 to)
        {
            PathFindingPath res = null;

            PathPoint startPt = this.Query.FindNearestPoly(from, Vector3.Zero);
            PathPoint endPt = this.Query.FindNearestPoly(to, Vector3.Zero);
            List<int> path = new List<int>();
            if (this.Query.FindPath(ref startPt, ref endPt, path))
            {
                //find a smooth path over the mesh surface
                int npolys = path.Count;
                int[] polys = path.ToArray();
                Vector3 iterPos = new Vector3();
                Vector3 targetPos = new Vector3();
                this.Query.ClosestPointOnPoly(startPt.Polygon, startPt.Position, ref iterPos);
                this.Query.ClosestPointOnPoly(polys[npolys - 1], endPt.Position, ref targetPos);

                var smoothPath = new List<Vector3>(2048);
                smoothPath.Add(iterPos);

                float STEP_SIZE = 0.5f;
                float SLOP = 0.01f;
                while (npolys > 0 && smoothPath.Count < smoothPath.Capacity)
                {
                    //find location to steer towards
                    Vector3 steerPos = new Vector3();
                    int steerPosFlag = 0;
                    int steerPosRef = 0;
                    if (!GetSteerTarget(this.Query, iterPos, targetPos, SLOP, polys, npolys, ref steerPos, ref steerPosFlag, ref steerPosRef))
                    {
                        break;
                    }

                    bool endOfPath = (steerPosFlag & NavigationMeshQuery.STRAIGHTPATH_END) != 0 ? true : false;
                    bool offMeshConnection = (steerPosFlag & NavigationMeshQuery.STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;

                    //find movement delta
                    Vector3 delta = steerPos - iterPos;
                    float len = (float)Math.Sqrt(Vector3.Dot(delta, delta));

                    //if steer target is at end of path or off-mesh link
                    //don't move past location
                    if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                    {
                        len = 1;
                    }
                    else
                    {
                        len = STEP_SIZE / len;
                    }

                    Vector3 moveTgt = new Vector3();
                    VMad(ref moveTgt, iterPos, delta, len);

                    //move
                    int MAX_POLYS = 256;
                    Vector3 result = new Vector3();
                    List<int> visited = new List<int>(16);
                    this.Query.MoveAlongSurface(new PathPoint(polys[0], iterPos), moveTgt, ref result, visited);
                    npolys = FixupCorridor(polys, npolys, MAX_POLYS, visited);
                    float h = 0;
                    this.Query.GetPolyHeight(polys[0], result, ref h);
                    result.Y = h;
                    iterPos = result;

                    //handle end of path when close enough
                    if (endOfPath && InRange(iterPos, steerPos, SLOP, 1.0f))
                    {
                        //reached end of path
                        iterPos = targetPos;
                        if (smoothPath.Count < smoothPath.Capacity)
                        {
                            smoothPath.Add(iterPos);
                        }
                        break;
                    }

                    //store results
                    if (smoothPath.Count < smoothPath.Capacity)
                    {
                        smoothPath.Add(iterPos);
                    }
                }

                res = new PathFindingPath(from, to, smoothPath.ToArray());
            }

            return res;
        }
        /// <summary>
        /// Scaled vector addition
        /// </summary>
        /// <param name="dest">Result</param>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <param name="s">Scalar</param>
        private void VMad(ref Vector3 dest, Vector3 v1, Vector3 v2, float s)
        {
            dest.X = v1.X + v2.X * s;
            dest.Y = v1.Y + v2.Y * s;
            dest.Z = v1.Z + v2.Z * s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="navMeshQuery"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="minTargetDist"></param>
        /// <param name="path"></param>
        /// <param name="pathSize"></param>
        /// <param name="steerPos"></param>
        /// <param name="steerPosFlag"></param>
        /// <param name="steerPosRef"></param>
        /// <returns></returns>
        private bool GetSteerTarget(
            NavigationMeshQuery navMeshQuery, 
            Vector3 startPos, 
            Vector3 endPos, 
            float minTargetDist,
            int[] path, 
            int pathSize,
            ref Vector3 steerPos, 
            ref int steerPosFlag,
            ref int steerPosRef)
        {
            int MAX_STEER_POINTS = 3;
            Vector3[] steerPath = new Vector3[MAX_STEER_POINTS];
            int[] steerPathFlags = new int[MAX_STEER_POINTS];
            int[] steerPathPolys = new int[MAX_STEER_POINTS];
            int nsteerPath = 0;
            navMeshQuery.FindStraightPath(
                startPos, endPos, 
                path, pathSize,
                steerPath, steerPathFlags, steerPathPolys, 
                ref nsteerPath, 
                MAX_STEER_POINTS, 0);

            if (nsteerPath == 0)
            {
                return false;
            }

            //find vertex far enough to steer to
            int ns = 0;
            while (ns < nsteerPath)
            {
                if ((steerPathFlags[ns] & NavigationMeshQuery.STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath[ns], startPos, minTargetDist, 1000.0f))
                {
                    break;
                }

                ns++;
            }

            //failed to find good point to steer to
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
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="r"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        private bool InRange(Vector3 v1, Vector3 v2, float r, float h)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return (dx * dx + dz * dz) < (r * r) && Math.Abs(dy) < h;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="npath"></param>
        /// <param name="maxPath"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private int FixupCorridor(int[] path, int npath, int maxPath, List<int> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            //find furhtest common polygon
            for (int i = npath - 1; i >= 0; i--)
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

            //if no intersection found, return current path
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return npath;
            }

            //concatenate paths
            //adjust beginning of the buffer to include the visited
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, npath);
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }

            for (int i = 0; i < size; i++)
            {
                path[req + i] = path[orig + i];
            }

            //store visited
            for (int i = 0; i < req; i++)
            {
                path[i] = visited[(visited.Count - 1) - i];
            }

            return req + size;
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return "NavMesh";
        }
    }
}
