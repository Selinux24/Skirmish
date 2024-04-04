using SharpDX;
using System;

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
        /// Number of vertices.
        /// </summary>
        private int vertCount;
        /// <summary>
        /// Vertices of the mesh, 3 elements per vertex.
        /// </summary>
        private Int3[] vertList;
        /// <summary>
        /// Number of polygons.
        /// </summary>
        private int polyCount;
        /// <summary>
        /// Polygons of the mesh, nvp*2 elements per polygon.
        /// </summary>
        private IndexedPolygon[] polyList;
        /// <summary>
        /// Per polygon flags.
        /// </summary>
        private SamplePolyFlagTypes[] flagList;
        /// <summary>
        /// Area ID of polygons.
        /// </summary>
        private SamplePolyAreas[] areaList;

        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; set; }

        /// <summary>
        /// Builds a tile cache polygon mesh
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="nvp">Number of maximum vertex per polygon</param>
        public static TileCachePolyMesh Build(TileCacheContourSet cset, int nvp)
        {
            cset.GetGeometryConfiguration(out int maxVertices, out int maxPolys, out _);

            var mesh = new TileCachePolyMesh
            {
                vertCount = 0,
                vertList = new Int3[maxVertices],

                polyCount = 0,
                polyList = new IndexedPolygon[maxPolys],
                areaList = Helper.CreateArray(maxPolys, SamplePolyAreas.None),
                flagList = Helper.CreateArray(maxPolys, SamplePolyFlagTypes.None),

                NVP = nvp,
            };

            bool[] vflags = new bool[maxVertices];
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, NULL_IDX);

            foreach (var (i, cont) in cset.IterateContours())
            {
                // Skip null contours.
                int nverts = cont.GetVertexCount();
                if (nverts < 3)
                {
                    continue;
                }

                // Triangulate contour
                var tris = cont.Triangulate();

                // Add and merge vertices.
                int[] indices = new int[nverts];
                foreach (var (j, cv) in cont.IterateVertices())
                {
                    indices[j] = mesh.AddVertex(cv, firstVert, nextVert);
                    if (Edge.IsBorderVertex(cv.Flag))
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = true;
                    }
                }

                // Build initial polygons.
                var polys = IndexedPolygon.CreateInitialPolygons(indices, tris);
                if (polys.Length == 0)
                {
                    continue;
                }

                if (nvp > 3)
                {
                    // Merge polygons.
                    polys = IndexedPolygon.MergePolygons(polys, mesh.vertList);
                }

                // Store polygons.
                if (!mesh.StorePolygons(polys, polys.Length, cont, maxPolys))
                {
                    throw new EngineException($"rcBuildPolyMesh: Too many polygons {mesh.polyCount} (max:{maxPolys}).");
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
        /// <param name="cv">Contour vertex</param>
        /// <param name="firstVert">First vertex</param>
        /// <param name="nextVert">Next vertex</param>
        /// <returns>Returns the added index</returns>
        private int AddVertex(ContourVertex cv, int[] firstVert, int[] nextVert)
        {
            int x = cv.X;
            int y = cv.Y;
            int z = cv.Z;

            int bucket = Utils.ComputeVertexHash(x, 0, z, VERTEX_BUCKET_COUNT - 1);
            int i = firstVert[bucket];

            while (i != NULL_IDX)
            {
                var v = vertList[i];
                if (v.X == x && v.Z == z && (Math.Abs(v.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = vertCount++;
            vertList[i] = new(x, y, z);
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
            for (int i = 0; i < vertCount; ++i)
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
                for (int j = i; j < vertCount; ++j)
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
            var (numTouchedVerts, numRemainingEdges) = IndexedPolygon.CountVertexToRemove(polyList, polyCount, rem);

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

            for (int i = 0; i < polyCount; ++i)
            {
                var p = polyList[i];
                int nv = p.CountPolyVerts();

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    int a = p.GetVertex(j);
                    int b = p.GetVertex(k);

                    if (a != rem && b != rem)
                    {
                        continue;
                    }

                    // Arrange edge so that a=rem.
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
            int numOpenEdges = IndexedEdge.CountOpenEdges(edges, nedges);

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
            int numRemovedVerts = IndexedPolygon.CountPolygonsToRemove(polyList, polyCount, rem);
            numRemovedVerts = Math.Min(numRemovedVerts, MAX_REM_EDGES);

            var (edges, nedges) = RemoveEdges(rem, numRemovedVerts);
            if (nedges <= 0)
            {
                return nedges == 0;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            var (result, triVerts, triHole, hole, harea) = GetTriangulateHole(edges, ref nedges, numRemovedVerts);
            if (!result)
            {
                return false;
            }

            // Triangulate the hole.
            var (triRes, tris) = TriangulationHelper.Triangulate(triVerts, triHole);
            if (!triRes)
            {
                Logger.WriteWarning(this, "removeVertex: Hole triangulation error");
            }

            if (tris.Length > MAX_REM_EDGES)
            {
                return false;
            }

            // Merge the hole triangles back to polygons.
            var (polys, pareas, _) = IndexedPolygon.CreateInitialPolygons(hole, tris, harea, null);
            if (polys.Length == 0)
            {
                return true;
            }

            if (NVP > 3)
            {
                // Merge polygons.
                var (mergedPolys, mergedAreas, _) = IndexedPolygon.MergePolygons(polys, pareas, null, vertList);
                polys = mergedPolys;
                pareas = mergedAreas;
            }

            // Store polygons.
            if (!StorePolygons(polys, polys.Length, pareas, maxPolys))
            {
                Logger.WriteWarning(this, $"removeVertex: Too many polygons {polyCount} (max:{maxPolys}).");

                return false;
            }

            return true;
        }
        /// <summary>
        /// Remove edges
        /// </summary>
        /// <param name="rem">Index to remove</param>
        /// <param name="numRemovedVerts">Number of removed vertices</param>
        /// <returns>Returns the edge list</returns>
        private (IndexedRegionEdge[] Edges, int NEdges) RemoveEdges(int rem, int numRemovedVerts)
        {
            var edges = new IndexedRegionEdge[numRemovedVerts];
            int nedges = 0;
            for (int i = 0; i < polyCount; ++i)
            {
                var p = polyList[i];

                if (!p.ContainsVertex(rem))
                {
                    continue;
                }

                // Collect edges which does not touch the removed vertex.
                int nv = p.CountPolyVerts();
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    int a = p.GetVertex(j);
                    int b = p.GetVertex(k);

                    if (a == rem || b == rem)
                    {
                        continue;
                    }

                    if (nedges >= numRemovedVerts)
                    {
                        return (Array.Empty<IndexedRegionEdge>(), -1);
                    }

                    edges[nedges++] = new(b, a, -1, areaList[i]);
                }

                // Remove the polygon.
                RemovePolygon(i);

                --i;
            }

            // Remove vertex.
            RemoveVertex(rem);

            IndexedRegionEdge.RemoveIndex(edges, nedges, rem);

            return (edges, nedges);
        }
        /// <summary>
        /// Removes the specified polygon by index
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemovePolygon(int rem)
        {
            polyList[rem] = polyList[polyCount - 1];
            areaList[rem] = areaList[polyCount - 1];

            polyList[polyCount - 1] = null;
            areaList[polyCount - 1] = SamplePolyAreas.None;

            polyCount--;
        }
        /// <summary>
        /// Removes the specified vertex by index, and adjusts the indexed polygon layout
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemoveVertex(int rem)
        {
            for (int i = rem; i < vertCount - 1; ++i)
            {
                vertList[i] = vertList[i + 1];
            }
            vertCount--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < polyCount; ++i)
            {
                var p = polyList[i];
                int nv = p.CountPolyVerts();
                for (int j = 0; j < nv; ++j)
                {
                    var a = p.GetVertex(j);
                    if (a > rem)
                    {
                        p.SetVertex(j, a - 1);
                    }
                }
            }
        }
        /// <summary>
        /// Gets the triangulated hole
        /// </summary>
        /// <param name="edges">Edge list</param>
        /// <param name="nedges">Number of edges</param>
        /// <param name="numRemovedVerts">Number of removed vertices</param>
        /// <returns>Returns the triangulated vertices and the triangulated hole</returns>
        private readonly (bool Result, Int3[] TriVerts, int[] TriHole, int[] Hole, SamplePolyAreas[] HArea) GetTriangulateHole(IndexedRegionEdge[] edges, ref int nedges, int numRemovedVerts)
        {
            var hole = new int[numRemovedVerts * NVP];
            var harea = new SamplePolyAreas[numRemovedVerts * NVP];

            int nhole = 0;
            int nharea = 0;

            ArrayUtils.PushBack(edges[0].EdgeIndexA, hole, ref nhole);
            ArrayUtils.PushBack(edges[0].Area, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = MatchEdge(edges, ref nedges, hole, ref nhole, harea, ref nharea);

                if (nhole >= MAX_REM_EDGES)
                {
                    return (false, Array.Empty<Int3>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<SamplePolyAreas>());
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
                tverts[i] = vertList[hole[i]];
                thole[i] = i;
            }

            return (true, tverts, thole, hole, harea);
        }
        /// <summary>
        /// Gets whether the edges segment matches the hole boundary
        /// </summary>
        /// <param name="edges">Edges list</param>
        /// <param name="nedges">Number of edges</param>
        /// <param name="hole">Hole indices</param>
        /// <param name="nhole">Number of indices in the hole</param>
        /// <param name="harea">Hole areas</param>
        /// <param name="nharea">Number of areas in the hole</param>
        private static bool MatchEdge(IndexedRegionEdge[] edges, ref int nedges, int[] hole, ref int nhole, SamplePolyAreas[] harea, ref int nharea)
        {
            bool match = false;

            for (int i = 0; i < nedges; ++i)
            {
                int ea = edges[i].EdgeIndexA;
                int eb = edges[i].EdgeIndexB;
                var a = edges[i].Area;

                bool added = false;
                if (hole[0] == eb)
                {
                    // The segment matches the beginning of the hole boundary.
                    if (nhole >= MAX_REM_EDGES)
                    {
                        return false;
                    }
                    ArrayUtils.PushFront(ea, hole, ref nhole);
                    ArrayUtils.PushFront(a, harea, ref nharea);
                    added = true;
                }
                else if (hole[nhole - 1] == ea)
                {
                    // The segment matches the end of the hole boundary.
                    if (nhole >= MAX_REM_EDGES)
                    {
                        return false;
                    }
                    ArrayUtils.PushBack(eb, hole, ref nhole);
                    ArrayUtils.PushBack(a, harea, ref nharea);
                    added = true;
                }

                if (added)
                {
                    // The edge segment was added, remove it.
                    edges[i] = edges[nedges - 1];
                    nedges--;
                    match = true;
                    i--;
                }
            }

            return match;
        }

        /// <summary>
        /// Builds the mesh adjacency
        /// </summary>
        /// <param name="cset">Contour set</param>
        public readonly void BuildMeshAdjacency(TileCacheContourSet cset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            var (edges, edgeCount) = IndexedPolygon.BuildAdjacencyEdges(polyList, polyCount, vertCount, true, 0xff);

            // Mark portal edges.
            FindPortalEdges(cset, edges, edgeCount);

            // Store adjacency
            IndexedPolygon.StoreAdjacency(polyList, edges, edgeCount, true, 0xff);
        }
        /// <summary>
        /// Finds edges between portals
        /// </summary>
        /// <param name="cset">Contour set</param>
        private readonly void FindPortalEdges(TileCacheContourSet cset, Edge[] edges, int edgeCount)
        {
            // Mark portal edges.
            foreach (var (i, cont) in cset.IterateContours())
            {
                if (cont.GetVertexCount() < 3)
                {
                    continue;
                }

                foreach (var (va, vb) in cont.IterateSegments())
                {
                    if (!Edge.HasDirection(va.Flag))
                    {
                        continue;
                    }

                    int dir = Edge.GetVertexDirection(va.Flag);
                    if (dir == 0 || dir == 2)
                    {
                        // Find matching horizontal edge
                        FindMatchingHorizontalEdge(va, vb, edges, edgeCount, dir);
                    }
                    else
                    {
                        // Find matching vertical edge
                        FindMatchingVerticalEdge(va, vb, edges, edgeCount, dir);
                    }
                }
            }
        }
        /// <summary>
        /// Finds matching vertical edge
        /// </summary>
        /// <param name="va">First vertex</param>
        /// <param name="vb">Second vertex</param>
        /// <param name="edges">Edge list</param>
        /// <param name="edgeCount">Number of edges in the list</param>
        /// <param name="dir">Search direction</param>
        private readonly void FindMatchingVerticalEdge(ContourVertex va, ContourVertex vb, Edge[] edges, int edgeCount, int dir)
        {
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

                var eva = vertList[e.Vert[0]];
                var evb = vertList[e.Vert[1]];
                if (eva.X != x || evb.X != x)
                {
                    continue;
                }

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
        /// <summary>
        /// Finds matching horizontal edge
        /// </summary>
        /// <param name="va">First vertex</param>
        /// <param name="vb">Second vertex</param>
        /// <param name="edges">Edge list</param>
        /// <param name="edgeCount">Number of edges in the list</param>
        /// <param name="dir">Search direction</param>
        private readonly void FindMatchingHorizontalEdge(ContourVertex va, ContourVertex vb, Edge[] edges, int edgeCount, int dir)
        {
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

                var eva = vertList[e.Vert[0]];
                var evb = vertList[e.Vert[1]];
                if (eva.Z != z || evb.Z != z)
                {
                    continue;
                }

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

        /// <summary>
        /// Update polygon flags
        /// </summary>
        public readonly void UpdatePolyFlags()
        {
            // Update poly flags from areas.
            for (int i = 0; i < polyCount; ++i)
            {
                if ((int)areaList[i] == (int)AreaTypes.RC_WALKABLE_AREA)
                {
                    areaList[i] = SamplePolyAreas.Ground;
                }

                flagList[i] = SamplePolyFlagTypesExtents.EvaluateArea(areaList[i]);
            }
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
                var p = new IndexedPolygon(NVP, true);
                p.CopyVertices(polys[i]);

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
                var p = new IndexedPolygon(NVP, true);
                p.CopyVertices(polys[i]);

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
            polyList[polyCount] = p;
            areaList[polyCount] = area;
            polyCount++;
        }

        /// <summary>
        /// Gets the number of vertices
        /// </summary>
        public readonly int GetVertexCount()
        {
            return vertCount;
        }
        /// <summary>
        /// Return vertices
        /// </summary>
        public readonly Int3[] GetVertices()
        {
            return [.. vertList];
        }

        /// <summary>
        /// Gets the number of polygons
        /// </summary>
        public readonly int GetPolyCount()
        {
            return polyCount;
        }
        /// <summary>
        /// Return polygons
        /// </summary>
        public readonly IndexedPolygon[] GetPolygons()
        {
            return [.. polyList];
        }

        /// <summary>
        /// Return Areas
        /// </summary>
        public readonly SamplePolyAreas[] GetAreas()
        {
            return [.. areaList];
        }
        /// <summary>
        /// Gets the area at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly SamplePolyAreas GetArea(int index)
        {
            return areaList[index];
        }
        /// <summary>
        /// Sets the area at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="area">Area value</param>
        public readonly void SetArea(int index, SamplePolyAreas area)
        {
            areaList[index] = area;
        }

        /// <summary>
        /// Return flags
        /// </summary>
        public readonly SamplePolyFlagTypes[] GetFlags()
        {
            return [.. flagList];
        }
        /// <summary>
        /// Gets the flag at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly SamplePolyFlagTypes GetFlag(int index)
        {
            return flagList[index];
        }
        /// <summary>
        /// Sets tg
        /// </summary>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        public readonly void SetFlag(int index, SamplePolyFlagTypes flag)
        {
            flagList[index] = flag;
        }
    }
}
