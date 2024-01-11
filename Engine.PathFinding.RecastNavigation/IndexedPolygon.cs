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
        /// Vertex indices
        /// </summary>
        private int[] vertices = null;
        /// <summary>
        /// Gets the polygon vertex index by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the polygon vertex index by index</returns>
        public int this[int index]
        {
            get
            {
                return vertices[index];
            }
            set
            {
                vertices[index] = value;
            }
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

            va = pa[(ea + na - 1) % na];
            vb = pa[ea];
            vc = pb[(eb + 2) % nb];
            if (!ULeft2D(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pb[(eb + nb - 1) % nb];
            vb = pb[eb];
            vc = pa[(ea + 2) % na];
            if (!ULeft2D(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pa[ea];
            vb = pa[(ea + 1) % na];
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
                int va0 = pa[i];
                int va1 = pa[(i + 1) % na];

                if (va0 > va1)
                {
                    Helper.Swap(ref va0, ref va1);
                }

                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = pb[j];
                    int vb1 = pb[(j + 1) % nb];

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
        /// <param name="npolys">Number of polygons</param>
        /// <param name="pregs">Region list</param>
        /// <param name="pareas">Area list</param>
        /// <returns>Returns the resulting merged polygon</returns>
        public static (IndexedPolygon[] MergedPolys, int MergedNPolys, SamplePolyAreas[] MergedAreas, int[] MergedRegs) MergePolygons(IndexedPolygon[] polys, int npolys, SamplePolyAreas[] pareas, int[] pregs, Int3[] verts)
        {
            var mergedPolys = polys.ToArray();
            var mergedNpolys = npolys;
            var mergedareas = pareas?.ToArray() ?? Array.Empty<SamplePolyAreas>();
            var mergedregs = pregs?.ToArray() ?? Array.Empty<int>();

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

                if (mergedareas.Any())
                {
                    mergedareas[bestPb] = mergedareas[mergedNpolys - 1];
                }

                if (mergedregs.Any())
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
            mergedPolys = mergedPolys.Take(mergedNpolys).ToArray();
            if (mergedareas.Any()) mergedareas = mergedareas.Take(mergedNpolys).ToArray();
            if (mergedregs.Any()) mergedregs = mergedregs.Take(mergedNpolys).ToArray();

            return (mergedPolys, mergedNpolys, mergedareas, mergedregs);
        }
        /// <summary>
        /// Merges the polygon list
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons</param>
        /// <returns>Returns the resulting merged polygon</returns>
        public static (IndexedPolygon[] MergedPolys, int MergedNPolys) MergePolygons(IndexedPolygon[] polys, int npolys, Int3[] verts)
        {
            var (mergedPolys, mergedNpolys, _, _) = MergePolygons(polys, npolys, null, null, verts);

            return (mergedPolys, mergedNpolys);
        }
        /// <summary>
        /// Build initial polygons for the remove operation
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="ntris">Number of triangles</param>
        /// <param name="hole">Hole indices</param>
        /// <param name="hreg">Region id list</param>
        /// <param name="harea">Area list</param>
        /// <returns>Returns the indexed polygons, regions and areas</returns>
        public static (IndexedPolygon[] Polys, int NPolys, SamplePolyAreas[] PAreas, int[] PRegs) BuildRemoveInitialPolygons(Int3[] tris, int ntris, int[] hole, SamplePolyAreas[] harea, int[] hreg)
        {
            bool procAreas = harea?.Any() ?? false;
            bool procRegs = hreg?.Any() ?? false;

            // Merge the hole triangles back to polygons.
            var polys = new IndexedPolygon[ntris + 1];
            var pareas = new SamplePolyAreas[ntris];
            var pregs = new int[ntris];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];

                if (t.X == t.Y || t.X == t.Z || t.Y == t.Z)
                {
                    continue;
                }

                polys[npolys] = new();
                polys[npolys][0] = hole[t.X];
                polys[npolys][1] = hole[t.Y];
                polys[npolys][2] = hole[t.Z];

                if (procAreas) pareas[npolys] = harea[t.X];

                if (procRegs)
                {
                    // If this polygon covers multiple region types then mark it as such
                    if (hreg[t.X] != hreg[t.Y] || hreg[t.Y] != hreg[t.Z])
                    {
                        pregs[npolys] = RC_MULTIPLE_REGS;
                    }
                    else
                    {
                        pregs[npolys] = hreg[t.X];
                    }
                }

                npolys++;
            }

            return (polys, npolys, pareas, pregs);
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

            var tmp = new IndexedPolygon(Math.Max(DT_VERTS_PER_POLYGON, na - 1 + nb - 1));

            // Merge polygons.
            int n = 0;
            // Add pa
            for (int i = 0; i < na - 1; ++i)
            {
                tmp[n++] = pa[(ea + 1 + i) % na];
            }
            // Add pb
            for (int i = 0; i < nb - 1; ++i)
            {
                tmp[n++] = pb[(eb + 1 + i) % nb];
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
        public static bool VertexIsNull(int v)
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
            for (int i = 0; i < nPolys; i++)
            {
                var p = polys[i];
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
        /// <param name="vertsPerPoly">Vertices per each polygon</param>
        /// <param name="nverts">Number of total vertices in the referenced vertex list</param>
        /// <param name="addOpenEdges">Sets whether not matching edges must be connected by creating new edges</param>
        /// <param name="openPolyEdgeValue">Value to set in an open edge</param>
        /// <returns>Returns the edge list</returns>
        public static (Edge[] Edges, int EdgeCount) BuildAdjacencyEdges(IndexedPolygon[] polys, int npolys, int vertsPerPoly, int nverts, bool addOpenEdges, int openPolyEdgeValue)
        {
            int maxEdgeCount = npolys * vertsPerPoly;
            int[] firstEdge = Helper.CreateArray(nverts, RC_MESH_NULL_IDX);
            int[] nextEdge = Helper.CreateArray(maxEdgeCount, RC_MESH_NULL_IDX);
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < npolys; ++i)
            {
                var p = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (p.IsNull(j))
                    {
                        break;
                    }

                    int v0 = p[j];
                    int v1 = (j + 1 >= vertsPerPoly || p.IsNull(j + 1)) ? p[0] : p[j + 1];
                    if (v0 < v1)
                    {
                        edges[edgeCount] = new()
                        {
                            Vert = new int[] { v0, v1 },
                            Poly = new int[] { i, i },
                            PolyEdge = new int[] { j, openPolyEdgeValue },
                        };

                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount++;
                    }
                }
            }

            AdjacencyEdgeHelper adjEdges = new()
            {
                Edges = edges,
                EdgeCount = edgeCount,
                FirstEdge = firstEdge,
                NextEdge = nextEdge,
            };

            return ConnectAdjacencyEdges(polys, npolys, vertsPerPoly, adjEdges, addOpenEdges, openPolyEdgeValue);
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
        private static (Edge[] Edges, int EdgeCount) ConnectAdjacencyEdges(IndexedPolygon[] polys, int npolys, int vertsPerPoly, AdjacencyEdgeHelper edgeList, bool addOpenEdges, int openPolyEdgeValue)
        {
            var edges = edgeList.Edges;
            var edgeCount = edgeList.EdgeCount;
            var firstEdge = edgeList.FirstEdge;
            var nextEdge = edgeList.NextEdge;

            for (int i = 0; i < npolys; ++i)
            {
                var p = polys[i];

                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (p.IsNull(j))
                    {
                        break;
                    }

                    var (v0, v1) = p.GetSegmentIndices(j, vertsPerPoly);
                    if (v0 <= v1)
                    {
                        continue;
                    }

                    bool found = false;
                    for (int e = firstEdge[v1]; !VertexIsNull(e); e = nextEdge[e])
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
                    edges[edgeCount] = new()
                    {
                        Vert = new int[] { v0, v1 },
                        Poly = new int[] { i, i },
                        PolyEdge = new int[] { j, openPolyEdgeValue },
                    };

                    // Insert edge
                    nextEdge[edgeCount] = firstEdge[v1];
                    firstEdge[v1] = edgeCount++;
                }
            }

            return (edges, edgeCount);
        }
        /// <summary>
        /// Stores the adjacency data
        /// </summary>
        /// <param name="polys">Polygon list to update</param>
        /// <param name="vertsPerPoly">Vertices per polygon</param>
        /// <param name="edges">Edge list</param>
        /// <param name="edgeCount">Number of edges in the list</param>
        public static void StoreAdjacency(IndexedPolygon[] polys, int vertsPerPoly, Edge[] edges, int edgeCount, bool addOpenEdges, int openPolyEdgeValue)
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

                if (!addOpenEdges)
                {
                    continue;
                }

                if (e.PolyEdge[1] != openPolyEdgeValue)
                {
                    var p0 = polys[e.Poly[0]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = VertexFlags.DT_EXT_LINK | e.PolyEdge[1];
                }
            }
        }

        /// <summary>
        /// Creates the initial polygon list from a triangle definition
        /// </summary>
        /// <param name="indices">Triangle indices</param>
        /// <param name="tris">Triangle list</param>
        /// <param name="ntris">Number of triangles in the triangle list</param>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        /// <returns>Returns the indexed polygon list and the number o polygons in the list</returns>
        public static (IndexedPolygon[] Polys, int NPolys) CreateInitialPolygons(int[] indices, Int3[] tris, int ntris, int maxVertsPerCont)
        {
            int npolys = 0;
            var polys = new IndexedPolygon[maxVertsPerCont];

            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];

                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    var poly = new IndexedPolygon(DT_VERTS_PER_POLYGON);
                    poly[0] = indices[t.X];
                    poly[1] = indices[t.Y];
                    poly[2] = indices[t.Z];

                    polys[npolys++] = poly;
                }
            }

            return (polys, npolys);
        }

        /// <summary>
        /// Iterates over the polygon vertices of each polygon in the collection
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons in the list</param>
        /// <param name="nvp">Number of vertes per polygon</param>
        /// <returns>Returns the polygon, the polygon index in the polygon list, the vertex and the vertex index in the polygon vertex list</returns>
        public static IEnumerable<(IndexedPolygon Poly, int pIndex, int vertex, int vIndex)> Iterate(IndexedPolygon[] polys, int npolys, int nvp)
        {
            if (npolys <= 0)
            {
                yield break;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var p = polys[i];

                for (int j = 0; j < nvp; ++j)
                {
                    yield return (p, i, p[j], j);
                }
            }
        }
        /// <summary>
        /// Returns the portal value, if any
        /// </summary>
        /// <param name="va">First vertex</param>
        /// <param name="vb">Second vertex</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="portalValue">Returns the portal value</param>
        /// <returns>Returns true if found</returns>
        public static bool IsPortal(Int3 va, Int3 vb, int w, int h, out int portalValue)
        {
            if (va.X == 0 && vb.X == 0)
            {
                portalValue = VertexFlags.DT_EXT_LINK;

                return true;
            }
            else if (va.Z == h && vb.Z == h)
            {
                portalValue = VertexFlags.DT_EXT_LINK | 1;

                return true;
            }
            else if (va.X == w && vb.X == w)
            {
                portalValue = VertexFlags.DT_EXT_LINK | 2;

                return true;
            }
            else if (va.Z == 0 && vb.Z == 0)
            {
                portalValue = VertexFlags.DT_EXT_LINK | 3;

                return true;
            }

            portalValue = -1;

            return false;
        }
        /// <summary>
        /// Calculates the vertex portal flag direction value
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <returns>Returns the vertex portal flag direction value</returns>
        public static int CalculateVertexPortalFlag(int v)
        {
            var dir = v & VertexFlags.PORTAL_FLAG;

            if (dir == VertexFlags.PORTAL_FLAG) // Border
            {
                return 0;
            }
            else if (dir == 0) // Portal x-
            {
                return VertexFlags.DT_EXT_LINK | 4;
            }
            else if (dir == 1) // Portal z+
            {
                return VertexFlags.DT_EXT_LINK | 2;
            }
            else if (dir == 2) // Portal x+
            {
                return VertexFlags.DT_EXT_LINK;
            }
            else if (dir == 3) // Portal z-
            {
                return VertexFlags.DT_EXT_LINK | 6;
            }

            return v;
        }
        /// <summary>
        /// Gets the point to side index
        /// </summary>
        /// <param name="side">Side</param>
        public static int PointToSide(int side)
        {
            return VertexFlags.DT_EXT_LINK | side;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public IndexedPolygon() : this(10)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Polygon capacity</param>
        public IndexedPolygon(int capacity)
        {
            vertices = Helper.CreateArray(capacity, RC_MESH_NULL_IDX);
        }

        /// <summary>
        /// Gets the vertex count
        /// </summary>
        /// <returns>Returns the vertex count</returns>
        public int CountPolyVerts()
        {
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i)
            {
                if (vertices[i] == RC_MESH_NULL_IDX)
                {
                    return i;
                }
            }

            return DT_VERTS_PER_POLYGON;
        }
        /// <summary>
        /// Gets the vertices list
        /// </summary>
        public int[] GetVertices()
        {
            //Copy array
            return vertices.ToArray();
        }
        /// <summary>
        /// Copy the current polygon to another instance
        /// </summary>
        /// <returns>Returns the new instance</returns>
        public IndexedPolygon Copy()
        {
            return new IndexedPolygon(vertices.Length)
            {
                //Copy array
                vertices = vertices.ToArray(),
            };
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
        public bool Contains(int index)
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
        /// <param name="nvp">Number of vertex to copy</param>
        public void CopyVertices(IndexedPolygon p, int nvp)
        {
            for (int i = 0; i < nvp; ++i)
            {
                vertices[i] = p[i];
            }
        }

        /// <summary>
        /// Gets whether the vertex at the specified index is null
        /// </summary>
        /// <param name="index">Vertex index</param>
        public bool IsNull(int index)
        {
            return VertexIsNull(vertices[index]);
        }
        /// <summary>
        /// Gets whether the vertex has stored a external link or not
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        /// <returns></returns>
        public bool IsExternalLink(int adjIndex)
        {
            return VertexFlags.IsExternalLink(vertices[adjIndex]);
        }
        /// <summary>
        /// Gets whether the vertex has stored a direction or not
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        public bool HasDirection(int adjIndex)
        {
            return VertexFlags.HasDirection(vertices[adjIndex]);
        }
        /// <summary>
        /// Gets the stored direction at the specified adjacency index
        /// </summary>
        /// <param name="adjIndex">Adjacency index</param>
        public int GetDirection(int adjIndex)
        {
            return VertexFlags.GetVertexDirection(vertices[adjIndex]);
        }
        /// <summary>
        /// Gets the segment indices from the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="vertsPerPoly">Number of vertices in the polygon</param>
        /// <returns>Returns the vertex at the specified index, and the next vertex in the polygon</returns>
        private (int V0, int V1) GetSegmentIndices(int index, int vertsPerPoly)
        {
            int v0 = vertices[index];
            int v1 = (index + 1 >= vertsPerPoly || IsNull(index + 1)) ? vertices[0] : vertices[index + 1];

            return (v0, v1);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Indices: {vertices?.Join(",")}";
        }
    }
}
