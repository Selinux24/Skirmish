using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a navigation mesh.
    /// </summary>
    class PolyMesh
    {
        /// <summary>
        /// Vertex bucket count
        /// </summary>
        const int VERTEX_BUCKET_COUNT = 1 << 12;
        /// <summary>
        /// Null index
        /// </summary>
        const int NULL_IDX = -1;
        /// <summary>
        /// Maximum number of vertices
        /// </summary>
        const int MAX_VERTICES = 0xffff;

        /// <summary>
        /// Polygon remapping parameters
        /// </summary>
        struct RemapParams
        {
            /// <summary>
            /// Is min X
            /// </summary>
            public bool IsMinX;
            /// <summary>
            /// Is min Z
            /// </summary>
            public bool IsMinZ;
            /// <summary>
            /// Is max X
            /// </summary>
            public bool IsMaxX;
            /// <summary>
            /// Is max Z
            /// </summary>
            public bool IsMaxZ;
            /// <summary>
            /// Gets whether the intex in on border
            /// </summary>
            public readonly bool IsOnBorder
            {
                get
                {
                    return IsMinX || IsMinZ || IsMaxX || IsMaxZ;
                }
            }

            /// <summary>
            /// Updates the polygon with the portal definition
            /// </summary>
            /// <param name="src">Source index list</param>
            /// <param name="vertexCount">Indexed polygon vertex count</param>
            /// <param name="vremap">Remap list</param>
            /// <param name="tgt">Target index list</param>
            public readonly void Remap(IndexedPolygon src, int vertexCount, int[] vremap, IndexedPolygon tgt)
            {
                for (int k = 0; k < vertexCount; ++k)
                {
                    if (IndexedPolygon.IndexIsNull(src[k]))
                    {
                        break;
                    }
                    tgt[k] = vremap[src[k]];
                }

                if (!IsOnBorder)
                {
                    return;
                }

                for (int k = vertexCount; k < vertexCount * 2; ++k)
                {
                    if ((src[k] & IndexedPolygon.DT_EXT_LINK) == 0 || src[k] == 0xffff)
                    {
                        continue;
                    }

                    int dir = src[k] & IndexedPolygon.PORTAL_FLAG;
                    if (RemapDirection(dir))
                    {
                        tgt[k] = src[k];
                    }
                }
            }
            /// <summary>
            /// Remap direction
            /// </summary>
            /// <param name="dir">Direction</param>
            private readonly bool RemapDirection(int dir)
            {
                bool doRemap = false;
                switch (dir)
                {
                    case 0: // Portal x-
                        if (IsMinX) doRemap = true;
                        break;
                    case 1: // Portal z+
                        if (IsMaxZ) doRemap = true;
                        break;
                    case 2: // Portal x+
                        if (IsMaxX) doRemap = true;
                        break;
                    case 3: // Portal z-
                        if (IsMinZ) doRemap = true;
                        break;
                }

                return doRemap;
            }
        }

        /// <summary>
        /// The mesh vertices. [Form: (x, y, z) * #<see cref="NVerts"/>]
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Polygon and neighbor data. [Length: #<see cref="MaxPolys"/> * 2 * #<see cref="NVP"/>]
        /// </summary>
        public IndexedPolygon[] Polys { get; set; }
        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; set; }

        /// <summary>
        /// The region id assigned to each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        public int[] Regs { get; set; }
        /// <summary>
        /// The user defined flags for each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// The area id assigned to each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }

        /// <summary>
        /// The bounds in world space.
        /// </summary>
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// The AABB border size used to generate the source data from which the mesh was derived.
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The max error of the polygon edges in the mesh.
        /// </summary>
        public float MaxEdgeError { get; set; }

        /// <summary>
        /// Builds a polygon mesh
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="nvp">Number of maximum vertex per polygon</param>
        /// <returns>Returns the new polygon mesh</returns>
        public static PolyMesh Build(ContourSet cset, int nvp)
        {
            cset.GetGeometryConfiguration(out int maxVertices, out int maxPolys, out int maxVertsPerCont);
            if (maxVertices >= MAX_VERTICES - 1)
            {
                throw new EngineException($"rcBuildPolyMesh: Too many vertices {maxVertices}.");
            }

            var mesh = new PolyMesh
            {
                NVerts = 0,
                Verts = new Int3[maxVertices],

                NPolys = 0,
                Polys = new IndexedPolygon[maxPolys],
                Regs = Helper.CreateArray(maxPolys, -1),
                Areas = Helper.CreateArray(maxPolys, SamplePolyAreas.None),
                Flags = Helper.CreateArray(maxPolys, SamplePolyFlagTypes.None),

                NVP = nvp,

                Bounds = cset.Bounds,
                CellSize = cset.CellSize,
                CellHeight = cset.CellHeight,
                BorderSize = cset.BorderSize,
                MaxEdgeError = cset.MaxError,
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
                    if ((v.Flag & ContourSet.RC_BORDER_VERTEX) != 0)
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
                var (mergedPolys, mergedNpolys) = IndexedPolygon.MergePolygons(polys, npolys, mesh.Verts);

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
        /// Merges a list of polygon meshes into a new one
        /// </summary>
        /// <param name="meshes">Polygon mesh list</param>
        /// <param name="cset">Contour set</param>
        /// <returns>Returns the new polygon mesh</returns>
        public static PolyMesh Merge(PolyMesh[] meshes, ContourSet cset)
        {
            if (!meshes.Any())
            {
                return null;
            }

            var first = meshes[0];

            var res = new PolyMesh
            {
                NVP = first.NVP,
                CellSize = first.CellSize,
                CellHeight = first.CellHeight,
                Bounds = first.Bounds,
            };

            var (maxVerts, maxPolys, maxVertsPerMesh) = UpdateBounds(meshes);

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
                int ox = (int)Math.Floor((pmesh.Bounds.Minimum.X - res.Bounds.Minimum.X) / res.CellSize + 0.5f);
                int oz = (int)Math.Floor((pmesh.Bounds.Minimum.X - res.Bounds.Minimum.Z) / res.CellSize + 0.5f);

                RemapParams remapParams = new()
                {
                    IsMinX = ox == 0,
                    IsMinZ = oz == 0,
                    IsMaxX = ((int)Math.Floor((res.Bounds.Maximum.X - pmesh.Bounds.Maximum.X) / res.CellSize + 0.5f)) == 0,
                    IsMaxZ = ((int)Math.Floor((res.Bounds.Maximum.Z - pmesh.Bounds.Maximum.Z) / res.CellSize + 0.5f)) == 0,
                };

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

                    remapParams.Remap(src, res.NVP, vremap, tgt);
                }
            }

            // Calculate adjacency.
            res.BuildMeshAdjacency(cset);

            return res;
        }
        /// <summary>
        /// Updates each mesh bounds
        /// </summary>
        /// <param name="meshes">Mesh list</param>
        private static (int MaxVerts, int MaxPolys, int MaxVersPerMesh) UpdateBounds(PolyMesh[] meshes)
        {
            var first = meshes[0];

            int maxVerts = 0;
            int maxPolys = 0;
            int maxVertsPerMesh = 0;
            foreach (var mesh in meshes)
            {
                var bmin = Vector3.Min(first.Bounds.Minimum, mesh.Bounds.Minimum);
                var bmax = Vector3.Max(first.Bounds.Maximum, mesh.Bounds.Maximum);
                mesh.Bounds = new(bmin, bmax);
                maxVertsPerMesh = Math.Max(maxVertsPerMesh, mesh.NVerts);
                maxVerts += mesh.NVerts;
                maxPolys += mesh.NPolys;
            }

            return (maxVerts, maxPolys, maxVertsPerMesh);
        }

        /// <summary>
        /// Build polygon vertices
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        public Vector3[] BuildPolyVertices(IndexedPolygon p)
        {
            var res = new List<Vector3>();

            float cs = CellSize;
            float ch = CellHeight;

            for (int j = 0; j < NVP; ++j)
            {
                if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX)
                {
                    break;
                }

                var v = Verts[p[j]];
                var pv = new Vector3(v.X * cs, v.Y * ch, v.Z * cs);

                res.Add(pv);
            }

            return res.ToArray();
        }
        /// <summary>
        /// Finds polygon bounds
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        public (RectBounds[] Bounds, int MaxHWidth, int MaxHHeight) FindBounds(CompactHeightfield chf)
        {
            var bounds = new List<RectBounds>();

            int nPolyVerts = 0;
            int maxhw = 0;
            int maxhh = 0;

            // Find max size for a polygon area.
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];

                var (b, PolyVerts) = FindMaxSizeArea(chf, p);

                bounds.Add(b);
                nPolyVerts += PolyVerts;

                // Try to store max size
                if (b.Min.X >= b.Max.X || b.Min.Y >= b.Max.Y)
                {
                    continue;
                }

                maxhw = Math.Max(maxhw, b.Max.X - b.Min.X);
                maxhh = Math.Max(maxhh, b.Max.Y - b.Min.Y);
            }

            return (bounds.ToArray(), maxhw, maxhh);
        }
        /// <summary>
        /// Finds maximum area size of polygon
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="p">Indexed polygon</param>
        private (RectBounds bounds, int PolyVerts) FindMaxSizeArea(CompactHeightfield chf, IndexedPolygon p)
        {
            int xmin = chf.Width;
            int xmax = 0;
            int ymin = chf.Height;
            int ymax = 0;
            int polyVerts = 0;

            for (int j = 0; j < NVP; ++j)
            {
                if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX)
                {
                    break;
                }

                var v = Verts[p[j]];
                xmin = Math.Min(xmin, v.X);
                xmax = Math.Max(xmax, v.X);
                ymin = Math.Min(ymin, v.Z);
                ymax = Math.Max(ymax, v.Z);
                polyVerts++;
            }
            xmin = Math.Max(0, xmin - 1);
            xmax = Math.Min(chf.Width, xmax + 1);
            ymin = Math.Max(0, ymin - 1);
            ymax = Math.Min(chf.Height, ymax + 1);

            RectBounds b = new(xmin, ymin, xmax, ymax);

            return (b, polyVerts);
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
        /// <param name="maxPolys">Maximum number of polygons</param>
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
        private bool CanRemoveVertex(int rem)
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

            // Find edges which share the removed vertex.
            int maxEdges = numTouchedVerts * 2;
            int nedges = 0;
            IndexedEdge[] edges = new IndexedEdge[maxEdges];

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
            int numOpenEdges = IndexedEdge.CountOpenEdges(edges, nedges);

            return numOpenEdges <= 2;
        }

        /// <summary>
        /// Removes the specified vertex
        /// </summary>
        /// <param name="rem">Vertex to remove</param>
        /// <param name="maxPolys">Maxmimum number of polygons</param>
        /// <returns>Returns true if the vertex were removed</returns>
        private bool RemoveVertex(int rem, int maxPolys)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = IndexedPolygon.CountPolygonsToRemove(Polys, NPolys, rem);

            var (edges, nedges) = GenerateRemoveEdges(numRemovedVerts, rem);
            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            var (tverts, thole, hole, harea, hreg) = GetTriangulateHole(edges, ref nedges, numRemovedVerts);

            // Triangulate the hole.
            int ntris = TriangulationHelper.Triangulate(tverts, ref thole, out var tris);
            if (ntris < 0)
            {
                Logger.WriteWarning(this, "removeVertex: Hole triangulation error");

                ntris = -ntris;
            }

            // Merge the hole triangles back to polygons.
            var (polys, npolys, pareas, pregs) = IndexedPolygon.BuildRemoveInitialPolygons(tris, ntris, hole, harea, hreg);
            if (npolys == 0)
            {
                return true;
            }

            if (NVP > 3)
            {
                // Merge polygons.
                var (mergedPolys, mergedNPolys, mergedAreas, mergedRegs) = IndexedPolygon.MergePolygons(polys, npolys, pareas, pregs, Verts);
                polys = mergedPolys;
                npolys = mergedNPolys;
                pareas = mergedAreas;
                pregs = mergedRegs;
            }

            // Store polygons.
            if (!StorePolygons(polys, npolys, pareas, pregs, maxPolys))
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
        /// <returns>Returns the edge list</returns>
        private (IndexedRegionEdge[] Edges, int NEdges) GenerateRemoveEdges(int numRemovedVerts, int rem)
        {
            var edges = new IndexedRegionEdge[numRemovedVerts * NVP];
            int nedges = 0;
            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];

                if (!p.Contains(rem))
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

                    edges[nedges++] = new(p[k], p[j], Regs[i], Areas[i]);
                }

                // Remove the polygon.
                RemovePolygon(i);

                --i;
            }

            // Remove vertex.
            RemoveVertex(rem);

            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].EdgeIndexA > rem) edges[i].EdgeIndexA--;
                if (edges[i].EdgeIndexB > rem) edges[i].EdgeIndexB--;
            }

            return (edges, nedges);
        }
        /// <summary>
        /// Removes the specified polygon by index
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemovePolygon(int rem)
        {
            Polys[rem] = Polys[NPolys - 1];
            Regs[rem] = Regs[NPolys - 1];
            Areas[rem] = Areas[NPolys - 1];

            Polys[NPolys - 1] = null;
            Regs[NPolys - 1] = NULL_IDX;
            Areas[NPolys - 1] = SamplePolyAreas.None;

            NPolys--;
        }
        /// <summary>
        /// Removes the specified vertex by index, and adjusts the indexed polygon layout
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemoveVertex(int rem)
        {
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
        }
        /// <summary>
        /// Gets the triangulated hole
        /// </summary>
        /// <param name="edges">Edge list</param>
        /// <param name="nedges">Number of edges</param>
        /// <param name="hole">Hole indices</param>
        /// <param name="hreg">Hole regions</param>
        /// <param name="harea">Hole areas</param>
        /// <returns>Retusn the triangulated vertices and the triangulated hole</returns>
        private (Int3[] TriVerts, int[] TriHole, int[] hole, SamplePolyAreas[] harea, int[] hreg) GetTriangulateHole(IndexedRegionEdge[] edges, ref int nedges, int numRemovedVerts)
        {
            var hole = new int[numRemovedVerts * NVP];
            var harea = new SamplePolyAreas[numRemovedVerts * NVP];
            var hreg = new int[numRemovedVerts * NVP];

            int nhole = 0;
            int nhreg = 0;
            int nharea = 0;

            Utils.PushBack(edges[0].EdgeIndexA, hole, ref nhole);
            Utils.PushBack(edges[0].Area, harea, ref nharea);
            Utils.PushBack(edges[0].Region, hreg, ref nhreg);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].EdgeIndexA;
                    int eb = edges[i].EdgeIndexB;
                    var a = edges[i].Area;
                    int r = edges[i].Region;

                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        Utils.PushFront(ea, hole, ref nhole);
                        Utils.PushFront(a, harea, ref nharea);
                        Utils.PushFront(r, hreg, ref nhreg);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        Utils.PushBack(eb, hole, ref nhole);
                        Utils.PushBack(a, harea, ref nharea);
                        Utils.PushBack(r, hreg, ref nhreg);
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

            return (tverts, thole, hole, harea, hreg);
        }

        /// <summary>
        /// Builds the mesh adjacency
        /// <param name="cset">Contour set</param>
        /// </summary>
        private void BuildMeshAdjacency(ContourSet cset)
        {
            var polys = Polys;
            int npolys = NPolys;
            int nverts = NVerts;
            int vertsPerPoly = NVP;

            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            var (edges, edgeCount) = IndexedPolygon.BuildAdjacencyEdges(polys, npolys, vertsPerPoly, nverts, false, 0);

            // Find portal edges
            FindPortalEdges(cset);

            // Store adjacency
            IndexedPolygon.StoreAdjacency(polys, vertsPerPoly, edges, edgeCount, false, 0);
        }
        /// <summary>
        /// Finds edges between portals
        /// </summary>
        /// <param name="cset">Contour set</param>
        private void FindPortalEdges(ContourSet cset)
        {
            if (BorderSize <= 0)
            {
                return;
            }

            int w = cset.Width;
            int h = cset.Height;

            for (int i = 0; i < NPolys; ++i)
            {
                var p = Polys[i];

                for (int j = 0; j < NVP; ++j)
                {
                    if (IndexedPolygon.IndexIsNull(p[j]))
                    {
                        break;
                    }

                    // Skip connected edges.
                    if (!IndexedPolygon.IndexIsNull(p[NVP + j]))
                    {
                        continue;
                    }

                    int nj = j + 1;
                    if (nj >= NVP || IndexedPolygon.IndexIsNull(p[nj]))
                    {
                        nj = 0;
                    }

                    var va = Verts[p[j]];
                    var vb = Verts[p[nj]];

                    if (IndexedPolygon.IsPortal(va, vb, w, h, out int v))
                    {
                        p[NVP + j] = v;
                    }
                }
            }
        }

        /// <summary>
        /// Stores the polygon list into the mesh
        /// </summary>
        /// <param name="polys">Polygon index list</param>
        /// <param name="npolys">Number of polygon indices</param>
        /// <param name="pregs">Region id list</param>
        /// <param name="pareas">Area list</param>
        /// <param name="maxPolys">Maximum number of polygons to store</param>
        private bool StorePolygons(IndexedPolygon[] polys, int npolys, SamplePolyAreas[] pareas, int[] pregs, int maxPolys)
        {
            for (int i = 0; i < npolys; ++i)
            {
                if (npolys > maxPolys)
                {
                    return false;
                }

                //Polygon with adjacency
                var p = new IndexedPolygon(NVP * 2);
                p.CopyVertices(polys[i], NVP);

                StorePolygon(p, pareas[i], pregs[i]);
            }

            return true;
        }
        /// <summary>
        /// Stores the polygon list into the mesh
        /// </summary>
        /// <param name="polys">Polygon index list</param>
        /// <param name="npolys">Number of polygon indices</param>
        /// <param name="cont">Contour data</param>
        /// <param name="maxPolys">Maximum number of polygons to store</param>
        private bool StorePolygons(IndexedPolygon[] polys, int npolys, Contour cont, int maxPolys)
        {
            for (int i = 0; i < npolys; ++i)
            {
                if (npolys > maxPolys)
                {
                    return false;
                }

                //Polygon with adjacency
                var p = new IndexedPolygon(NVP * 2);
                p.CopyVertices(polys[i], NVP);

                StorePolygon(p, (SamplePolyAreas)(int)cont.Area, cont.RegionId);
            }

            return true;
        }
        /// <summary>
        /// Stores a plygon into the mesh
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        /// <param name="reg">Region id</param>
        /// <param name="area">Area type</param>
        private void StorePolygon(IndexedPolygon p, SamplePolyAreas area, int reg)
        {
            Polys[NPolys] = p;
            Areas[NPolys] = area;
            Regs[NPolys] = reg;
            NPolys++;
        }

        /// <summary>
        /// Updates the polygon flags
        /// </summary>
        public void UpdatePolyFlags()
        {
            for (int i = 0; i < NPolys; ++i)
            {
                if ((int)Areas[i] == (int)AreaTypes.RC_WALKABLE_AREA)
                {
                    Areas[i] = SamplePolyAreas.Ground;
                }

                Flags[i] = SamplePolyFlagTypesExtents.EvaluateArea(Areas[i]);
            }
        }
    }
}
