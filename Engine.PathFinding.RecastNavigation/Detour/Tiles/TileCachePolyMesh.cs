using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct TileCachePolyMesh
    {
        public const int MAX_REM_EDGES = 48;

        public int NVP { get; set; }
        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// Vertices of the mesh, 3 elements per vertex.
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// Polygons of the mesh, nvp*2 elements per polygon.
        /// </summary>
        public IndexedPolygon[] Polys { get; set; }
        /// <summary>
        /// Per polygon flags.
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// Area ID of polygons.
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }

        public static int ComputeVertexHash2(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (TileCache.VERTEX_BUCKET_COUNT2 - 1));
        }
        public static bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return !(amin >= bmax || amax <= bmin);
        }

        public bool BuildMeshAdjacency(TileCacheContourSet lcset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = NPolys * DetourUtils.DT_VERTS_PER_POLYGON;
            int[] firstEdge = new int[NVerts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < NVerts; i++)
            {
                firstEdge[i] = TileCache.DT_TILECACHE_NULL_IDX;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = TileCache.DT_TILECACHE_NULL_IDX;
            }

            for (int i = 0; i < NPolys; ++i)
            {
                var t = Polys[i];
                for (int j = 0; j < DetourUtils.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == TileCache.DT_TILECACHE_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= DetourUtils.DT_VERTS_PER_POLYGON || t[j + 1] == TileCache.DT_TILECACHE_NULL_IDX) ? t[0] : t[j + 1];
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

            for (int i = 0; i < NPolys; ++i)
            {
                var t = Polys[i];
                for (int j = 0; j < DetourUtils.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == TileCache.DT_TILECACHE_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= DetourUtils.DT_VERTS_PER_POLYGON || t[j + 1] == TileCache.DT_TILECACHE_NULL_IDX) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != TileCache.DT_TILECACHE_NULL_IDX; e = nextEdge[e])
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
                        if (!found)
                        {
                            // Matching edge not found, it is an open edge, add it.
                            var edge = new Edge()
                            {
                                Vert = new int[2],
                                PolyEdge = new int[2],
                                Poly = new int[2],
                            };
                            edge.Vert[0] = v1;
                            edge.Vert[1] = v0;
                            edge.Poly[0] = i;
                            edge.PolyEdge[0] = j;
                            edge.Poly[1] = i;
                            edge.PolyEdge[1] = 0xff;
                            edges[edgeCount] = edge;
                            // Insert edge
                            nextEdge[edgeCount] = firstEdge[v1];
                            firstEdge[v1] = edgeCount;
                            edgeCount++;
                        }
                    }
                }
            }

            // Mark portal edges.
            for (int i = 0; i < lcset.NConts; ++i)
            {
                var cont = lcset.Conts[i];
                if (cont.NVerts < 3)
                {
                    continue;
                }

                for (int j = 0, k = cont.NVerts - 1; j < cont.NVerts; k = j++)
                {
                    var va = cont.Verts[k];
                    var vb = cont.Verts[j];
                    int dir = va.W & 0xf;
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
                                if (OverlapRangeExl(zmin, zmax, ezmin, ezmax))
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
                                if (OverlapRangeExl(xmin, xmax, exmin, exmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.PolyEdge[1] = dir;
                                }
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                var e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = Polys[e.Poly[0]];
                    var p1 = Polys[e.Poly[1]];
                    p0[DetourUtils.DT_VERTS_PER_POLYGON + e.PolyEdge[0]] = e.Poly[1];
                    p1[DetourUtils.DT_VERTS_PER_POLYGON + e.PolyEdge[1]] = e.Poly[0];
                }
                else if (e.PolyEdge[1] != 0xff)
                {
                    var p0 = Polys[e.Poly[0]];
                    p0[DetourUtils.DT_VERTS_PER_POLYGON + e.PolyEdge[0]] = 0x8000 | e.PolyEdge[1];
                }
            }

            return true;
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
        public int AddVertex(int x, int y, int z, int[] firstVert, int[] nextVert)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];

            while (i != TileCache.DT_TILECACHE_NULL_IDX)
            {
                var vx = Verts[i];
                if (vx.X == x && vx.Z == z && (Math.Abs(vx.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = NVerts;
            NVerts++;
            Verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        /// <summary>
        /// Removes the specified vertex
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <param name="maxTris">Maxmimum number of triangles</param>
        /// <returns>Returns true if the vertex were removed</returns>
        public bool RemoveVertex(int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numRemovedVerts++;
                    }
                }
            }

            int nedges = 0;
            Int3[] edges = new Int3[MAX_REM_EDGES];
            int nhole = 0;
            int[] hole = new int[MAX_REM_EDGES];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[MAX_REM_EDGES];

            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem) hasRem = true;
                }
                if (hasRem)
                {
                    // Collect edges which does not touch the removed vertex.
                    for (int j = 0, k = nv - 1; j < nv; k = j++)
                    {
                        if (p[j] != rem && p[k] != rem)
                        {
                            if (nedges >= MAX_REM_EDGES)
                            {
                                return false;
                            }
                            var e = new Int3(p[k], p[j], (int)Areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    Polys[i] = Polys[NPolys - 1];
                    Polys[NPolys - 1] = null;
                    Areas[i] = Areas[NPolys - 1];
                    NPolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < NVerts; ++i)
            {
                Verts[i] = Verts[(i + 1)];
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
                if (edges[i].X > rem) edges[i].X--;
                if (edges[i].Y > rem) edges[i].Y--;
            }

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            RecastUtils.PushBack(edges[0].X, hole, ref nhole);
            RecastUtils.PushBack((SamplePolyAreas)edges[0].Z, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;

                    SamplePolyAreas a = (SamplePolyAreas)edges[i].Z;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            return false;
                        }
                        RecastUtils.PushFront(ea, hole, ref nhole);
                        RecastUtils.PushFront(a, harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            return false;
                        }
                        RecastUtils.PushBack(eb, hole, ref nhole);
                        RecastUtils.PushBack(a, harea, ref nharea);
                        add = true;
                    }
                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i] = edges[(nedges - 1)];
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

            var tverts = new Int4[nhole];
            var thole = new int[nhole];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = Verts[pi].X;
                tverts[i].Y = Verts[pi].Y;
                tverts[i].Z = Verts[pi].Z;
                tverts[i].W = 0;
                thole[i] = i;
            }

            // Triangulate the hole.
            int ntris = TriangulateHelper.Triangulate(tverts, ref thole, out var tris);
            if (ntris < 0)
            {
                Logger.WriteWarning(nameof(TileCachePolyMesh), "Hole triangulation error");

                ntris = -ntris;
            }

            if (ntris > MAX_REM_EDGES)
            {
                return false;
            }

            // Merge the hole triangles back to polygons.
            var polys = new IndexedPolygon[MAX_REM_EDGES];

            var pareas = new SamplePolyAreas[MAX_REM_EDGES];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys][0] = hole[t.X];
                    polys[npolys][1] = hole[t.Y];
                    polys[npolys][2] = hole[t.Z];

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int maxVertsPerPoly = DetourUtils.DT_VERTS_PER_POLYGON;
            if (maxVertsPerPoly > 3)
            {
                while (true)
                {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j)
                    {
                        var pj = polys[j];
                        for (int k = j + 1; k < npolys; ++k)
                        {
                            var pk = polys[k];
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

                    if (bestMergeVal > 0)
                    {
                        // Found best, merge.
                        polys[bestPa] = IndexedPolygon.Merge(polys[bestPa], polys[bestPb], bestEa, bestEb);
                        polys[bestPb] = polys[npolys - 1];
                        pareas[bestPb] = pareas[npolys - 1];
                        npolys--;
                    }
                    else
                    {
                        // Could not merge any polygons, stop.
                        break;
                    }
                }
            }

            // Store polygons.
            for (int i = 0; i < npolys; ++i)
            {
                if (NPolys >= maxTris) break;
                var p = Polys[NPolys];
                for (int j = 0; j < DetourUtils.DT_VERTS_PER_POLYGON; ++j)
                {
                    p[j] = polys[i][j];
                }

                Areas[NPolys] = pareas[i];
                NPolys++;
                if (NPolys > maxTris)
                {
                    Logger.WriteWarning(nameof(TileCachePolyMesh), $"removeVertex: Too many polygons {NPolys} (max:{maxTris}).");
                    return false;
                }
            }

            return true;
        }
    }
}
