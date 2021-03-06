﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Represents a polygon mesh suitable for use in building a navigation mesh.
    /// </summary>
    class PolyMesh
    {
        /// <summary>
        /// Vertex bucket count
        /// </summary>
        const int VERTEX_BUCKET_COUNT = (1 << 12);

        /// <summary>
        /// Builds a polygon mesh
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="nvp">Number of maximum vertex per polygon</param>
        /// <returns>Returns the new polygon mesh</returns>
        public static PolyMesh Build(ContourSet cset, int nvp)
        {
            PolyMesh mesh = new PolyMesh
            {
                BMin = cset.BMin,
                BMax = cset.BMax,
                CS = cset.CellSize,
                CH = cset.CellHeight,
                BorderSize = cset.BorderSize,
                MaxEdgeError = cset.MaxError
            };

            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < cset.NConts; ++i)
            {
                // Skip null contours.
                if (cset.Conts[i].NVertices < 3) continue;
                maxVertices += cset.Conts[i].NVertices;
                maxTris += cset.Conts[i].NVertices - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, cset.Conts[i].NVertices);
            }

            if (maxVertices >= 0xfffe)
            {
                throw new EngineException(string.Format("rcBuildPolyMesh: Too many vertices {0}.", maxVertices));
            }

            int[] vflags = new int[maxVertices];

            mesh.Verts = new Int3[maxVertices];
            mesh.Polys = new IndexedPolygon[maxTris];
            mesh.Regs = new int[maxTris];
            mesh.Areas = new SamplePolyAreas[maxTris];

            mesh.NVerts = 0;
            mesh.NPolys = 0;
            mesh.NVP = nvp;
            mesh.MaxPolys = maxTris;

            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, -1);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < cset.NConts; ++i)
            {
                var cont = cset.Conts[i];

                // Skip null contours.
                if (cont.NVertices < 3)
                {
                    continue;
                }

                // Triangulate contour
                for (int j = 0; j < cont.NVertices; ++j)
                {
                    indices[j] = j;
                }

                int ntris = RecastUtils.Triangulate(cont.Vertices, ref indices, out Int3[] tris);
                if (ntris <= 0)
                {
                    // Bad triangulation, should not happen.
                    Logger.WriteWarning(nameof(PolyMesh), $"rcBuildPolyMesh: Bad triangulation Contour {i}.");
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.NVertices; ++j)
                {
                    var v = cont.Vertices[j];
                    indices[j] = mesh.AddVertex(v.X, v.Y, v.Z, firstVert, nextVert);
                    if ((v.W & ContourSet.RC_BORDER_VERTEX) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                IndexedPolygon[] polys = new IndexedPolygon[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new IndexedPolygon(DetourUtils.DT_VERTS_PER_POLYGON);
                        polys[npolys][0] = indices[t.X];
                        polys[npolys][1] = indices[t.Y];
                        polys[npolys][2] = indices[t.Z];
                        npolys++;
                    }
                }
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                if (nvp > 3)
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
                                int v = IndexedPolygon.GetMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
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
                            polys[bestPb] = polys[npolys - 1].Copy();
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
                for (int j = 0; j < npolys; ++j)
                {
                    var p = new IndexedPolygon(nvp * 2); //Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < nvp; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.Polys[mesh.NPolys] = p;
                    mesh.Regs[mesh.NPolys] = cont.RegionId;
                    mesh.Areas[mesh.NPolys] = (SamplePolyAreas)(int)cont.Area;
                    mesh.NPolys++;
                    if (mesh.NPolys > maxTris)
                    {
                        throw new EngineException(string.Format("rcBuildPolyMesh: Too many polygons {0} (max:{1}).", mesh.NPolys, maxTris));
                    }
                }
            }

            // Remove edge vertices.
            for (int i = 0; i < mesh.NVerts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!mesh.CanRemoveVertex(i))
                    {
                        continue;
                    }
                    if (!mesh.RemoveVertex(i, maxTris))
                    {
                        // Failed to remove vertex
                        throw new EngineException(string.Format("Failed to remove edge vertex {0}.", i));
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    // Fixup vertex flags
                    for (int j = i; j < mesh.NVerts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }

            // Calculate adjacency.
            mesh.BuildMeshAdjacency();

            // Find portal edges
            if (mesh.BorderSize > 0)
            {
                int w = cset.Width;
                int h = cset.Height;
                for (int i = 0; i < mesh.NPolys; ++i)
                {
                    var p = mesh.Polys[i];
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (IndexedPolygon.IndexIsNull(p[j]))
                        {
                            break;
                        }
                        // Skip connected edges.
                        if (!IndexedPolygon.IndexIsNull(p[nvp + j]))
                        {
                            continue;
                        }
                        int nj = j + 1;
                        if (nj >= nvp || IndexedPolygon.IndexIsNull(p[nj]))
                        {
                            nj = 0;
                        }
                        var va = mesh.Verts[p[j]];
                        var vb = mesh.Verts[p[nj]];

                        if (va.X == 0 && vb.X == 0)
                        {
                            p[nvp + j] = 0x8000;
                        }
                        else if (va.Z == h && vb.Z == h)
                        {
                            p[nvp + j] = 0x8000 | 1;
                        }
                        else if (va.X == w && vb.X == w)
                        {
                            p[nvp + j] = 0x8000 | 2;
                        }
                        else if (va.Z == 0 && vb.Z == 0)
                        {
                            p[nvp + j] = 0x8000 | 3;
                        }
                    }
                }
            }

            // Just allocate the mesh flags array. The user is resposible to fill it.
            mesh.Flags = new SamplePolyFlagTypes[mesh.NPolys];

            return mesh;
        }
        /// <summary>
        /// Merges a list of polygon meshes into a new one
        /// </summary>
        /// <param name="meshes">Polygon mesh list</param>
        /// <returns>Returns the new polygon mesh</returns>
        public static PolyMesh Merge(IEnumerable<PolyMesh> meshes)
        {
            if (!meshes.Any())
            {
                return null;
            }

            var first = meshes.First();

            PolyMesh res = new PolyMesh
            {
                NVP = first.NVP,
                CS = first.CS,
                CH = first.CH,
                BMin = first.BMin,
                BMax = first.BMax
            };

            int maxVerts = 0;
            int maxPolys = 0;
            int maxVertsPerMesh = 0;
            foreach (var mesh in meshes)
            {
                mesh.BMin = Vector3.Min(mesh.BMin, mesh.BMin);
                mesh.BMax = Vector3.Max(mesh.BMax, mesh.BMax);
                maxVertsPerMesh = Math.Max(maxVertsPerMesh, mesh.NVerts);
                maxVerts += mesh.NVerts;
                maxPolys += mesh.NPolys;
            }

            res.NVerts = 0;
            res.Verts = new Int3[maxVerts];
            res.NPolys = 0;
            res.Polys = new IndexedPolygon[maxPolys];
            res.Regs = new int[maxPolys];
            res.Areas = new SamplePolyAreas[maxPolys];
            res.Flags = new SamplePolyFlagTypes[maxPolys];

            int[] nextVert = Helper.CreateArray(maxVerts, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, -1);
            int[] vremap = Helper.CreateArray(maxVertsPerMesh, 0);

            foreach (var pmesh in meshes)
            {
                int ox = (int)Math.Floor((pmesh.BMin.X - res.BMin.X) / res.CS + 0.5f);
                int oz = (int)Math.Floor((pmesh.BMin.X - res.BMin.Z) / res.CS + 0.5f);

                bool isMinX = (ox == 0);
                bool isMinZ = (oz == 0);
                bool isMaxX = ((int)Math.Floor((res.BMax.X - pmesh.BMax.X) / res.CS + 0.5f)) == 0;
                bool isMaxZ = ((int)Math.Floor((res.BMax.Z - pmesh.BMax.Z) / res.CS + 0.5f)) == 0;
                bool isOnBorder = (isMinX || isMinZ || isMaxX || isMaxZ);

                for (int j = 0; j < pmesh.NVerts; ++j)
                {
                    var v = pmesh.Verts[j];
                    vremap[j] = res.AddVertex(v.X + ox, v.Y, v.Z + oz, firstVert, nextVert);
                }

                for (int j = 0; j < pmesh.NPolys; ++j)
                {
                    var tgt = res.Polys[res.NPolys];
                    var src = pmesh.Polys[j];
                    res.Regs[res.NPolys] = pmesh.Regs[j];
                    res.Areas[res.NPolys] = pmesh.Areas[j];
                    res.Flags[res.NPolys] = pmesh.Flags[j];
                    res.NPolys++;
                    for (int k = 0; k < res.NVP; ++k)
                    {
                        if (IndexedPolygon.IndexIsNull(src[k]))
                        {
                            break;
                        }
                        tgt[k] = vremap[src[k]];
                    }

                    if (isOnBorder)
                    {
                        for (int k = res.NVP; k < res.NVP * 2; ++k)
                        {
                            if ((src[k] & 0x8000) != 0 && src[k] != 0xffff)
                            {
                                int dir = src[k] & 0xf;
                                switch (dir)
                                {
                                    case 0: // Portal x-
                                        if (isMinX) tgt[k] = src[k];
                                        break;
                                    case 1: // Portal z+
                                        if (isMaxZ) tgt[k] = src[k];
                                        break;
                                    case 2: // Portal x+
                                        if (isMaxX) tgt[k] = src[k];
                                        break;
                                    case 3: // Portal z-
                                        if (isMinZ) tgt[k] = src[k];
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // Calculate adjacency.
            res.BuildMeshAdjacency();

            return res;
        }
        /// <summary>
        /// Computes the vertex hash
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <returns>Returns the hash value</returns>
        /// <remarks>
        /// Using the vertex coordinates, calculates a unique bucket number for easy storing and retrieval.
        /// </remarks>
        private static int ComputeVertexHash(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (VERTEX_BUCKET_COUNT - 1));
        }

        /// <summary>
        /// The mesh vertices. [Form: (x, y, z) * #nverts]
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        /// </summary>
        public IndexedPolygon[] Polys { get; set; }
        /// <summary>
        /// The region id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public int[] Regs { get; set; }
        /// <summary>
        /// The user defined flags for each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// The area id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }
        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// The number of allocated polygons.
        /// </summary>
        public int MaxPolys { get; set; }
        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMin { get; set; }
        /// <summary>
        /// The maximum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMax { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
        /// <summary>
        /// The AABB border size used to generate the source data from which the mesh was derived.
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The max error of the polygon edges in the mesh.
        /// </summary>
        public float MaxEdgeError { get; set; }

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
            int bucket = ComputeVertexHash(x, 0, z);
            int i = firstVert[bucket];

            while (i != -1)
            {
                var v = Verts[i];
                if (v.X == x && (Math.Abs(v.Y - y) <= 2) && v.Z == z)
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = NVerts; NVerts++;
            Verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        /// <summary>
        /// Gets whether the specified vertex can be removed
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <returns>Returns true if the vertex can be removed</returns>
        private bool CanRemoveVertex(int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numTouchedVerts++;
                        numRemoved++;
                    }
                    numVerts++;
                }
                if (numRemoved != 0)
                {
                    numRemovedVerts += numRemoved;
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
            }

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            int maxEdges = numTouchedVerts * 2;
            int nedges = 0;
            Int3[] edges = new Int3[maxEdges];

            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];
                int nv = p.CountPolyVerts();

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] == rem || p[k] == rem)
                    {
                        // Arrange edge so that a=rem.
                        int a = p[j], b = p[k];
                        if (b == rem)
                        {
                            Helper.Swap(ref a, ref b);
                        }

                        // Check if the edge exists
                        bool exists = false;
                        for (int m = 0; m < nedges; ++m)
                        {
                            var e = edges[m];
                            if (e[1] == b)
                            {
                                // Exists, increment vertex share count.
                                e[2]++;
                                exists = true;
                            }
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            var e = new Int3();
                            e[0] = a;
                            e[1] = b;
                            e[2] = 1;
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i][2] < 2)
                {
                    numOpenEdges++;
                }
            }
            if (numOpenEdges > 2)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Removes the specified vertex
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <param name="maxTris">Maxmimum number of triangles</param>
        /// <returns>Returns true if the vertex were removed</returns>
        private bool RemoveVertex(int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            for (int i = 0; i < NPolys; i++)
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
            Int4[] edges = new Int4[numRemovedVerts * NVP];
            int nhole = 0;
            int[] hole = new int[numRemovedVerts * NVP];
            int nhreg = 0;
            int[] hreg = new int[numRemovedVerts * NVP];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[numRemovedVerts * NVP];

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
                            var e = new Int4(p[k], p[j], Regs[i], (int)Areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    var p2 = Polys[NPolys - 1];
                    if (p != p2)
                    {
                        Polys[i] = Polys[NPolys - 1];
                    }
                    Polys[NPolys - 1] = null;
                    Regs[i] = Regs[NPolys - 1];
                    Areas[i] = Areas[NPolys - 1];
                    NPolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < NVerts - 1; ++i)
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
            RecastUtils.PushBack(edges[0].Z, hreg, ref nhreg);
            RecastUtils.PushBack((SamplePolyAreas)edges[0].W, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;
                    int r = edges[i].Z;
                    SamplePolyAreas a = (SamplePolyAreas)edges[i].W;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        RecastUtils.PushFront(ea, hole, ref nhole);
                        RecastUtils.PushFront(r, hreg, ref nhreg);
                        RecastUtils.PushFront(a, harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        RecastUtils.PushBack(eb, hole, ref nhole);
                        RecastUtils.PushBack(r, hreg, ref nhreg);
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
            int ntris = RecastUtils.Triangulate(tverts, ref thole, out Int3[] tris);
            if (ntris < 0)
            {
                Logger.WriteWarning(this, "removeVertex: triangulate() returned bad results.");
                ntris = -ntris;
            }

            // Merge the hole triangles back to polygons.
            var polys = new IndexedPolygon[(ntris + 1)];
            var pregs = new int[ntris];
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

                    // If this polygon covers multiple region types then mark it as such
                    if (hreg[t.X] != hreg[t.Y] || hreg[t.Y] != hreg[t.Z])
                    {
                        pregs[npolys] = IndexedPolygon.RC_MULTIPLE_REGS;
                    }
                    else
                    {
                        pregs[npolys] = hreg[t.X];
                    }

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int nvp = NVP;
            if (nvp > 3)
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
                        if (pregs[bestPa] != pregs[bestPb])
                        {
                            pregs[bestPa] = IndexedPolygon.RC_MULTIPLE_REGS;
                        }
                        polys[bestPb] = polys[(npolys - 1)];
                        pregs[bestPb] = pregs[npolys - 1];
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
                var p = new IndexedPolygon();
                Polys[NPolys] = p;
                for (int j = 0; j < nvp; ++j)
                {
                    p[j] = polys[i][j];
                }
                Regs[NPolys] = pregs[i];
                Areas[NPolys] = pareas[i];
                NPolys++;
                if (NPolys > maxTris)
                {
                    Logger.WriteWarning(this, $"removeVertex: Too many polygons {NPolys} (max:{maxTris}).");
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Builds the mesh adjacency
        /// </summary>
        private void BuildMeshAdjacency()
        {
            IndexedPolygon[] polys = Polys;
            int npolys = NPolys;
            int nverts = NVerts;
            int vertsPerPoly = NVP;

            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * vertsPerPoly;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = IndexedPolygon.RC_MESH_NULL_IDX;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = IndexedPolygon.RC_MESH_NULL_IDX;
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
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
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
                        edge.PolyEdge[1] = 0;
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
                    if (v0 > v1)
                    {
                        for (int e = firstEdge[v1]; !IndexedPolygon.IndexIsNull(e); e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                            {
                                edge.Poly[1] = i;
                                edge.PolyEdge[1] = j;
                                break;
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                Edge e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = e.Poly[1];
                    p1[vertsPerPoly + e.PolyEdge[1]] = e.Poly[0];
                }
            }
        }
        /// <summary>
        /// Makes a copy of the current polygon mesh
        /// </summary>
        /// <returns>Returns the new polygon mesh</returns>
        public PolyMesh Copy()
        {
            PolyMesh dst = new PolyMesh
            {
                NVerts = NVerts,
                NPolys = NPolys,
                MaxPolys = NPolys,
                NVP = NVP,
                BMin = BMin,
                BMax = BMax,
                CS = CS,
                CH = CH,
                BorderSize = BorderSize,
                MaxEdgeError = MaxEdgeError,
                Verts = new Int3[NVerts],
                Polys = new IndexedPolygon[NPolys],
                Regs = new int[NPolys],
                Areas = new SamplePolyAreas[NPolys],
                Flags = new SamplePolyFlagTypes[NPolys]
            };

            Array.Copy(Verts, dst.Verts, NVerts);
            Array.Copy(Polys, dst.Polys, NPolys);
            Array.Copy(Regs, dst.Regs, NPolys);
            Array.Copy(Areas, dst.Areas, NPolys);
            Array.Copy(Flags, dst.Flags, NPolys);

            return dst;
        }
    }
}
