using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using Engine.Geometry;
    using Engine.PathFinding;

    /// <summary>
    /// Navigation Mesh
    /// </summary>
    public class NavMesh : IGraph
    {
        /// <summary>
        /// Vertex partition info
        /// </summary>
        class PartitionVertex
        {
            public bool IsActive;
            public bool IsConvex;
            public bool IsEar;

            public Vector3 Point;
            public float Angle;
            public PartitionVertex Previous;
            public PartitionVertex Next;
        }

        public TiledNavMesh TiledNavigationMesh = null;

        public NavMeshQuery Query = null;

        public NavMeshNode[] Nodes = null;

        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavMesh Build(VertexData[] vertices, uint[] indices)
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

            return Build(triangles);
        }
        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="triangles">List of triangles</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavMesh Build(Triangle[] triangles)
        {
            BoundingBox bbox = BoundingBox.FromPoints(triangles[0].GetCorners());
            Array.ForEach(triangles, tri => bbox = BoundingBox.Merge(bbox, BoundingBox.FromPoints(tri.GetCorners())));

            float diagonal = Vector2.Distance(new Vector2(bbox.Maximum.X, bbox.Maximum.Z), new Vector2(bbox.Minimum.X, bbox.Minimum.Z));
            float cellSize = diagonal / 512f;
            float cellHeight = cellSize * 0.66f;
            int walkableHeight = 1;
            int walkableClimb = 1;
            var fh = new Heightfield(bbox, cellSize, cellHeight);
            fh.RasterizeTriangles(triangles, Area.Default);
            fh.FilterLedgeSpans(walkableHeight * 10, walkableClimb * 2);
            fh.FilterLowHangingWalkableObstacles(walkableClimb * 2);
            fh.FilterWalkableLowHeightSpans(walkableHeight * 10);

            int radius = 1;
            int borderSize = 0;
            int minRegionArea = 16;
            int mergeRegionArea = 40;
            var ch = new CompactHeightfield(fh, walkableHeight, walkableClimb);
            ch.Erode(radius);
            ch.BuildDistanceField();
            ch.BuildRegions(borderSize, minRegionArea, mergeRegionArea);

            float maxError = 1.8f;
            int maxEdgeLength = 24;
            var cs = ch.BuildContourSet(maxError, maxEdgeLength, ContourBuildFlags.None);

            int vertsPerPoly = 6;
            var pm = new PolyMesh(cs, cellSize, cellHeight, borderSize, vertsPerPoly);

            int sampleDist = 6;
            int sampleMaxError = 1;
            var pmd = new PolyMeshDetail(pm, ch, sampleDist, sampleMaxError);

            float maxClimb = 0.9f;
            bool buildBoundingVolumeTree = true;
            float agentHeight = 2f;
            float agentRadius = 0.6f;
            var nm = NavMesh.Build(pm, pmd, null, cellSize, cellHeight, vertsPerPoly, maxClimb, buildBoundingVolumeTree, agentHeight, agentRadius);

            return nm;
        }

        public static NavMesh Build(
            PolyMesh polyMesh,
            PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb, bool buildBoundingVolumeTree, float agentHeight, float agentRadius)
        {
            var res = new NavMesh();

            var builder = new NavMeshBuilder(
                polyMesh,
                polyMeshDetail,
                offMeshCons,
                cellSize,
                cellHeight,
                vertsPerPoly,
                maxClimb,
                buildBoundingVolumeTree,
                agentHeight,
                agentRadius);

            var nm = new NavMesh();
            nm.TiledNavigationMesh = new TiledNavMesh(builder);
            nm.Query = new NavMeshQuery(nm.TiledNavigationMesh, 2048);
            nm.Nodes = new NavMeshNode[polyMeshDetail.MeshCount];

            for (int i = 0; i < polyMeshDetail.MeshCount; i++)
            {
                var mesh = polyMeshDetail.Meshes[i];

                nm.Nodes[i] = new NavMeshNode(nm, new Polygon(mesh.VertexCount));
                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    nm.Nodes[i].Poly.Points[v] = polyMeshDetail.Verts[mesh.VertexIndex + v];
                }
            }

            return nm;
        }

        public IGraphNode[] GetNodes()
        {
            var nodes = Array.ConvertAll(this.Nodes, (n) => { return (IGraphNode)n; });

            return nodes;
        }

        public PathFindingPath FindPath(Vector3 from, Vector3 to)
        {
            PathFindingPath res = null;

            NavPoint startPt = this.Query.FindNearestPoly(from, Vector3.Zero);
            NavPoint endPt = this.Query.FindNearestPoly(to, Vector3.Zero);
            List<PolyId> path = new List<PolyId>();
            if (this.Query.FindPath(ref startPt, ref endPt, path))
            {
                //find a smooth path over the mesh surface
                int npolys = path.Count;
                PolyId[] polys = path.ToArray();
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
                    PolyId steerPosRef = PolyId.Null;
                    if (!GetSteerTarget(this.Query, iterPos, targetPos, SLOP, polys, npolys, ref steerPos, ref steerPosFlag, ref steerPosRef))
                    {
                        break;
                    }

                    bool endOfPath = (steerPosFlag & PathfindingCommon.STRAIGHTPATH_END) != 0 ? true : false;
                    bool offMeshConnection = (steerPosFlag & PathfindingCommon.STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;

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
                    List<PolyId> visited = new List<PolyId>(16);
                    this.Query.MoveAlongSurface(new NavPoint(polys[0], iterPos), moveTgt, ref result, visited);
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
        
        private bool GetSteerTarget(
            NavMeshQuery navMeshQuery, 
            Vector3 startPos, 
            Vector3 endPos, 
            float minTargetDist, 
            PolyId[] path, 
            int pathSize,
            ref Vector3 steerPos, 
            ref int steerPosFlag, 
            ref PolyId steerPosRef)
        {
            int MAX_STEER_POINTS = 3;
            Vector3[] steerPath = new Vector3[MAX_STEER_POINTS];
            int[] steerPathFlags = new int[MAX_STEER_POINTS];
            PolyId[] steerPathPolys = new PolyId[MAX_STEER_POINTS];
            int nsteerPath = 0;
            navMeshQuery.FindStraightPath(startPos, endPos, path, pathSize,
                steerPath, steerPathFlags, steerPathPolys, ref nsteerPath, MAX_STEER_POINTS, 0);

            if (nsteerPath == 0)
                return false;

            //find vertex far enough to steer to
            int ns = 0;
            while (ns < nsteerPath)
            {
                if ((steerPathFlags[ns] & PathfindingCommon.STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath[ns], startPos, minTargetDist, 1000.0f))
                    break;

                ns++;
            }

            //failed to find good point to steer to
            if (ns >= nsteerPath)
                return false;

            steerPos = steerPath[ns];
            steerPos.Y = startPos.Y;
            steerPosFlag = steerPathFlags[ns];
            steerPosRef = steerPathPolys[ns];

            return true;
        }

        private bool InRange(Vector3 v1, Vector3 v2, float r, float h)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return (dx * dx + dz * dz) < (r * r) && Math.Abs(dy) < h;
        }

        private int FixupCorridor(PolyId[] path, int npath, int maxPath, List<PolyId> visited)
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
                    break;
            }

            //if no intersection found, return current path
            if (furthestPath == -1 || furthestVisited == -1)
                return npath;

            //concatenate paths
            //adjust beginning of the buffer to include the visited
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, npath);
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
                size = maxPath - req;
            for (int i = 0; i < size; i++)
                path[req + i] = path[orig + i];

            //store visited
            for (int i = 0; i < req; i++)
                path[i] = visited[(visited.Count - 1) - i];

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
