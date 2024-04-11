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
                    if (src.VertexIsNull(k))
                    {
                        break;
                    }
                    tgt.SetVertex(k, vremap[src.GetVertex(k)]);
                }

                if (!IsOnBorder)
                {
                    return;
                }

                for (int k = 0; k < vertexCount; ++k)
                {
                    if (!src.IsExternalLink(k) || src.GetVertex(k) == 0xffff)
                    {
                        continue;
                    }

                    int dir = src.GetDirection(k);
                    if (RemapDirection(dir))
                    {
                        tgt.SetAdjacency(k, src.GetAdjacency(k));
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
        /// The number of vertices.
        /// </summary>
        private int vertCount;
        /// <summary>
        /// The mesh vertices. [Form: (x, y, z) * #<see cref="vertCount"/>]
        /// </summary>
        private Int3[] vertList;
        /// <summary>
        /// The number of polygons.
        /// </summary>
        private int polyCount;
        /// <summary>
        /// Polygon and neighbor data. [Length: #<see cref="MaxPolys"/> * 2 * #<see cref="NVP"/>]
        /// </summary>
        private IndexedPolygon[] polyList;
        /// <summary>
        /// The region id assigned to each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        private int[] regionList;
        /// <summary>
        /// The area id assigned to each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        private SamplePolyAreas[] areaList;
        /// <summary>
        /// The user defined flags for each polygon. [Length: #<see cref="MaxPolys"/>]
        /// </summary>
        private SamplePolyFlagTypes[] flagList;

        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; private set; }
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
            cset.GetGeometryConfiguration(out int maxVertices, out int maxPolys, out _);
            if (maxVertices >= MAX_VERTICES - 1)
            {
                throw new EngineException($"rcBuildPolyMesh: Too many vertices {maxVertices}.");
            }

            var mesh = new PolyMesh
            {
                vertCount = 0,
                vertList = new Int3[maxVertices],

                polyCount = 0,
                polyList = new IndexedPolygon[maxPolys],
                regionList = Helper.CreateArray(maxPolys, -1),
                areaList = Helper.CreateArray(maxPolys, SamplePolyAreas.None),
                flagList = Helper.CreateArray(maxPolys, SamplePolyFlagTypes.None),

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
                for (int j = 0; j < nverts; ++j)
                {
                    var cv = cont.GetVertex(j);

                    indices[j] = mesh.AddVertex(cv.X, cv.Y, cv.Z, firstVert, nextVert);
                    if (Contour.IsBorderVertex(cv.Flag))
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

                // Merge polygons.
                var mergedPolys = IndexedPolygon.MergePolygons(polys, mesh.vertList);

                // Store polygons.
                if (!mesh.StorePolygons(mergedPolys, cont, maxPolys))
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
        /// Merges a list of polygon meshes into a new one
        /// </summary>
        /// <param name="meshes">Polygon mesh list</param>
        /// <param name="cset">Contour set</param>
        /// <returns>Returns the new polygon mesh</returns>
        public static PolyMesh Merge(PolyMesh[] meshes, ContourSet cset)
        {
            if (meshes.Length == 0)
            {
                return null;
            }

            var first = meshes[0];

            var mesh = new PolyMesh
            {
                NVP = first.NVP,
                CellSize = first.CellSize,
                CellHeight = first.CellHeight,
                Bounds = first.Bounds,
            };

            var (maxVerts, maxPolys, maxVertsPerMesh) = UpdateBounds(meshes);

            mesh.vertCount = 0;
            mesh.vertList = new Int3[maxVerts];
            mesh.polyCount = 0;
            mesh.polyList = new IndexedPolygon[maxPolys];
            mesh.regionList = new int[maxPolys];
            mesh.areaList = new SamplePolyAreas[maxPolys];
            mesh.flagList = new SamplePolyFlagTypes[maxPolys];

            int[] nextVert = Helper.CreateArray(maxVerts, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, -1);
            int[] vremap = Helper.CreateArray(maxVertsPerMesh, 0);

            foreach (var pmesh in meshes)
            {
                int ox = (int)MathF.Floor((pmesh.Bounds.Minimum.X - mesh.Bounds.Minimum.X) / mesh.CellSize + 0.5f);
                int oz = (int)MathF.Floor((pmesh.Bounds.Minimum.X - mesh.Bounds.Minimum.Z) / mesh.CellSize + 0.5f);

                RemapParams remapParams = new()
                {
                    IsMinX = ox == 0,
                    IsMinZ = oz == 0,
                    IsMaxX = ((int)MathF.Floor((mesh.Bounds.Maximum.X - pmesh.Bounds.Maximum.X) / mesh.CellSize + 0.5f)) == 0,
                    IsMaxZ = ((int)MathF.Floor((mesh.Bounds.Maximum.Z - pmesh.Bounds.Maximum.Z) / mesh.CellSize + 0.5f)) == 0,
                };

                for (int j = 0; j < pmesh.vertCount; ++j)
                {
                    var v = pmesh.vertList[j];
                    vremap[j] = mesh.AddVertex(v.X + ox, v.Y, v.Z + oz, firstVert, nextVert);
                }

                for (int j = 0; j < pmesh.polyCount; ++j)
                {
                    var tgt = mesh.polyList[mesh.polyCount];
                    var src = pmesh.polyList[j];
                    mesh.regionList[mesh.polyCount] = pmesh.regionList[j];
                    mesh.areaList[mesh.polyCount] = pmesh.areaList[j];
                    mesh.flagList[mesh.polyCount] = pmesh.flagList[j];
                    mesh.polyCount++;

                    remapParams.Remap(src, mesh.NVP, vremap, tgt);
                }
            }

            // Calculate adjacency.
            mesh.BuildMeshAdjacency(cset);

            return mesh;
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
                maxVertsPerMesh = Math.Max(maxVertsPerMesh, mesh.vertCount);
                maxVerts += mesh.vertCount;
                maxPolys += mesh.polyCount;
            }

            return (maxVerts, maxPolys, maxVertsPerMesh);
        }

        /// <summary>
        /// Build polygon vertices
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        public Vector3[] BuildPolyVertices(IndexedPolygon p)
        {
            List<Vector3> res = [];

            float cs = CellSize;
            float ch = CellHeight;

            for (int j = 0; j < NVP; ++j)
            {
                if (p.VertexIsNull(j))
                {
                    break;
                }

                var v = vertList[p.GetVertex(j)];
                var pv = new Vector3(v.X * cs, v.Y * ch, v.Z * cs);

                res.Add(pv);
            }

            return [.. res];
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
            for (int i = 0; i < polyCount; ++i)
            {
                var p = polyList[i];

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
                if (p.VertexIsNull(j))
                {
                    break;
                }

                var v = vertList[p.GetVertex(j)];
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
        /// <param name="maxPolys">Maximum number of polygons</param>
        private void RemoveEdgeVertices(bool[] vflags, int maxPolys)
        {
            int i = 0;
            while (i < vertCount)
            {
                if (!vflags[i])
                {
                    i++;
                    continue;
                }

                if (!CanRemoveVertex(i))
                {
                    i++;
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
            var (numTouchedVerts, numRemainingEdges) = IndexedPolygon.CountVertexToRemove(polyList, polyCount, rem);

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

            for (int i = 0; i < polyCount; ++i)
            {
                var p = polyList[i];

                // Collect edges which touches the removed vertex.
                foreach (var iter in p.IterateVertices())
                {
                    int a = iter.a;
                    int b = iter.b;

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
        /// <param name="maxPolys">Maxmimum number of polygons</param>
        /// <returns>Returns true if the vertex were removed</returns>
        private bool RemoveVertex(int rem, int maxPolys)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = IndexedPolygon.CountPolygonsToRemove(this.polyList, polyCount, rem);

            var (edges, nedges) = GenerateRemoveEdges(numRemovedVerts, rem);
            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            var (tverts, thole, hole, harea, hreg) = GetTriangulateHole(edges, ref nedges, numRemovedVerts);

            // Triangulate the hole.
            var (triRes, tris) = TriangulationHelper.Triangulate(tverts, thole);
            if (!triRes)
            {
                Logger.WriteWarning(this, "removeVertex: Hole triangulation error");
            }

            // Merge the hole triangles back to polygons.
            var (polys, pareas, pregs) = IndexedPolygon.CreateInitialPolygons(hole, tris, harea, hreg);
            if (polys.Length == 0)
            {
                return true;
            }

            if (NVP > 3)
            {
                // Merge polygons.
                var (mergedPolys, mergedAreas, mergedRegs) = IndexedPolygon.MergePolygons(polys, pareas, pregs, vertList);
                polys = mergedPolys;
                pareas = mergedAreas;
                pregs = mergedRegs;
            }

            // Store polygons.
            if (!StorePolygons(polys, polys.Length, pareas, pregs, maxPolys))
            {
                Logger.WriteWarning(this, $"removeVertex: Too many polygons {polyCount} (max:{maxPolys}).");

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
            int i = 0;
            while (i < polyCount)
            {
                var p = polyList[i];

                if (!p.ContainsVertex(rem))
                {
                    i++;
                    continue;
                }

                // Collect edges which does not touch the removed vertex.
                foreach (var (a, b, _, _) in p.IterateVertices())
                {
                    if (a == rem || b == rem)
                    {
                        continue;
                    }

                    edges[nedges++] = new(b, a, regionList[i], areaList[i]);
                }

                // Remove the polygon.
                RemovePolygon(i);
            }

            // Remove vertex.
            RemoveVertexIndex(rem);

            for (int j = 0; j < nedges; ++j)
            {
                if (edges[j].EdgeIndexA > rem) edges[j].EdgeIndexA--;
                if (edges[j].EdgeIndexB > rem) edges[j].EdgeIndexB--;
            }

            return (edges, nedges);
        }
        /// <summary>
        /// Removes the specified polygon by index
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemovePolygon(int rem)
        {
            polyList[rem] = polyList[polyCount - 1];
            regionList[rem] = regionList[polyCount - 1];
            areaList[rem] = areaList[polyCount - 1];

            polyList[polyCount - 1] = null;
            regionList[polyCount - 1] = NULL_IDX;
            areaList[polyCount - 1] = SamplePolyAreas.None;

            polyCount--;
        }
        /// <summary>
        /// Removes the specified vertex by index, and adjusts the indexed polygon layout
        /// </summary>
        /// <param name="rem">Index to remove</param>
        private void RemoveVertexIndex(int rem)
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

                foreach (var (a, _, j, _) in p.IterateVertices())
                {
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

            ArrayUtils.PushBack(edges[0].EdgeIndexA, hole, ref nhole);
            ArrayUtils.PushBack(edges[0].Area, harea, ref nharea);
            ArrayUtils.PushBack(edges[0].Region, hreg, ref nhreg);

            while (nedges != 0)
            {
                bool match = false;

                int i = 0;
                while (i < nedges)
                {
                    int ea = edges[i].EdgeIndexA;
                    int eb = edges[i].EdgeIndexB;
                    var a = edges[i].Area;
                    int r = edges[i].Region;

                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        ArrayUtils.PushFront(ea, hole, ref nhole);
                        ArrayUtils.PushFront(a, harea, ref nharea);
                        ArrayUtils.PushFront(r, hreg, ref nhreg);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        ArrayUtils.PushBack(eb, hole, ref nhole);
                        ArrayUtils.PushBack(a, harea, ref nharea);
                        ArrayUtils.PushBack(r, hreg, ref nhreg);
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

                    i++;
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
                tverts[i].X = vertList[pi].X;
                tverts[i].Y = vertList[pi].Y;
                tverts[i].Z = vertList[pi].Z;
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
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            var (edges, edgeCount) = IndexedPolygon.BuildAdjacencyEdges(polyList, polyCount, vertCount, false, 0);

            // Store adjacency
            IndexedPolygon.StoreAdjacency(polyList, edges, edgeCount, false, 0);

            if (BorderSize > 0)
            {
                // Find portal edges
                FindPortalEdges(cset);
            }
        }
        /// <summary>
        /// Finds edges between portals
        /// </summary>
        /// <param name="cset">Contour set</param>
        private void FindPortalEdges(ContourSet cset)
        {
            int w = cset.Width;
            int h = cset.Height;

            foreach (var (p, _, j) in IndexedPolygon.IteratePolygonVertices(polyList, polyCount))
            {
                // Skip connected edges.
                if (!p.AdjacencyIsNull(j))
                {
                    continue;
                }

                var va = vertList[p.GetVertex(j)];
                var vb = vertList[p.GetNextVertex(j)];

                if (Edge.IsPortal(va, vb, w, h, out int v))
                {
                    p.SetAdjacency(j, v);
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
                var p = new IndexedPolygon(NVP, true);
                p.CopyVertices(polys[i]);

                StorePolygon(p, pareas[i], pregs[i]);
            }

            return true;
        }
        /// <summary>
        /// Stores the polygon list into the mesh
        /// </summary>
        /// <param name="polys">Polygon index list</param>
        /// <param name="cont">Contour data</param>
        /// <param name="maxPolys">Maximum number of polygons to store</param>
        private bool StorePolygons(IndexedPolygon[] polys, Contour cont, int maxPolys)
        {
            if (polys.Length > maxPolys)
            {
                return false;
            }

            for (int i = 0; i < polys.Length; ++i)
            {
                //Polygon with adjacency
                var p = new IndexedPolygon(NVP, true);
                p.CopyVertices(polys[i]);

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
            polyList[polyCount] = p;
            areaList[polyCount] = area;
            regionList[polyCount] = reg;
            polyCount++;
        }

        /// <summary>
        /// Updates the polygon flags
        /// </summary>
        public void UpdatePolyFlags()
        {
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
        /// Iterates over the mesh polygons
        /// </summary>
        public IEnumerable<(int i, IndexedPolygon p, int r, SamplePolyAreas a)> IteratePolygons()
        {
            for (int p = 0; p < polyCount; p++)
            {
                yield return (p, polyList[p], regionList[p], areaList[p]);
            }
        }
        /// <summary>
        /// Iterates over the mesh segments
        /// </summary>
        /// <returns>Returns the polygon, and the two vertices of each segment</returns>
        public IEnumerable<(int i0, int i1, IndexedPolygon p)> IteratePolySegments()
        {
            for (int i = 0; i < polyCount; i++)
            {
                var p = polyList[i];

                for (int i0 = 0; i0 < p.Capacity; i0++)
                {
                    if (p.VertexIsNull(i0))
                    {
                        continue;
                    }

                    int i1 = p.GetNextIndex(i0);

                    yield return (i0, i1, p);
                }
            }
        }
        /// <summary>
        /// Iterates over the mesh triangles
        /// </summary>
        /// <returns>Returns the polygon index, the polygon, and the three vertices of each triangle</returns>
        public IEnumerable<(int i, Int3[] tri, IndexedPolygon p, int r, SamplePolyAreas a)> IteratePolyTriangles()
        {
            for (int i = 0; i < polyCount; i++)
            {
                var p = polyList[i];

                for (int iv = 2; iv < p.Capacity; iv++)
                {
                    if (p.VertexIsNull(iv))
                    {
                        continue;
                    }

                    int p0 = p.GetVertex(0);
                    int p1 = p.GetVertex(iv - 1);
                    int p2 = p.GetVertex(iv);
                    int[] vi = [p0, p1, p2];
                    var tri = vi.Select(GetVertex).ToArray();

                    yield return (i, tri, p, regionList[i], areaList[i]);
                }
            }
        }

        /// <summary>
        /// Gets the number of vertices
        /// </summary>
        public int GetVertexCount()
        {
            return vertCount;
        }
        /// <summary>
        /// Return vertices
        /// </summary>
        public Int3[] GetVertices()
        {
            return [.. vertList];
        }
        /// <summary>
        /// Gets the vertex at index
        /// </summary>
        /// <param name="index">Index</param>
        public Int3 GetVertex(int index)
        {
            return vertList[index];
        }

        /// <summary>
        /// Gets the number of polygons
        /// </summary>
        public int GetPolyCount()
        {
            return polyCount;
        }
        /// <summary>
        /// Return polygons
        /// </summary>
        public IndexedPolygon[] GetPolygons()
        {
            return [.. polyList];
        }
        /// <summary>
        /// Gets the polygon at index
        /// </summary>
        /// <param name="index">Index</param>
        public IndexedPolygon GetPolygon(int index)
        {
            return polyList[index];
        }

        /// <summary>
        /// Return regions
        /// </summary>
        public int[] GetRegions()
        {
            return [.. regionList];
        }
        /// <summary>
        /// Return Areas
        /// </summary>
        public SamplePolyAreas[] GetAreas()
        {
            return [.. areaList];
        }
        /// <summary>
        /// Return flags
        /// </summary>
        public SamplePolyFlagTypes[] GetFlags()
        {
            return [.. flagList];
        }
    }
}
