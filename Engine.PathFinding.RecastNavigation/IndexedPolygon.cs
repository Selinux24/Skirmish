using SharpDX;
using System;
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
        public const int RC_MULTIPLE_REGS = 0;
        /// <summary>
        /// An value which indicates an invalid index within a mesh.
        /// </summary>
        public const int RC_MESH_NULL_IDX = -1;

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
        /// Gets whether the index is null or not
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns true if the index is null</returns>
        public static bool IndexIsNull(int index)
        {
            return index == RC_MESH_NULL_IDX;
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
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i)
            {
                if (Vertices[i] == RC_MESH_NULL_IDX)
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
                //Copy array
                Vertices = Vertices.ToArray(),
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
            return $"Indices: {Vertices?.Join(",")}";
        }
    }
}
