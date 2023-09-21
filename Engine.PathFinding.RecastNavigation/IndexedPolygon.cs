using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Indexed polygon
    /// </summary>
    [Serializable]
    public class IndexedPolygon
    {
        /// <summary>
        /// Polygon touches multiple regions.
        /// If a polygon has this region ID it was merged with or created
        /// from polygons of different regions during the polymesh
        /// build step that removes redundant border vertices. 
        /// (Used during the polymesh and detail polymesh build processes)
        /// </summary>
        public const int RC_MULTIPLE_REGS = 0;
        /// <summary>
        /// An value which indicates an invalid index within a mesh.
        /// </summary>
        public const int RC_MESH_NULL_IDX = -1;

        /// <summary>
        /// Gets whether the specified vertex can be removed
        /// </summary>
        /// <param name="polys">Polygon list</param>
        /// <param name="npolys">Number of polygons in the list</param>
        /// <param name="rem">Vertex to remove</param>
        /// <param name="MAX_REM_EDGES">Maximum edges to remove</param>
        /// <returns>Returns true if the vertex can be removed</returns>
        public static bool CanRemoveVertex(IndexedPolygon[] polys, int npolys, int rem, int MAX_REM_EDGES = 0)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < npolys; ++i)
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
            if (MAX_REM_EDGES > 0 && maxEdges > MAX_REM_EDGES)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            Int3[] edges = new Int3[maxEdges];
            int nedges = 0;

            for (int i = 0; i < npolys; ++i)
            {
                var p = polys[i];
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
                            if (e.Y == b)
                            {
                                // Exists, increment vertex share count.
                                e.Z++;
                                exists = true;
                            }
                            edges[m] = e;
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            edges[nedges] = new Int3(a, b, 1);
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
                if (edges[i].Z < 2)
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
            var (CanMerge, EdgeA, EdgeB) = PolygonsCanMerge(pa, pb);

            ea = EdgeA;
            eb = EdgeB;

            if (!CanMerge)
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
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pb[(eb + nb - 1) % nb];
            vb = pb[eb];
            vc = pa[(ea + 2) % na];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return RC_MESH_NULL_IDX;
            }

            va = pa[ea];
            vb = pa[(ea + 1) % na];

            int dx = verts[va].X - verts[vb].X;
            int dy = verts[va].Z - verts[vb].Z;

            return dx * dx + dy * dy;
        }
        private static (bool CanMerge, int EdgeA, int EdgeB) PolygonsCanMerge(IndexedPolygon pa, IndexedPolygon pb)
        {
            int na = pa.CountPolyVerts();
            int nb = pb.CountPolyVerts();

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > DetourUtils.DT_VERTS_PER_POLYGON)
            {
                return (false, RC_MESH_NULL_IDX, RC_MESH_NULL_IDX);
            }

            int ea = RC_MESH_NULL_IDX;
            int eb = RC_MESH_NULL_IDX;

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

            // No common edge, cannot merge.
            if (ea == RC_MESH_NULL_IDX || eb == RC_MESH_NULL_IDX)
            {
                return (false, RC_MESH_NULL_IDX, RC_MESH_NULL_IDX);
            }

            return (true, ea, eb);
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
        /// Gets whether the index is null or not
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns true if the index is null</returns>
        public static bool IndexIsNull(int index)
        {
            return index == RC_MESH_NULL_IDX;
        }

        private static bool Uleft(Int3 a, Int3 b, Int3 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
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
                return Vertices[index];
            }
            set
            {
                Vertices[index] = value;
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
            Vertices = Helper.CreateArray(capacity, RC_MESH_NULL_IDX);
        }

        /// <summary>
        /// Gets the vertex count
        /// </summary>
        /// <returns>Returns the vertex count</returns>
        public int CountPolyVerts()
        {
            for (int i = 0; i < DetourUtils.DT_VERTS_PER_POLYGON; ++i)
            {
                if (Vertices[i] == RC_MESH_NULL_IDX)
                {
                    return i;
                }
            }

            return DetourUtils.DT_VERTS_PER_POLYGON;
        }
        /// <summary>
        /// Gets the vertices list
        /// </summary>
        public int[] GetVertices()
        {
            return Vertices.ToArray();
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
        /// Gets the first free index (RC_MESH_NULL_IDX value)
        /// </summary>
        /// <param name="nvp">Vertex per polygon</param>
        /// <returns>Returns the first free index</returns>
        public int FindFirstFreeIndex(int nvp)
        {
            int nv = 0;

            for (int j = 0; j < nvp; ++j)
            {
                if (Vertices[j] == RC_MESH_NULL_IDX)
                {
                    break;
                }
                nv++;
            }

            return nv;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Vertices?.Join(",")}";
        }
    }
}
