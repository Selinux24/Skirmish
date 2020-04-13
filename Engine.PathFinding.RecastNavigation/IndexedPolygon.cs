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
        public int GetPolyMergeValue(IndexedPolygon poly, Int3[] verts, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = this.CountPolyVerts();
            int nb = poly.CountPolyVerts();

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > DetourUtils.DT_VERTS_PER_POLYGON)
            {
                return -1;
            }

            // Check if the polygons share an edge.
            this.GetSharedEdges(poly, out ea, out eb);

            // No common edge, cannot merge.
            if (ea == -1 || eb == -1)
            {
                return -1;
            }

            // Check to see if the merged polygon would be convex.
            int va, vb, vc;

            va = this[(ea + na - 1) % na];
            vb = this[ea];
            vc = poly[(eb + 2) % nb];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = poly[(eb + nb - 1) % nb];
            vb = poly[eb];
            vc = this[(ea + 2) % na];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = this[ea];
            vb = this[(ea + 1) % na];

            int dx = verts[va].X - verts[vb].X;
            int dy = verts[va].Z - verts[vb].Z;

            return dx * dx + dy * dy;
        }
        public static IndexedPolygon Merge(IndexedPolygon pa, IndexedPolygon pb, int ea, int eb)
        {
            int na = pa.CountPolyVerts();
            int nb = pb.CountPolyVerts();

            var tmp = new IndexedPolygon(Math.Max(DetourUtils.DT_VERTS_PER_POLYGON, na - 1 + nb - 1));

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
        public static bool BuildMeshAdjacency(IndexedPolygon[] polys, int npolys, int nverts, int vertsPerPoly)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            // Get the polygon edges
            var edges = GetEdges(polys, npolys, nverts, vertsPerPoly, out var firstEdge, out var nextEdge);

            // Update edge polygon indexes
            for (int i = 0; i < npolys; ++i)
            {
                polys[i].UpdateEdges(i, edges, firstEdge, nextEdge, vertsPerPoly);
            }

            // Store adjacency
            StoreAdjacency(edges, polys, vertsPerPoly);

            return true;
        }
        private static IEnumerable<Edge> GetEdges(IndexedPolygon[] polys, int npolys, int nverts, int vertsPerPoly, out int[] firstEdge, out int[] nextEdge)
        {
            int maxEdgeCount = npolys * vertsPerPoly;
            firstEdge = Helper.CreateArray(nverts, RecastUtils.RC_MESH_NULL_IDX);
            nextEdge = Helper.CreateArray(maxEdgeCount, RecastUtils.RC_MESH_NULL_IDX);
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];

                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (t[j] == RecastUtils.RC_MESH_NULL_IDX)
                    {
                        break;
                    }

                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || t[j + 1] == RecastUtils.RC_MESH_NULL_IDX) ? t[0] : t[j + 1];
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

            return edges.Take(edgeCount);
        }
        private static void StoreAdjacency(IEnumerable<Edge> edges, IndexedPolygon[] polys, int vertsPerPoly)
        {
            foreach (var e in edges)
            {
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = e.Poly[1];
                    p1[vertsPerPoly + e.PolyEdge[1]] = e.Poly[0];
                }
            }
        }
        private static bool IsPortalVertex(int dir, bool isMinX, bool isMaxX, bool isMinZ, bool isMaxZ)
        {
            switch (dir)
            {
                case 0: // Portal x-
                    if (isMinX)
                    {
                        return true;
                    }
                    break;
                case 1: // Portal z+
                    if (isMaxZ)
                    {
                        return true;
                    }
                    break;
                case 2: // Portal x+
                    if (isMaxX)
                    {
                        return true;
                    }
                    break;
                case 3: // Portal z-
                    if (isMinZ)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
        private static bool Uleft(Int3 a, Int3 b, Int3 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
        }
        private static bool GetAdjacency(Int3 va, Int3 vb, int h, int w, out int adjacency)
        {
            adjacency = -1;

            if (va.X == 0 && vb.X == 0)
            {
                adjacency = 0x8000;
                return true;
            }
            else if (va.Z == h && vb.Z == h)
            {
                adjacency = 0x8000 | 1;
                return true;
            }
            else if (va.X == w && vb.X == w)
            {
                adjacency = 0x8000 | 2;
                return true;
            }
            else if (va.Z == 0 && vb.Z == 0)
            {
                adjacency = 0x8000 | 3;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Vertex indices
        /// </summary>
        private int[] Vertices = null;
        /// <summary>
        /// Gets the polygon vertex index by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the polygon vertex index by index</returns>
        public int this[int index]
        {
            get
            {
                return this.Vertices[index];
            }
            set
            {
                this.Vertices[index] = value;
            }
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
            this.Vertices = Helper.CreateArray(capacity, -1);
        }

        /// <summary>
        /// Gets the vertices list
        /// </summary>
        public int[] GetVertices()
        {
            return Vertices.ToArray();
        }

        public void GetSharedEdges(IndexedPolygon poly, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = this.CountPolyVerts();
            int nb = poly.CountPolyVerts();

            for (int i = 0; i < na; ++i)
            {
                int va0 = this[i];
                int va1 = this[(i + 1) % na];
                if (va0 > va1)
                {
                    Helper.Swap(ref va0, ref va1);
                }

                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = poly[j];
                    int vb1 = poly[(j + 1) % nb];
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
        }
        /// <summary>
        /// Gets the first free index (-1 value)
        /// </summary>
        /// <param name="nvp">Vertex per polygon</param>
        /// <returns>Returns the first free index</returns>
        public int FindFirstFreeIndex(int nvp)
        {
            int nv = 0;

            for (int j = 0; j < nvp; ++j)
            {
                if (Vertices[j] == -1)
                {
                    break;
                }
                nv++;
            }

            return nv;
        }

        public int CountPolyVerts()
        {
            for (int i = 0; i < DetourUtils.DT_VERTS_PER_POLYGON; ++i)
            {
                if (Vertices[i] == RecastUtils.RC_MESH_NULL_IDX)
                {
                    return i;
                }
            }

            return DetourUtils.DT_VERTS_PER_POLYGON;
        }

        public void UpdateAdjacency(Int3[] verts, int h, int w, int nvp)
        {
            for (int j = 0; j < nvp; ++j)
            {
                if (Vertices[j] == RecastUtils.RC_MESH_NULL_IDX)
                {
                    break;
                }
                // Skip connected edges.
                if (Vertices[nvp + j] != RecastUtils.RC_MESH_NULL_IDX)
                {
                    continue;
                }
                int nj = j + 1;
                if (nj >= nvp || Vertices[nj] == RecastUtils.RC_MESH_NULL_IDX)
                {
                    nj = 0;
                }
                var va = verts[Vertices[j]];
                var vb = verts[Vertices[nj]];

                if (GetAdjacency(va, vb, h, w, out int adjacency))
                {
                    Vertices[nvp + j] = adjacency;
                }
            }
        }

        public void UpdateAdjacency(IndexedPolygon poly, int nvp, bool isMinX, bool isMaxX, bool isMinZ, bool isMaxZ)
        {
            for (int k = nvp; k < nvp * 2; k++)
            {
                if ((poly.Vertices[k] & 0x8000) == 0 || poly.Vertices[k] == 0xffff)
                {
                    continue;
                }

                int dir = poly.Vertices[k] & 0xf;
                if (IsPortalVertex(dir, isMinX, isMaxX, isMinZ, isMaxZ))
                {
                    Vertices[k] = poly.Vertices[k];
                }
            }
        }

        public void UpdateEdges(int polyIndex, IEnumerable<Edge> edges, int[] firstEdge, int[] nextEdge, int vertsPerPoly)
        {
            for (int j = 0; j < vertsPerPoly; ++j)
            {
                if (Vertices[j] == RecastUtils.RC_MESH_NULL_IDX)
                {
                    break;
                }

                int v0 = Vertices[j];
                int v1 = (j + 1 >= vertsPerPoly || Vertices[j + 1] == RecastUtils.RC_MESH_NULL_IDX) ? Vertices[0] : Vertices[j + 1];
                if (v0 <= v1)
                {
                    continue;
                }

                for (int e = firstEdge[v1]; e != RecastUtils.RC_MESH_NULL_IDX; e = nextEdge[e])
                {
                    Edge edge = edges.ElementAt(e);
                    if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                    {
                        edge.Poly[1] = polyIndex;
                        edge.PolyEdge[1] = j;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Copy the current polygon to another instance
        /// </summary>
        /// <returns>Returns the new instance</returns>
        public IndexedPolygon Copy()
        {
            return new IndexedPolygon(Vertices.Length)
            {
                Vertices = Vertices.ToArray(),
            };
        }
        /// <summary>
        /// Gets the polygon center
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <returns>Returns the center</returns>
        public Vector2Int GetCenter(Int3[] verts)
        {
            int pcx = 0;
            int pcy = 0;

            var polyIndices = GetVertices();
            int npoly = CountPolyVerts();

            for (int j = 0; j < npoly; j++)
            {
                pcx += verts[polyIndices[j]].X;
                pcy += verts[polyIndices[j]].Z;
            }

            pcx /= npoly;
            pcy /= npoly;

            return new Vector2Int(pcx, pcy);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}", Vertices?.Join(","));
        }
    }
}
