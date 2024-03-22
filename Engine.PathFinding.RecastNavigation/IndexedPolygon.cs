using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Indexed polygon
    /// </summary>
    [Serializable]
    public class IndexedPolygon
    {
        /// <summary>
        /// The maximum number of vertices per navigation polygon.
        /// </summary>
        public const int DT_VERTS_PER_POLYGON = 6;
        /// <summary>
        /// Polygon touches multiple regions.
        /// If a polygon has this region ID it was merged with or created
        /// from polygons of different regions during the polymesh
        /// build step that removes redundant border vertices. 
        /// (Used during the polymesh and detail polymesh build processes)
        /// </summary>
        const int RC_MULTIPLE_REGS = 0;
        /// <summary>
        /// An value which indicates an invalid index within a mesh.
        /// </summary>
        const int RC_MESH_NULL_IDX = -1;

        /// <summary>
        /// Adjacency edge list helper struct
        /// </summary>
        struct AdjacencyEdgeHelper
        {
            /// <summary>
            /// Edge list
            /// </summary>
            public Edge[] Edges;
            /// <summary>
            /// Number of edges in the list
            /// </summary>
            public int EdgeCount;
            /// <summary>
            /// First edge index list
            /// </summary>
            public int[] FirstEdge;
            /// <summary>
            /// Next edge index list
            /// </summary>
            public int[] NextEdge;
        }

        /// <summary>
        /// Polygon capacity
        /// </summary>
        public int Capacity { get; private set; }
        /// <summary>
        /// Vertex indices
        /// </summary>
        private readonly int[] vertices;
        /// <summary>
        /// Uses adjacency flag
        /// </summary>
        public bool UseAdjacency { get; private set; }
        /// <summary>
        /// Adjacency flags
        /// </summary>
        private readonly int[] adjacency;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Polygon capacity</param>
        /// <param name="useAdjacency">Use adjacency</param>
        public IndexedPolygon(int capacity, bool useAdjacency = false)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

            Capacity = capacity;
            UseAdjacency = useAdjacency;
            vertices = Helper.CreateArray(capacity, RC_MESH_NULL_IDX);
            adjacency = useAdjacency ? Helper.CreateArray(capacity, RC_MESH_NULL_IDX) : [];
        }

        /// <summary>
        /// Gets the best polygon merge value between two polygons
        /// </summary>
        /// <param name="pa">First polygon</param>
        /// <param name="pb">Second polygon</param>
        /// <param name="verts">Vertices</param>
        /// <param name="ea">Resulting first merge value</param>
        /// <param name="eb">Resulting second merge value</param>
        /// <returns>Returns the best merge value</returns>
        public static int GetMergeValue(IndexedPolygon pa, IndexedPolygon pb, Int3[] verts, out int ea, out int eb)
        {
            if (!PolygonsCanMerge(pa, pb, out ea, out eb))
            {
                return RC_MESH_NULL_IDX;
            }

            int na = pa.CountPolyVerts();
            int nb = pb.CountPolyVerts();

            // Check to see if the merged polygon would be convex.
            int va, vb, vc;

            va = pa.vertices[(ea + na - 1) % na];
            vb = pa.vertices[ea];
            vc = pb.vertices[(eb + 2) % nb];
            if (!ULeft2D(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pb.vertices[(eb + nb - 1) % nb];
            vb = pb.vertices[eb];
            vc = pa.vertices[(ea + 2) % na];
            if (!ULeft2D(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pa.vertices[ea];
            vb = pa.vertices[(ea + 1) % na];
            int dx = verts[va].X - verts[vb].X;
            int dy = verts[va].Z - verts[vb].Z;

            return dx * dx + dy * dy;
        }
        /// <summary>
        /// Gets whether two polygons can merge
        /// </summary>
        /// <param name="pa">First polygon</param>
        /// <param name="pb">Second polygon</param>
        /// <param name="ea">Resulting first merge value</param>
        /// <param name="eb">Resulting second merge value</param>
        /// <returns>Returns whether two polygons can merge, and the best edge merge indices of each polygon</returns>
        private static bool PolygonsCanMerge(IndexedPolygon pa, IndexedPolygon pb, out int ea, out int eb)
        {
            int na = pa.CountPolyVerts();
            int nb = pb.CountPolyVerts();

            if (na + nb - 2 > DT_VERTS_PER_POLYGON)
            {
                // If the merged polygon would be too big, do not merge.
                ea = RC_MESH_NULL_IDX;
                eb = RC_MESH_NULL_IDX;

                return false;
            }

            ea = RC_MESH_NULL_IDX;
            eb = RC_MESH_NULL_IDX;

            // Check if the polygons share an edge.
            for (int i = 0; i < na; ++i)
            {
                int va0 = pa.vertices[i];
                int va1 = pa.vertices[(i + 1) % na];

                if (va0 > va1)
                {
                    Helper.Swap(ref va0, ref va1);
                }

                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = pb.vertices[j];
                    int vb1 = pb.vertices[(j + 1) % nb];

                    if (vb0 > vb1)
                    {
                        Helper.Swap(ref vb0, ref vb1);
                    }

                    if (va0 == vb0 && va1 == vb1)
                    {
                        ea = i;
                        eb = j;
                        break;
                    }
                }
            }

            if (ea == RC_MESH_NULL_IDX || eb == RC_MESH_NULL_IDX)
            {
                // No common edge, cannot merge.
                ea = RC_MESH_NULL_IDX;
                eb = RC_MESH_NULL_IDX;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Merges the polygon list with their regions and areas
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="pregs">Region list</param>
        /// <param name="pareas">Area list</param>
        /// <param name="verts">Polygon vertex list</param>
        /// <returns>Returns the resulting merged polygon</returns>
        public static (IndexedPolygon[] Polys, SamplePolyAreas[] PAreas, int[] PRegs) MergePolygons(IndexedPolygon[] polys, SamplePolyAreas[] pareas, int[] pregs, Int3[] verts)
        {
            if ((polys?.Length ?? 0) == 0)
            {
                return ([], [], []);
            }

            bool procAreas = (pareas?.Length ?? 0) != 0;
            bool procRegs = (pregs?.Length ?? 0) != 0;

            var mergedNpolys = polys.Length;
            var mergedPolys = polys.ToArray();
            var mergedareas = pareas?.ToArray() ?? [];
            var mergedregs = pregs?.ToArray() ?? [];

            while (true)
            {
                // Find best polygons to merge.
                var (bestMergeVal, bestPa, bestPb, bestEa, bestEb) = GetBestMergePolygon(mergedPolys, mergedNpolys, verts);
                if (bestMergeVal <= 0)
                {
                    // Could not merge any polygons, stop.
                    break;
                }

                // Found best, merge.
                mergedPolys[bestPa] = Merge(mergedPolys[bestPa], mergedPolys[bestPb], bestEa, bestEb);
                mergedPolys[bestPb] = mergedPolys[mergedNpolys - 1].Copy();

                if (procAreas)
                {
                    mergedareas[bestPb] = mergedareas[mergedNpolys - 1];
                }

                if (procRegs)
                {
                    if (mergedregs[bestPa] != mergedregs[bestPb])
                    {
                        mergedregs[bestPa] = RC_MULTIPLE_REGS;
                    }
                    mergedregs[bestPb] = mergedregs[mergedNpolys - 1];
                }

                mergedNpolys--;
            }

            // Cut to mergedNpolys
            mergedPolys = Helper.Truncate(mergedPolys, mergedNpolys);
            mergedareas = Helper.Truncate(mergedareas, mergedNpolys);
            mergedregs = Helper.Truncate(mergedregs, mergedNpolys);

            return (mergedPolys, mergedareas, mergedregs);
        }
        /// <summary>
        /// Merges the polygon list
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="verts">Polygon vertex list</param>
        /// <returns>Returns the resulting merged polygon</returns>
        public static IndexedPolygon[] MergePolygons(IndexedPolygon[] polys, Int3[] verts)
        {
            var (mergedPolys, _, _) = MergePolygons(polys, null, null, verts);

            return mergedPolys;
        }
        /// <summary>
        /// Creates the initial polygon list from a triangle definition
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="indices">Triangle indices</param>
        /// <param name="hreg">Region id list</param>
        /// <param name="harea">Area list</param>
        /// <returns>Returns the indexed polygons, regions and areas</returns>
        public static (IndexedPolygon[] Polys, SamplePolyAreas[] PAreas, int[] PRegs) CreateInitialPolygons(int[] indices, Int3[] tris, SamplePolyAreas[] harea, int[] hreg)
        {
            if ((tris?.Length ?? 0) == 0)
            {
                return ([], [], []);
            }

            bool procAreas = (harea?.Length ?? 0) != 0;
            bool procRegs = (hreg?.Length ?? 0) != 0;

            // Merge the hole triangles back to polygons.
            int ntris = tris.Length;
            var polys = new IndexedPolygon[ntris];
            var pareas = new SamplePolyAreas[ntris];
            var pregs = new int[ntris];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];

                if (!ValidateIndex(t))
                {
                    continue;
                }

                polys[npolys] = new(3, false);
                polys[npolys].SetVertex(0, indices[t.X]);
                polys[npolys].SetVertex(1, indices[t.Y]);
                polys[npolys].SetVertex(2, indices[t.Z]);

                if (procAreas)
                {
                    pareas[npolys] = harea[t.X];
                }

                if (procRegs)
                {
                    // If this polygon covers multiple region types then mark it as such
                    bool multiReg = HastMultipleRegions(hreg, t);

                    pregs[npolys] = multiReg ? RC_MULTIPLE_REGS : hreg[t.X];
                }

                npolys++;
            }

            // Cut to npolys
            polys = Helper.Truncate(polys, npolys);
            pareas = Helper.Truncate(pareas, npolys);
            pregs = Helper.Truncate(pregs, npolys);

            return (polys, pareas, pregs);
        }
        /// <summary>
        /// Creates the initial polygon list from a triangle definition
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="indices">Triangle indices</param>
        /// <returns>Returns the indexed polygon list and the number o polygons in the list</returns>
        public static IndexedPolygon[] CreateInitialPolygons(int[] indices, Int3[] tris)
        {
            var (polys, _, _) = CreateInitialPolygons(indices, tris, null, null);

            return polys;
        }
        /// <summary>
        /// Gets whether the specified indexed triangle contains different indexes
        /// </summary>
        /// <param name="t">Indexed triangle</param>
        private static bool ValidateIndex(Int3 t)
        {
            if (t.X == t.Y || t.X == t.Z || t.Y == t.Z)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether the specified indexed triangle has multiple regions
        /// </summary>
        /// <param name="hreg">Regions</param>
        /// <param name="t">Indexed triangle</param>
        private static bool HastMultipleRegions(int[] hreg, Int3 t)
        {
            return hreg[t.X] != hreg[t.Y] || hreg[t.Y] != hreg[t.Z];
        }

        /// <summary>
        /// Merges two polygons
        /// </summary>
        /// <param name="pa">First polygon</param>
        /// <param name="pb">Second polygon</param>
        /// <param name="ea">First merge value</param>
        /// <param name="eb">Second merge value</param>
        /// <returns>Returns the new polygon</returns>
        public static IndexedPolygon Merge(IndexedPolygon pa, IndexedPolygon pb, int ea, int eb)
        {
            int na = pa.CountPolyVerts();
            int nb = pb.CountPolyVerts();

            bool useAdj = pa.UseAdjacency && pb.UseAdjacency;
            var tmp = new IndexedPolygon(Math.Max(DT_VERTS_PER_POLYGON, na - 1 + nb - 1), useAdj);

            // Merge polygons.
            int n = 0;

            // Add pa
            for (int i = 0; i < na - 1; ++i)
            {
                int idx = (ea + 1 + i) % na;
                tmp.vertices[n] = pa.vertices[idx];
                if (useAdj) tmp.adjacency[n] = pa.adjacency[idx];
                n++;
            }

            // Add pb
            for (int i = 0; i < nb - 1; ++i)
            {
                int idx = (eb + 1 + i) % nb;
                tmp.vertices[n] = pb.vertices[idx];
                if (useAdj) tmp.adjacency[n] = pb.adjacency[idx];
                n++;
            }

            return tmp;
        }
        /// <summary>
        /// Gets whether the index has multiple regions or not
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns true if the index has multiple regions</returns>
        public static bool HasMultipleRegions(int index)
        {
            return index == RC_MULTIPLE_REGS;
        }
        /// <summary>
        /// Gets whether the vertex is null or not
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <returns>Returns true if the vertex is null</returns>
        public static bool IsNull(int v)
        {
            return v == RC_MESH_NULL_IDX;
        }
        /// <summary>
        /// Gets whether the specified points are sorted counter-clockwise in the xz plane
        /// </summary>
        /// <param name="a">Point a</param>
        /// <param name="b">Point b</param>
        /// <param name="c">Point c</param>
        private static bool ULeft2D(Int3 a, Int3 b, Int3 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
        }

        /// <summary>
        /// Counts the number of touched edges, and the remaining edges
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="nPolys">Number of polygons in the list</param>
        /// <param name="rem">Vertex to remove</param>
        public static (int NumTouchedVerts, int NumRemainingEdges) CountVertexToRemove(IndexedPolygon[] polys, int nPolys, int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < nPolys; ++i)
            {
                var p = polys[i];
                int nv = p.CountPolyVerts();
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (p.vertices[j] == rem)
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

            return (numTouchedVerts, numRemainingEdges);
        }
        /// <summary>
        /// Count the number of polygons to remove
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="nPolys">Number of polygons in the list</param>
        /// <param name="rem">Vertex to remove</param>
        public static int CountPolygonsToRemove(IndexedPolygon[] polys, int nPolys, int rem)
        {
            int numRemovedVerts = 0;

            foreach (var (p, _, j) in IteratePolygonVertices(polys, nPolys))
            {
                if (p.vertices[j] == rem)
                {
                    numRemovedVerts++;
                }
            }

            return numRemovedVerts;
        }
        /// <summary>
        /// Gets the best merge polygon indexes
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="nPolys">Number of polygons in the list</param>
        /// <param name="verts">Vertices</param>
        /// <returns>Returns the best merge value, and the polygon and edge indexes</returns>
        public static (int BestMergeValue, int BestPa, int BestPb, int BestEa, int BestEb) GetBestMergePolygon(IndexedPolygon[] polys, int nPolys, Int3[] verts)
        {
            // Find best polygons to merge.
            int bestMergeVal = 0;
            int bestPa = 0;
            int bestPb = 0;
            int bestEa = 0;
            int bestEb = 0;

            for (int j = 0; j < nPolys - 1; ++j)
            {
                var pj = polys[j];

                for (int k = j + 1; k < nPolys; ++k)
                {
                    var pk = polys[k];

                    int v = GetMergeValue(pj, pk, verts, out int ea, out int eb);
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

            return (bestMergeVal, bestPa, bestPb, bestEa, bestEb);
        }

        /// <summary>
        /// Builds a edge list from the specified polygon list
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons in the list</param>
        /// <param name="nverts">Number of total vertices in the referenced vertex list</param>
        /// <param name="addOpenEdges">Sets whether not matching edges must be connected by creating new edges</param>
        /// <param name="openPolyEdgeValue">Value to set in an open edge</param>
        /// <returns>Returns the edge list</returns>
        public static (Edge[] Edges, int EdgeCount) BuildAdjacencyEdges(IndexedPolygon[] polys, int npolys, int nverts, bool addOpenEdges, int openPolyEdgeValue)
        {
            int[] firstEdge = Helper.CreateArray(nverts, RC_MESH_NULL_IDX);

            List<Edge> edges = [];
            List<int> nextEdge = [];

            foreach (var (p, i, j) in IteratePolygonVertices(polys, npolys))
            {
                var (v0, v1) = p.GetSegmentIndices(j);
                if (v0 >= v1)
                {
                    continue;
                }

                Edge e = new()
                {
                    Vert = [v0, v1],
                    Poly = [i, i],
                    PolyEdge = [j, openPolyEdgeValue],
                };

                // Insert edge
                edges.Add(e);
                nextEdge.Add(firstEdge[v0]);
                firstEdge[v0] = edges.Count - 1;
            }

            AdjacencyEdgeHelper adjEdges = new()
            {
                EdgeCount = edges.Count,
                Edges = [.. edges],
                NextEdge = [.. nextEdge],
                FirstEdge = firstEdge,
            };

            return ConnectAdjacencyEdges(polys, npolys, adjEdges, addOpenEdges, openPolyEdgeValue);
        }
        /// <summary>
        /// Connects the adjacency edges
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons in the list</param>
        /// <param name="vertsPerPoly">Vertices per each polygon</param>
        /// <param name="edgeList">Adjacency edge list helper</param>
        /// <param name="addOpenEdges">Sets whether not matching edges must be connected by creating new edges</param>
        /// <param name="openPolyEdgeValue">Value to set in an open edge</param>
        /// <returns>Returns the edge list</returns>
        private static (Edge[] Edges, int EdgeCount) ConnectAdjacencyEdges(IndexedPolygon[] polys, int npolys, AdjacencyEdgeHelper edgeList, bool addOpenEdges, int openPolyEdgeValue)
        {
            var edges = new List<Edge>(edgeList.Edges);
            var firstEdge = new List<int>(edgeList.FirstEdge);
            var nextEdge = new List<int>(edgeList.NextEdge);

            foreach (var (p, i, j) in IteratePolygonVertices(polys, npolys))
            {
                var (v0, v1) = p.GetSegmentIndices(j);
                if (v0 <= v1)
                {
                    continue;
                }

                bool found = false;
                for (int e = firstEdge[v1]; !IsNull(e); e = nextEdge[e])
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
                if (!addOpenEdges || found)
                {
                    continue;
                }

                // Matching edge not found, it is an open edge, add it.
                edges.Add(new()
                {
                    Vert = [v0, v1],
                    Poly = [i, i],
                    PolyEdge = [j, openPolyEdgeValue],
                });

                // Insert edge
                nextEdge.Add(firstEdge[v1]);
                firstEdge[v1] = edges.Count - 1;
            }

            return (edges.ToArray(), edges.Count);
        }
        /// <summary>
        /// Stores the adjacency data
        /// </summary>
        /// <param name="polys">Polygon list to update</param>
        /// <param name="edges">Edge list</param>
        /// <param name="edgeCount">Number of edges in the list</param>
        /// <param name="addOpenEdges">Adds open edges</param>
        /// <param name="openPolyEdgeValue">Value to set in open edges</param>
        public static void StoreAdjacency(IndexedPolygon[] polys, Edge[] edges, int edgeCount, bool addOpenEdges, int openPolyEdgeValue)
        {
            for (int i = 0; i < edgeCount; ++i)
            {
                var e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0.adjacency[e.PolyEdge[0]] = e.Poly[1];
                    p1.adjacency[e.PolyEdge[1]] = e.Poly[0];
                }

                if (!addOpenEdges)
                {
                    continue;
                }

                if (e.PolyEdge[1] != openPolyEdgeValue)
                {
                    var p0 = polys[e.Poly[0]];
                    p0.adjacency[e.PolyEdge[0]] = Edge.DT_EXT_LINK | e.PolyEdge[1];
                }
            }
        }

        /// <summary>
        /// Iterates the vertices of each polygon in the specified list
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons in the list</param>
        /// <returns>Returns the polygon, the index of the polygon, and the index of de vertex</returns>
        public static IEnumerable<(IndexedPolygon p, int i, int j)> IteratePolygonVertices(IndexedPolygon[] polys, int npolys)
        {
            if (npolys <= 0)
            {
                yield break;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var p = polys[i];

                for (int j = 0; j < p.Capacity; ++j)
                {
                    if (p.VertexIsNull(j))
                    {
                        break;
                    }

                    yield return (p, i, j);
                }
            }
        }
        /// <summary>
        /// Iterates over the polygon vertices
        /// </summary>
        /// <returns>Returns the current and next vertex indices values (a,b), and its positions in the array (j,k)</returns>
        public IEnumerable<(int a, int b, int j, int k)> IterateVertices()
        {
            int nv = CountPolyVerts();

            for (int j = 0, k = nv - 1; j < nv; k = j++)
            {
                int a = GetVertex(j);
                int b = GetVertex(k);

                yield return (a, b, j, k);
            }
        }

        /// <summary>
        /// Gets the vertex count
        /// </summary>
        /// <returns>Returns the vertex count</returns>
        public int CountPolyVerts()
        {
            for (int i = 0; i < Capacity; ++i)
            {
                if (IsNull(vertices[i]))
                {
                    return i;
                }
            }

            return Capacity;
        }
        /// <summary>
        /// Gets the vertices list
        /// </summary>
        public int[] GetVertices()
        {
            //Copy array
            return [.. vertices];
        }
        /// <summary>
        /// Gets the vertex index value
        /// </summary>
        /// <param name="i">Index</param>
        public int GetVertex(int i)
        {
            return vertices[i];
        }
        /// <summary>
        /// Sets the vertex index value
        /// </summary>
        /// <param name="i">Index</param>
        /// <param name="value">Value</param>
        public void SetVertex(int i, int value)
        {
            vertices[i] = value;
        }
        /// <summary>
        /// Gets the adjacency list
        /// </summary>
        public int[] GetAdjacency()
        {
            //Copy array
            return [.. adjacency];
        }
        /// <summary>
        /// Gets the adjacency index value
        /// </summary>
        /// <param name="i">Index</param>
        public int GetAdjacency(int i)
        {
            return adjacency[i];
        }
        /// <summary>
        /// Sets the adjacency index value
        /// </summary>
        /// <param name="i">Index</param>
        /// <param name="value">Value</param>
        public void SetAdjacency(int i, int value)
        {
            adjacency[i] = value;
        }
        /// <summary>
        /// Gets the next valid vertex
        /// </summary>
        /// <param name="i">Start index</param>
        /// <param name="vertsPerPoly">Number of vertices per polygon</param>
        /// <returns>Returns the next index, or the first index if the start point is the last index</returns>
        public int GetNextVertex(int i)
        {
            return vertices[GetNextIndex(i)];
        }
        /// <summary>
        /// Gets the next index
        /// </summary>
        /// <param name="i">Current index</param>
        public int GetNextIndex(int i)
        {
            return (i + 1 >= Capacity || VertexIsNull(i + 1)) ? 0 : i + 1;
        }
        /// <summary>
        /// Copy the current polygon to another instance
        /// </summary>
        /// <returns>Returns the new instance</returns>
        public IndexedPolygon Copy()
        {
            var p = new IndexedPolygon(vertices.Length, UseAdjacency);

            Array.Copy(vertices, p.vertices, vertices.Length);
            if (UseAdjacency)
            {
                Array.Copy(adjacency, p.adjacency, adjacency.Length);
            }

            return p;
        }
        /// <summary>
        /// Gets the first free index (<see cref="RC_MESH_NULL_IDX"/> value)
        /// </summary>
        /// <param name="nvp">Vertex per polygon</param>
        /// <returns>Returns the first free index</returns>
        public int FindFirstFreeIndex(int nvp)
        {
            int nv = 0;

            for (int j = 0; j < nvp; ++j)
            {
                if (vertices[j] == RC_MESH_NULL_IDX)
                {
                    break;
                }
                nv++;
            }

            return nv;
        }
        /// <summary>
        /// Gets whether the collection contains the specified index
        /// </summary>
        /// <param name="index">Vertex index</param>
        public bool ContainsVertex(int index)
        {
            int nv = CountPolyVerts();

            for (int j = 0; j < nv; ++j)
            {
                if (vertices[j] == index)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Copy the other polygon vertices
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        public void CopyVertices(IndexedPolygon p)
        {
            int otherCapacity = p?.Capacity ?? 0;
            if (otherCapacity == 0)
            {
                return;
            }

            for (int i = 0; i < Capacity; ++i)
            {
                vertices[i] = i < otherCapacity ? p.vertices[i] : RC_MESH_NULL_IDX;
            }
        }

        /// <summary>
        /// Gets whether the vertex at the specified index is null
        /// </summary>
        /// <param name="index">Vertex index</param>
        public bool VertexIsNull(int index)
        {
            return IsNull(vertices[index]);
        }
        /// <summary>
        /// Gets whether the adjacency at the specified index is null
        /// </summary>
        /// <param name="index">Vertex index</param>
        public bool AdjacencyIsNull(int index)
        {
            return IsNull(adjacency[index]);
        }
        /// <summary>
        /// Gets whether the vertex has stored a external link or not
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        /// <returns></returns>
        public bool IsExternalLink(int adjIndex)
        {
            return Edge.IsExternalLink(adjacency[adjIndex]);
        }
        /// <summary>
        /// Gets whether the vertex has stored a direction or not
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        public bool HasDirection(int adjIndex)
        {
            return Edge.HasDirection(adjacency[adjIndex]);
        }
        /// <summary>
        /// Gets the stored direction at the specified adjacency index
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        public int GetDirection(int adjIndex)
        {
            return Edge.GetVertexDirection(adjacency[adjIndex]);
        }
        /// <summary>
        /// Gets the segment indices from the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the vertex at the specified index, and the next vertex in the polygon</returns>
        private (int V0, int V1) GetSegmentIndices(int index)
        {
            int v0 = vertices[index];
            int v1 = GetNextVertex(index);

            return (v0, v1);
        }

        /// <summary>
        /// Gets the polygon center
        /// </summary>
        /// <param name="verts">Polygon vertices</param>
        public (int X, int Y) GetCenter2D(Int3[] verts)
        {
            // Find center of the polygon
            int pcx = 0;
            int pcy = 0;
            for (int j = 0; j < vertices.Length; j++)
            {
                pcx += verts[vertices[j]].X;
                pcy += verts[vertices[j]].Z;
            }
            pcx /= vertices.Length;
            pcy /= vertices.Length;

            return (pcx, pcy);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (UseAdjacency)
            {
                return $"Indices: {vertices.Join(",")}; Adjacency: {adjacency.Join(",")}";
            }
            else
            {
                return $"Indices: {vertices.Join(",")}";
            }
        }
    }
}
