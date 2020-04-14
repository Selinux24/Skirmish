using SharpDX;
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
                if (cset.Conts[i].NVerts < 3)
                {
                    continue;
                }
                maxVertices += cset.Conts[i].NVerts;
                maxTris += cset.Conts[i].NVerts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, cset.Conts[i].NVerts);
            }

            if (maxVertices >= 0xfffe)
            {
                throw new EngineException(string.Format("rcBuildPolyMesh: Too many vertices {0}.", maxVertices));
            }

            mesh.Verts = new Int3[maxVertices];
            mesh.Polys = new IndexedPolygon[maxTris];
            mesh.Regs = new int[maxTris];
            mesh.Areas = new SamplePolyAreas[maxTris];

            mesh.NVerts = 0;
            mesh.NPolys = 0;
            mesh.NVP = nvp;
            mesh.MaxPolys = maxTris;

            mesh.CreatePolygons(cset, maxVertices, maxVertsPerCont, out int[] vflags);

            // Remove edge vertices.
            mesh.RemoveEdgeVertices(vflags);

            // Calculate adjacency.
            if (!IndexedPolygon.BuildMeshAdjacency(mesh.Polys, mesh.NPolys, mesh.NVerts, mesh.NVP))
            {
                throw new EngineException("Adjacency failed.");
            }

            // Find portal edges
            mesh.FindPortalEdges(cset);

            // Just allocate the mesh flags array. The user is resposible to fill it.
            mesh.Flags = new SamplePolyFlagTypes[mesh.NPolys];

            return mesh;
        }
        public static PolyMesh Merge(PolyMesh[] meshes, int nmeshes)
        {
            if (nmeshes == 0 || meshes == null)
            {
                return null;
            }

            PolyMesh mesh = new PolyMesh
            {
                NVP = meshes[0].NVP,
                CS = meshes[0].CS,
                CH = meshes[0].CH,
                BMin = meshes[0].BMin,
                BMax = meshes[0].BMax
            };

            int maxVerts = 0;
            int maxPolys = 0;
            int maxVertsPerMesh = 0;
            for (int i = 0; i < nmeshes; ++i)
            {
                mesh.BMin = Vector3.Min(mesh.BMin, meshes[i].BMin);
                mesh.BMax = Vector3.Max(mesh.BMax, meshes[i].BMax);
                maxVertsPerMesh = Math.Max(maxVertsPerMesh, meshes[i].NVerts);
                maxVerts += meshes[i].NVerts;
                maxPolys += meshes[i].NPolys;
            }

            mesh.NVerts = 0;
            mesh.Verts = new Int3[maxVerts];
            mesh.NPolys = 0;
            mesh.Polys = new IndexedPolygon[maxPolys];
            mesh.Regs = new int[maxPolys];
            mesh.Areas = new SamplePolyAreas[maxPolys];
            mesh.Flags = new SamplePolyFlagTypes[maxPolys];

            int[] nextVert = Helper.CreateArray(maxVerts, 0);
            int[] firstVert = Helper.CreateArray(RecastUtils.VERTEX_BUCKET_COUNT, -1);

            for (int i = 0; i < nmeshes; ++i)
            {
                mesh.Remap(meshes[i], maxVertsPerMesh, firstVert, nextVert);
            }

            // Calculate adjacency.
            if (!IndexedPolygon.BuildMeshAdjacency(mesh.Polys, mesh.NPolys, mesh.NVerts, mesh.NVP))
            {
                throw new EngineException("rcMergePolyMeshes: Adjacency failed.");
            }

            return mesh;
        }
        private static int ComputeVertexHash(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (RecastUtils.VERTEX_BUCKET_COUNT - 1));
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

        public int AddVertex(int x, int y, int z, int[] firstVert, int[] nextVert)
        {
            int bucket = ComputeVertexHash(x, 0, z);
            int i = firstVert[bucket];

            while (i != -1)
            {
                var v = this.Verts[i];
                if (v.X == x && (Math.Abs(v.Y - y) <= 2) && v.Z == z)
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = this.NVerts; this.NVerts++;
            this.Verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        public bool CanRemoveVertex(int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < this.NPolys; ++i)
            {
                var p = this.Polys[i];
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
            var edges = GetSharedEdges(maxEdges, rem);

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = edges.Count(e => e.ShareCount < 2);

            return numOpenEdges > 2;
        }
        public bool RemoveVertex(int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = GetRemovableVertexCount(rem);

            // Remove the polygon and collect affected edges
            RemovePolygon(rem, numRemovedVerts, out Int4[] edges, out int nedges);

            // Remove vertex.
            RemoveVertexByIndex(rem, edges, nedges);

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            AdjustEdges(edges, nedges, numRemovedVerts, out Hole hole);

            // Triangulate the hole.
            int ntris = TriangulateHole(hole, out var tris);

            // Merge the hole triangles back to polygons.
            int npolys = MergeHolesIntoPolygon(tris, ntris, hole, out var polys, out var pregs, out var pareas);
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int nvp = this.NVP;
            if (nvp > 3)
            {
                while (true)
                {
                    if (!MergePolygons(polys, ref npolys, pregs, pareas))
                    {
                        break;
                    }
                }
            }

            // Store polygons.
            StorePolygons(polys, npolys, pregs, pareas);

            return true;
        }
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

        private IEnumerable<SharedEdge> GetSharedEdges(int maxEdges, int rem)
        {
            List<SharedEdge> edges = new List<SharedEdge>(maxEdges);

            for (int i = 0; i < this.NPolys; ++i)
            {
                var p = this.Polys[i];
                int nv = p.CountPolyVerts();

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    int a = p[j];
                    int b = p[k];

                    if (a == rem || b == rem)
                    {
                        bool exists = CheckUpdateSharedEdge(edges, a, b, rem);
                        if (!exists)
                        {
                            // Add new edge.
                            edges.Add(new SharedEdge(a, b, 1));
                        }
                    }
                }
            }

            return edges.ToArray();
        }
        private bool CheckUpdateSharedEdge(IEnumerable<SharedEdge> edges, int a, int b, int rem)
        {
            // Arrange edge so that a=rem.
            if (b == rem)
            {
                Helper.Swap(ref a, ref b);
            }

            // Check if the edge exists
            bool exists = false;
            for (int m = 0; m < edges.Count(); m++)
            {
                var e = edges.ElementAt(m);
                if (e.B == b)
                {
                    // Exists, increment vertex share count.
                    e.ShareCount++;
                    exists = true;
                }
            }

            return exists;
        }
        private int GetRemovableVertexCount(int rem)
        {
            int numRemovedVerts = 0;
            for (int i = 0; i < this.NPolys; i++)
            {
                var p = this.Polys[i];
                int nv = p.CountPolyVerts();
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numRemovedVerts++;
                    }
                }
            }

            return numRemovedVerts;
        }
        private void RemovePolygon(int rem, int numRemovedVerts, out Int4[] edges, out int nedges)
        {
            nedges = 0;
            edges = new Int4[numRemovedVerts * this.NVP];

            for (int i = 0; i < this.NPolys; ++i)
            {
                var p = this.Polys[i];
                int nv = p.CountPolyVerts();
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem) hasRem = true;
                }

                if (!hasRem)
                {
                    continue;
                }

                // Collect edges which does not touch the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] != rem && p[k] != rem)
                    {
                        var e = new Int4(p[k], p[j], this.Regs[i], (int)this.Areas[i]);
                        edges[nedges] = e;
                        nedges++;
                    }
                }

                // Remove the polygon.
                RemovePolygonAt(i, p);
                i--;
            }
        }
        private void RemovePolygonAt(int index, IndexedPolygon p)
        {
            var p2 = this.Polys[this.NPolys - 1];
            if (p != p2)
            {
                this.Polys[index] = this.Polys[this.NPolys - 1];
            }
            this.Polys[this.NPolys - 1] = null;
            this.Regs[index] = this.Regs[this.NPolys - 1];
            this.Areas[index] = this.Areas[this.NPolys - 1];
            this.NPolys--;
        }
        private void RemoveVertexByIndex(int rem, Int4[] edges, int nedges)
        {
            for (int i = rem; i < this.NVerts - 1; ++i)
            {
                this.Verts[i] = this.Verts[i + 1];
            }
            this.NVerts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < this.NPolys; ++i)
            {
                var p = this.Polys[i];
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
        }
        private void AdjustEdges(Int4[] edges, int nedges, int numRemovedVerts, out Hole hole)
        {
            hole = new Hole(numRemovedVerts * this.NVP);

            RecastUtils.PushBack(edges[0].X, hole.Indices, ref hole.NIndices);
            RecastUtils.PushBack(edges[0].Z, hole.Region, ref hole.NRegion);
            RecastUtils.PushBack((SamplePolyAreas)edges[0].W, hole.Area, ref hole.NArea);

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
                    if (hole.Indices[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        RecastUtils.PushFront(ea, hole.Indices, ref hole.NIndices);
                        RecastUtils.PushFront(r, hole.Region, ref hole.NRegion);
                        RecastUtils.PushFront(a, hole.Area, ref hole.NArea);
                        add = true;
                    }
                    else if (hole.Indices[hole.NIndices - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        RecastUtils.PushBack(eb, hole.Indices, ref hole.NIndices);
                        RecastUtils.PushBack(r, hole.Region, ref hole.NRegion);
                        RecastUtils.PushBack(a, hole.Area, ref hole.NArea);
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
        }
        private int TriangulateHole(Hole hole, out Int3[] tris)
        {
            var tverts = new Int4[hole.NIndices];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < hole.NIndices; ++i)
            {
                int pi = hole.Indices[i];
                tverts[i].X = this.Verts[pi].X;
                tverts[i].Y = this.Verts[pi].Y;
                tverts[i].Z = this.Verts[pi].Z;
                tverts[i].W = 0;
            }

            // Triangulate the hole.
            int ntris = RecastUtils.Triangulate(tverts, out _, out tris);
            if (ntris < 0)
            {
                Console.WriteLine("removeVertex: triangulate() returned bad results.");
                ntris = -ntris;
            }

            return ntris;
        }
        private int MergeHolesIntoPolygon(Int3[] tris, int ntris, Hole hole, out IndexedPolygon[] polys, out int[] regs, out SamplePolyAreas[] areas)
        {
            // Merge the hole triangles back to polygons.
            polys = new IndexedPolygon[(ntris + 1)];
            regs = new int[ntris];
            areas = new SamplePolyAreas[ntris];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; j++)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys] = new IndexedPolygon();
                    polys[npolys][0] = hole.Indices[t.X];
                    polys[npolys][1] = hole.Indices[t.Y];
                    polys[npolys][2] = hole.Indices[t.Z];

                    // If this polygon covers multiple region types then mark it as such
                    if (hole.Region[t.X] != hole.Region[t.Y] || hole.Region[t.Y] != hole.Region[t.Z])
                    {
                        regs[npolys] = RecastUtils.RC_MULTIPLE_REGS;
                    }
                    else
                    {
                        regs[npolys] = hole.Region[t.X];
                    }

                    areas[npolys] = hole.Area[t.X];
                    npolys++;
                }
            }

            return npolys;
        }
        private bool MergePolygons(IndexedPolygon[] polys, ref int npolys, int[] regs, SamplePolyAreas[] areas)
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
                    int v = pj.GetPolyMergeValue(pk, this.Verts, out int ea, out int eb);
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
                if (regs[bestPa] != regs[bestPb])
                {
                    regs[bestPa] = RecastUtils.RC_MULTIPLE_REGS;
                }
                polys[bestPb] = polys[(npolys - 1)];
                regs[bestPb] = regs[npolys - 1];
                areas[bestPb] = areas[npolys - 1];
                npolys--;
            }
            else
            {
                // Could not merge any polygons, stop.
                return false;
            }

            return true;
        }
        private void CreatePolygons(ContourSet cset, int maxVertices, int maxVertsPerCont, out int[] vflags)
        {
            vflags = new int[maxVertices];

            int nvp = this.NVP;

            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(RecastUtils.VERTEX_BUCKET_COUNT, -1);

            for (int i = 0; i < cset.NConts; ++i)
            {
                var cont = cset.Conts[i];

                // Skip null contours.
                if (cont.NVerts < 3)
                {
                    continue;
                }

                // Triangulate contour
                int ntris = RecastUtils.Triangulate(cont.Verts, out int[] indices, out Int3[] tris);
                if (ntris <= 0)
                {
                    // Bad triangulation, should not happen.
                    Console.WriteLine($"rcBuildPolyMesh: Bad triangulation Contour {i}.");
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.NVerts; j++)
                {
                    var v = cont.Verts[j];
                    indices[j] = this.AddVertex(v.X, v.Y, v.Z, firstVert, nextVert);
                    if ((v.W & RecastUtils.RC_BORDER_VERTEX) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                var polys = BuildInitialPolygons(indices, tris, ntris, maxVertsPerCont);
                if (!polys.Any())
                {
                    continue;
                }

                // Merge polygons.
                if (nvp > 3)
                {
                    polys = Merge(polys);
                }

                // Store polygons.
                StorePolygons(cont, polys);
            }
        }
        private IEnumerable<IndexedPolygon> BuildInitialPolygons(int[] indices, Int3[] tris, int ntris, int maxVertsPerCont)
        {
            List<IndexedPolygon> polys = new List<IndexedPolygon>(maxVertsPerCont);

            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    var newPoly = new IndexedPolygon(DetourUtils.DT_VERTS_PER_POLYGON);
                    newPoly[0] = indices[t.X];
                    newPoly[1] = indices[t.Y];
                    newPoly[2] = indices[t.Z];

                    polys.Add(newPoly);
                }
            }

            return polys;
        }
        private IEnumerable<IndexedPolygon> Merge(IEnumerable<IndexedPolygon> polygons)
        {
            List<IndexedPolygon> polys = polygons.ToList();

            while (true)
            {
                // Find best polygons to merge.
                int bestMergeVal = 0;
                int bestPa = 0;
                int bestPb = 0;
                int bestEa = 0;
                int bestEb = 0;

                for (int j = 0; j < polys.Count - 1; ++j)
                {
                    var pj = polys[j];
                    for (int k = j + 1; k < polys.Count; ++k)
                    {
                        var pk = polys[k];
                        int v = pj.GetPolyMergeValue(pk, this.Verts, out int ea, out int eb);
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
                    polys[bestPb] = polys.Last().Copy();
                    polys.RemoveAt(polys.Count - 1);
                }
                else
                {
                    // Could not merge any polygons, stop.
                    break;
                }
            }

            return polys;
        }
        private void StorePolygons(Contour cont, IEnumerable<IndexedPolygon> polys)
        {
            foreach (var q in polys)
            {
                IndexedPolygon p = new IndexedPolygon(this.NVP * 2); //Polygon with adjacency
                for (int k = 0; k < this.NVP; k++)
                {
                    p[k] = q[k];
                }

                this.Polys[this.NPolys] = p;
                this.Regs[this.NPolys] = cont.Region;
                this.Areas[this.NPolys] = (SamplePolyAreas)(int)cont.Area;
                this.NPolys++;

                if (this.NPolys > this.MaxPolys)
                {
                    throw new EngineException($"removeVertex: Too many polygons {this.NPolys} (max:{this.MaxPolys}).");
                }
            }
        }
        private void StorePolygons(IndexedPolygon[] polys, int npolys, int[] regs, SamplePolyAreas[] areas)
        {
            for (int i = 0; i < npolys; i++)
            {
                IndexedPolygon p = new IndexedPolygon(this.NVP * 2);
                for (int j = 0; j < this.NVP; ++j)
                {
                    p[j] = polys[i][j];
                }

                this.Polys[this.NPolys] = p;
                this.Regs[this.NPolys] = regs[i];
                this.Areas[this.NPolys] = areas[i];
                this.NPolys++;

                if (this.NPolys > this.MaxPolys)
                {
                    throw new EngineException($"removeVertex: Too many polygons {this.NPolys} (max:{this.MaxPolys}).");
                }
            }
        }
        private void RemoveEdgeVertices(int[] vflags)
        {
            for (int i = 0; i < this.NVerts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!this.CanRemoveVertex(i))
                    {
                        continue;
                    }
                    if (!this.RemoveVertex(i))
                    {
                        // Failed to remove vertex
                        throw new EngineException(string.Format("Failed to remove edge vertex {0}.", i));
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    // Fixup vertex flags
                    for (int j = i; j < this.NVerts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }
        }
        private void FindPortalEdges(ContourSet cset)
        {
            if (this.BorderSize <= 0)
            {
                return;
            }

            int w = cset.Width;
            int h = cset.Height;
            for (int i = 0; i < this.NPolys; ++i)
            {
                this.Polys[i].UpdateAdjacency(this.Verts, h, w, this.NVP);
            }
        }
        private void Remap(PolyMesh pmesh, int maxVertsPerMesh, int[] firstVert, int[] nextVert)
        {
            int ox = (int)Math.Floor((pmesh.BMin.X - this.BMin.X) / this.CS + 0.5f);
            int oz = (int)Math.Floor((pmesh.BMin.X - this.BMin.Z) / this.CS + 0.5f);

            int[] vremap = Helper.CreateArray(maxVertsPerMesh, 0);

            for (int j = 0; j < pmesh.NVerts; ++j)
            {
                var v = pmesh.Verts[j];
                vremap[j] = this.AddVertex(v.X + ox, v.Y, v.Z + oz, firstVert, nextVert);
            }

            bool isMinX = (ox == 0);
            bool isMinZ = (oz == 0);
            bool isMaxX = ((int)Math.Floor((this.BMax.X - pmesh.BMax.X) / this.CS + 0.5f)) == 0;
            bool isMaxZ = ((int)Math.Floor((this.BMax.Z - pmesh.BMax.Z) / this.CS + 0.5f)) == 0;
            bool isOnBorder = (isMinX || isMinZ || isMaxX || isMaxZ);

            for (int j = 0; j < pmesh.NPolys; ++j)
            {
                var src = pmesh.Polys[j];

                var tgt = this.Polys[this.NPolys];

                this.Regs[this.NPolys] = pmesh.Regs[j];
                this.Areas[this.NPolys] = pmesh.Areas[j];
                this.Flags[this.NPolys] = pmesh.Flags[j];
                this.NPolys++;

                for (int k = 0; k < this.NVP; ++k)
                {
                    if (src[k] == RecastUtils.RC_MESH_NULL_IDX)
                    {
                        break;
                    }
                    tgt[k] = vremap[src[k]];
                }

                if (isOnBorder)
                {
                    tgt.UpdateAdjacency(src, this.NVP, isMinX, isMaxX, isMinZ, isMaxZ);
                }
            }
        }

        /// <summary>
        /// Updates the polygon intitial flags
        /// </summary>
        public void UpdatePolyFlags()
        {
            for (int i = 0; i < this.NPolys; ++i)
            {
                if ((int)this.Areas[i] == (int)AreaTypes.Walkable)
                {
                    this.Areas[i] = SamplePolyAreas.Ground;
                }

                this.Flags[i] = QueryFilter.EvaluateArea(this.Areas[i]);
            }
        }
    }
}
