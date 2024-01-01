using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a tile cache navigation mesh.
    /// </summary>
    public struct TileCachePolyMesh
    {
        /// <summary>
        /// Vertex bucket count
        /// </summary>
        const int VERTEX_BUCKET_COUNT = 1 << 8;
        /// <summary>
        /// Null index
        /// </summary>
        const int NULL_IDX = -1;
        /// <summary>
        /// Maximum number of edges to remove
        /// </summary>
        const int MAX_REM_EDGES = 48;

        /// <summary>
        /// Vertices of the mesh, 3 elements per vertex.
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Polygons of the mesh, nvp*2 elements per polygon.
        /// </summary>
        public IndexedPolygon[] Polys { get; set; }
        /// <summary>
        /// Number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; set; }
        /// <summary>
        /// Per polygon flags.
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// Area ID of polygons.
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }

        /// <summary>
        /// Builds a tile cache polygon mesh
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="nvp">Number of maximum vertex per polygon</param>
        public static TileCachePolyMesh Build(TileCacheContourSet cset, int nvp)
        {
            cset.GetGeometryConfiguration(out int maxVertices, out int maxPolys, out int maxVertsPerCont);

            var mesh = new TileCachePolyMesh
            {
                NPolys = 0,
              
                NVerts = 0,
                Verts = new Int3[maxVertices],
                Polys = new IndexedPolygon[maxPolys],
                Areas = Helper.CreateArray(maxPolys, SamplePolyAreas.None),
                Flags = Helper.CreateArray(maxPolys, SamplePolyFlagTypes.None),

                NVP = nvp,
            };

            bool[] vflags = new bool[maxVertices];
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, NULL_IDX);

            for (int i = 0; i < cset.NConts; ++i)
            {
                var cont = cset.Conts[i];

                // Skip null contours.
                if (cont.NVertices < 3)
                {
                    continue;
                }

                // Triangulate contour
                int ntris = cont.Triangulate(maxVertsPerCont, out var indices, out var tris);

                // Add and merge vertices.
                for (int j = 0; j < cont.NVertices; ++j)
                {
                    var v = cont.Vertices[j];
                    indices[j] = mesh.AddVertex(v.X, v.Y, v.Z, firstVert, nextVert);
                    if ((v.Flag & TileCacheContourSet.BORDER_VERTEX) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = true;
                    }
                }

                // Build initial polygons.
                IndexedPolygon.CreateInitialPolygons(indices, tris, ntris, maxVertsPerCont, out var polys, out var npolys);
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                var (mergedPolys, mergedNpolys) = mesh.MergePolygons(polys, npolys);

                // Store polygons.
                if (!mesh.StorePolygons(mergedPolys, mergedNpolys, cont, maxPolys))
                {
                    throw new EngineException($"rcBuildPolyMesh: Too many polygons {mesh.NPolys} (max:{maxPolys}).");
                }
            }

            // Remove edge vertices.
            mesh.RemoveEdgeVertices(vflags, maxPolys);

            // Calculate adjacency.
            mesh.BuildMeshAdjacency(cset);

            return mesh;
        }

        /// <summary>
        /// Adds a new vertex to the polygon mesh
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <param name="firstVert">First vertex</param>
        /// <param name="nextVert">Next vertex</param>
        /// <returns>Returns the added index</returns>
        private int AddVertex(int x, int y, int z, int[] firstVert, int[] nextVert)
        {
            int bucket = Utils.ComputeVertexHash(x, 0, z, VERTEX_BUCKET_COUNT - 1);
            int i = firstVert[bucket];

            while (i != NULL_IDX)
            {
                var v = Verts[i];
                if (v.X == x && v.Z == z && (Math.Abs(v.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = NVerts++;
            Verts[i] = new(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }

        /// <summary>
        /// Removes edge vertices
        /// </summary>
        /// <param name="vflags">Vertex flags</param>
        /// <param name="maxPolys">Maximum number of triangles</param>
        private void RemoveEdgeVertices(bool[] vflags, int maxPolys)
        {
            for (int i = 0; i < NVerts; ++i)
            {
                if (!vflags[i])
                {
                    continue;
                }

                if (!CanRemoveVertex(i))
                {
                    continue;
                }

                if (!RemoveVertex(i, maxPolys))
                {
                    // Failed to remove vertex
                    throw new EngineException($"Failed to remove edge vertex {i}.");
                }

                // Remove vertex
                // Note: mesh.nverts is already decremented inside removeVertex()!
                // Fixup vertex flags
                for (int j = i; j < NVerts; ++j)
                {
                    vflags[j] = vflags[j + 1];
                }
                --i;
            }
        }

        /// <summary>
        /// Gets whether the specified vertex can be removed
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <returns>Returns true if the vertex can be removed</returns>
        public readonly bool CanRemoveVertex(int rem)
        {
            // Count number of polygons to remove.
            var (numTouchedVerts, numRemainingEdges) = IndexedPolygon.CountVertexToRemove(Polys, NPolys, rem);

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
            {
                return false;
            }

            // Check that there is enough memory for the test.
            int maxEdges = numTouchedVerts * 2;
            if (maxEdges > MAX_REM_EDGES)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            int nedges = 0;
            IndexedEdge[] edges = new IndexedEdge[MAX_REM_EDGES];

            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] != rem && p[k] != rem)
                    {
                        continue;
                    }

                    // Arrange edge so that a=rem.
                    int a = p[j], b = p[k];
                    if (b == rem)
                    {
                        Helper.Swap(ref a, ref b);
                    }

                    // Check if the edge exists
                    bool exists = IndexedEdge.Exists(edges, nedges, b);

                    // Add new edge.
                    if (!exists)
                    {
                        edges[nedges++] = new(b, 1);
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].ShareCount < 2)
                {
                    numOpenEdges++;
                }
            }
            return numOpenEdges <= 2;
        }
      
        /// <summary>
        /// Removes the specified vertex
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <param name="maxTris">Maxmimum number of triangles</param>
        /// <returns>Returns true if the vertex were removed</returns>
        public bool RemoveVertex(int rem, int maxPolys)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = IndexedPolygon.CountPolygonsToRemove(Polys, NPolys, rem);
            numRemovedVerts = Math.Min(numRemovedVerts, MAX_REM_EDGES);

            if (!GenerateRemoveEdges(numRemovedVerts, rem, out var edgeList))
            {
                return false;
            }

            var edges = edgeList.Edges;
            var nedges = edges.Length;
            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            var hole = new int[numRemovedVerts * NVP];
            var harea = new SamplePolyAreas[numRemovedVerts * NVP];
            if (!GetTriangulateHole(edges, ref nedges, hole, harea, out var triList))
            {
                return false;
            }

            // Triangulate the hole.
            int ntris = TriangulationHelper.Triangulate(triList.TriVerts, ref triList.TriHole, out var tris);
            if (ntris < 0)
            {
                Logger.WriteWarning(this, "removeVertex: Hole triangulation error");

                ntris = -ntris;
            }
            if (ntris > MAX_REM_EDGES)
            {
                return false;
            }

            // Merge the hole triangles back to polygons.
            var (polys, pareas, npolys) = BuildRemoveInitialPolygons(tris, ntris, hole, harea);
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            var (mergedPolys, mergedNPolys, mergedAreas) = MergePolygons(polys, npolys, pareas);

            // Store polygons.
            if (!StorePolygons(mergedPolys, mergedNPolys, mergedAreas, maxPolys))
            {
                Logger.WriteWarning(this, $"removeVertex: Too many polygons {NPolys} (max:{maxPolys}).");

                return false;
            }

            return true;
        }
        /// <summary>
        /// Generates remove edges
        /// </summary>
        /// <param name="numRemovedVerts">Number of removed vertices</param>
        /// <param name="rem">Index to remove</param>
        /// <param name="edgeList">Returns the edge list</param>
        private bool GenerateRemoveEdges(int numRemovedVerts, int rem, out (IndexedRegionEdge[] Edges, int NEdges) edgeList)
        {
            var edges = new IndexedRegionEdge[numRemovedVerts];
            int nedges = 0;
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];

                bool hasRem = p.Contains(rem);
                if (!hasRem)
                {
                    continue;
                }

                // Collect edges which does not touch the removed vertex.
                int nv = p.CountPolyVerts();
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] == rem || p[k] == rem)
                    {
                        continue;
                    }

                    if (nedges >= numRemovedVerts)
                    {
                        edgeList = (Array.Empty<IndexedRegionEdge>(), 0);

                        return false;
                    }

                    edges[nedges++] = new(p[k], p[j], -1, Areas[i]);
                }

                // Remove the polygon.
                Polys[i] = Polys[NPolys - 1];
                Polys[NPolys - 1] = null;
                Areas[i] = Areas[NPolys - 1];
                NPolys--;
                --i;
            }

            // Remove vertex.
            for (int i = rem; i < NVerts - 1; ++i)
            {
                Verts[i] = Verts[i + 1];
            }
            NVerts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] > rem) p[j]--;
                }
            }
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].EdgeIndexA > rem) edges[i].EdgeIndexA--;
                if (edges[i].EdgeIndexB > rem) edges[i].EdgeIndexB--;
            }

            edgeList = (edges, nedges);

            return true;
        }
        /// <summary>
        /// Gets the triangulated hole
        /// </summary>
        /// <param name="edges">Edge list</param>
        /// <param name="nedges">Number of edges</param>
        /// <param name="hole">Hole indices</param>
        /// <param name="harea">Hole areas</param>
        /// <param name="tris">Return the triangulated vertices and the triangulated hole</param>
        private readonly bool GetTriangulateHole(IndexedRegionEdge[] edges, ref int nedges, int[] hole, SamplePolyAreas[] harea, out (Int3[] TriVerts, int[] TriHole) tris)
        {
            int nhole = 0;
            int nharea = 0;

            Utils.PushBack(edges[0].EdgeIndexA, hole, ref nhole);
            Utils.PushBack(edges[0].Area, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].EdgeIndexA;
                    int eb = edges[i].EdgeIndexB;
                    var a = edges[i].Area;

                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            tris = (Array.Empty<Int3>(), Array.Empty<int>());
                            return false;
                        }
                        Utils.PushFront(ea, hole, ref nhole);
                        Utils.PushFront(a, harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            tris = (Array.Empty<Int3>(), Array.Empty<int>());
                            return false;
                        }
                        Utils.PushBack(eb, hole, ref nhole);
                        Utils.PushBack(a, harea, ref nharea);
                        add = true;
                    }

                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i] = edges[nedges - 1];
                        nedges--;
                        match = true;
                        i--;
                    }
                }

                if (!match)
                {
                    break;
                }
            }

            var tverts = new Int3[nhole];
            var thole = new int[nhole];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = Verts[pi].X;
                tverts[i].Y = Verts[pi].Y;
                tverts[i].Z = Verts[pi].Z;
                thole[i] = i;
            }

            tris = (tverts, thole);
            return true;
        }
        /// <summary>
        /// Build initial polygons for the remove operation
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="ntris">Number of triangles</param>
        /// <param name="hole">Hole indices</param>
        /// <param name="harea">Area list</param>
        /// <returns>Returns the indexed polygons, regions and areas</returns>
        private static (IndexedPolygon[] Polys, SamplePolyAreas[] PAreas, int NPolys) BuildRemoveInitialPolygons(Int3[] tris, int ntris, int[] hole, SamplePolyAreas[] harea)
        {
            // Merge the hole triangles back to polygons.
            var polys = new IndexedPolygon[(ntris + 1)];
            var pareas = new SamplePolyAreas[ntris];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys] = new IndexedPolygon();
                    polys[npolys][0] = hole[t.X];
                    polys[npolys][1] = hole[t.Y];
                    polys[npolys][2] = hole[t.Z];

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }

            return (polys, pareas, npolys);
        }

        /// <summary>
        /// Builds the mesh adjacency
        /// </summary>
        /// <param name="lcset">Contour set</param>
        public void BuildMeshAdjacency(TileCacheContourSet lcset)
        {
            var polys = Polys;
            int npolys = NPolys;
            int nverts = NVerts;
            int vertsPerPoly = IndexedPolygon.DT_VERTS_PER_POLYGON;

            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = NPolys * vertsPerPoly;
            int[] firstEdge = Helper.CreateArray(nverts, IndexedPolygon.RC_MESH_NULL_IDX);
            int[] nextEdge = Helper.CreateArray(maxEdgeCount, IndexedPolygon.RC_MESH_NULL_IDX);
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (IndexedPolygon.IndexIsNull(t[j]))
                    {
                        break;
                    }

                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || IndexedPolygon.IndexIsNull(t[j + 1])) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        var edge = new Edge()
                        {
                            Vert = new int[2],
                            PolyEdge = new int[2],
                            Poly = new int[2],
                        };
                        edge.Vert[0] = v0;
                        edge.Vert[1] = v1;
                        edge.Poly[0] = i;
                        edge.PolyEdge[0] = j;
                        edge.Poly[1] = i;
                        edge.PolyEdge[1] = 0xff;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (IndexedPolygon.IndexIsNull(t[j]))
                    {
                        break;
                    }

                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || IndexedPolygon.IndexIsNull(t[j + 1])) ? t[0] : t[j + 1];
                    if (v0 <= v1)
                    {
                        continue;
                    }

                    bool found = false;
                    for (int e = firstEdge[v1]; !IndexedPolygon.IndexIsNull(e); e = nextEdge[e])
                    {
                        var edge = edges[e];
                        if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                        {
                            edge.Poly[1] = i;
                            edge.PolyEdge[1] = j;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }

                    // Matching edge not found, it is an open edge, add it.
                    var nedge = new Edge()
                    {
                        Vert = new int[2],
                        PolyEdge = new int[2],
                        Poly = new int[2],
                    };
                    nedge.Vert[0] = v1;
                    nedge.Vert[1] = v0;
                    nedge.Poly[0] = i;
                    nedge.PolyEdge[0] = j;
                    nedge.Poly[1] = i;
                    nedge.PolyEdge[1] = 0xff;
                    edges[edgeCount] = nedge;

                    // Insert edge
                    nextEdge[edgeCount] = firstEdge[v1];
                    firstEdge[v1] = edgeCount;
                    edgeCount++;
                }
            }

            // Mark portal edges.
            FindPortalEdges(lcset, edges, edgeCount);

            // Store adjacency
            StoreAdjacency(polys, vertsPerPoly, edges, edgeCount);
        }
        /// <summary>
        /// Stores the adjacency data
        /// </summary>
        /// <param name="polys">Polygon list to update</param>
        /// <param name="vertsPerPoly">Vertices per polygon</param>
        /// <param name="edges">Edge list</param>
        /// <param name="edgeCount">Number of edges in the list</param>
        private static void StoreAdjacency(IndexedPolygon[] polys, int vertsPerPoly, Edge[] edges, int edgeCount)
        {
            for (int i = 0; i < edgeCount; ++i)
            {
                var e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = e.Poly[1];
                    p1[vertsPerPoly + e.PolyEdge[1]] = e.Poly[0];
                }
                else if (e.PolyEdge[1] != 0xff)
                {
                    var p0 = polys[e.Poly[0]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = 0x8000 | e.PolyEdge[1];
                }
            }
        }
        /// <summary>
        /// Finds edges between portals
        /// </summary>
        /// <param name="lcset">Contour set</param>
        private readonly void FindPortalEdges(TileCacheContourSet lcset, Edge[] edges, int edgeCount)
        {
            // Mark portal edges.
            for (int i = 0; i < lcset.NConts; ++i)
            {
                var cont = lcset.Conts[i];
                if (cont.NVertices < 3)
                {
                    continue;
                }

                for (int j = 0, k = cont.NVertices - 1; j < cont.NVertices; k = j++)
                {
                    var va = cont.Vertices[k];
                    var vb = cont.Vertices[j];
                    int dir = va.Flag & 0xf;
                    if (dir == 0xf)
                    {
                        continue;
                    }

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = va.X;
                        int zmin = va.Z;
                        int zmax = vb.Z;
                        if (zmin > zmax)
                        {
                            Helper.Swap(ref zmin, ref zmax);
                        }

                        for (int m = 0; m < edgeCount; ++m)
                        {
                            var e = edges[m];
                            // Skip connected edges.
                            if (e.Poly[0] != e.Poly[1])
                            {
                                continue;
                            }
                            var eva = Verts[e.Vert[0]];
                            var evb = Verts[e.Vert[1]];
                            if (eva.X == x && evb.X == x)
                            {
                                int ezmin = eva.Z;
                                int ezmax = evb.Z;
                                if (ezmin > ezmax)
                                {
                                    Helper.Swap(ref ezmin, ref ezmax);
                                }
                                if (Utils.OverlapRange(zmin, zmax, ezmin, ezmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.PolyEdge[1] = dir;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Find matching vertical edge
                        int z = va.Z;
                        int xmin = va.X;
                        int xmax = vb.X;
                        if (xmin > xmax)
                        {
                            Helper.Swap(ref xmin, ref xmax);
                        }
                        for (int m = 0; m < edgeCount; ++m)
                        {
                            var e = edges[m];
                            // Skip connected edges.
                            if (e.Poly[0] != e.Poly[1])
                            {
                                continue;
                            }
                            var eva = Verts[e.Vert[0]];
                            var evb = Verts[e.Vert[1]];
                            if (eva.Z == z && evb.Z == z)
                            {
                                int exmin = eva.X;
                                int exmax = evb.X;
                                if (exmin > exmax)
                                {
                                    Helper.Swap(ref exmin, ref exmax);
                                }
                                if (Utils.OverlapRange(xmin, xmax, exmin, exmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.PolyEdge[1] = dir;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merges the polygon list with their regions and areas
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons</param>
        /// <param name="pareas">Area list</param>
        /// <returns>Returns the resulting merged polygon</returns>
        private readonly (IndexedPolygon[] MergedPolys, int MergedNPolys, SamplePolyAreas[] MergedAreas) MergePolygons(IndexedPolygon[] polys, int npolys, SamplePolyAreas[] pareas)
        {
            var mergedPolys = polys.ToArray();
            var mergedNpolys = npolys;
            var mergedareas = pareas?.ToArray() ?? Array.Empty<SamplePolyAreas>();

            int nvp = NVP;
            if (nvp <= 3)
            {
                return (mergedPolys, mergedNpolys, mergedareas);
            }

            while (true)
            {
                // Find best polygons to merge.
                int bestMergeVal = 0;
                int bestPa = 0;
                int bestPb = 0;
                int bestEa = 0;
                int bestEb = 0;

                for (int j = 0; j < mergedNpolys - 1; ++j)
                {
                    var pj = mergedPolys[j];
                    for (int k = j + 1; k < mergedNpolys; ++k)
                    {
                        var pk = mergedPolys[k];
                        int v = IndexedPolygon.GetMergeValue(pj, pk, Verts, out int ea, out int eb);
                        if (v > bestMergeVal)
                        {
                            bestMergeVal = v;
                            bestPa = j;
                            bestPb = k;
                            bestEa = ea;
                            bestEb = eb;
                        }
                    }
                }

                if (bestMergeVal <= 0)
                {
                    // Could not merge any polygons, stop.
                    break;
                }

                // Found best, merge.
                mergedPolys[bestPa] = IndexedPolygon.Merge(mergedPolys[bestPa], mergedPolys[bestPb], bestEa, bestEb);
                mergedPolys[bestPb] = mergedPolys[mergedNpolys - 1].Copy();

                if (mergedareas.Any())
                {
                    mergedareas[bestPb] = mergedareas[mergedNpolys - 1];
                }

                mergedNpolys--;
            }

            // Cut to mergedNpolys
            mergedPolys = mergedPolys.Take(mergedNpolys).ToArray();
            if (mergedareas.Any()) mergedareas = mergedareas.Take(mergedNpolys).ToArray();

            return (mergedPolys, mergedNpolys, mergedareas);
        }
        /// <summary>
        /// Merges the polygon list
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons</param>
        /// <returns>Returns the resulting merged polygon</returns>
        public readonly (IndexedPolygon[] MergedPolys, int MergedNPolys) MergePolygons(IndexedPolygon[] polys, int npolys)
        {
            var (mergedPolys, mergedNpolys, _) = MergePolygons(polys, npolys, null);

            return (mergedPolys, mergedNpolys);
        }
        /// <summary>
        /// Stores the polygon list into the mesh
        /// </summary>
        /// <param name="polys">Polygon index list</param>
        /// <param name="npolys">Number of polygon indices</param>
        /// <param name="pareas">Area list</param>
        /// <param name="maxPolys">Maximum number of polygons to store</param>
        private bool StorePolygons(IndexedPolygon[] polys, int npolys, SamplePolyAreas[] pareas, int maxPolys)
        {
            for (int i = 0; i < npolys; ++i)
            {
                if (npolys > maxPolys)
                {
                    return false;
                }

                //Polygon with adjacency
                var p = new IndexedPolygon(IndexedPolygon.DT_VERTS_PER_POLYGON * 2);
                p.CopyVertices(polys[i], IndexedPolygon.DT_VERTS_PER_POLYGON);

                StorePolygon(p, pareas[i]);
            }

            return true;
        }
        /// <summary>
        /// Stores the polygon list into the mesh
        /// </summary>
        /// <param name="polys">Polygon index list</param>
        /// <param name="npolys">Number of polygon indices</param>
        /// <param name="cont">Contour</param>
        /// <param name="maxPolys">Maximum number of polygons to store</param>
        private bool StorePolygons(IndexedPolygon[] polys, int npolys, TileCacheContour cont, int maxPolys)
        {
            for (int i = 0; i < npolys; ++i)
            {
                if (npolys > maxPolys)
                {
                    return false;
                }

                //Polygon with adjacency
                var p = new IndexedPolygon(IndexedPolygon.DT_VERTS_PER_POLYGON * 2);
                p.CopyVertices(polys[i], IndexedPolygon.DT_VERTS_PER_POLYGON);

                StorePolygon(p, (SamplePolyAreas)(int)cont.Area);
            }

            return true;
        }
        /// <summary>
        /// Stores a plygon into the mesh
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        /// <param name="area">Area type</param>
        private void StorePolygon(IndexedPolygon p, SamplePolyAreas area)
        {
            Polys[NPolys] = p;
            Areas[NPolys] = area;
            NPolys++;
        }
    }
}
