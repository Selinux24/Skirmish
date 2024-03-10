using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Triangulation helper
    /// </summary>
    static class TriangulationHelper
    {
        /// <summary>
        /// Removable index mask
        /// </summary>
        const int SET_REMOVABLE_INDEX = 0x8000;
        /// <summary>
        /// Mask to remove the removable index mask
        /// </summary>
        const int REM_REMOVABLE_INDEX = 0x7fff;

        /// <summary>
        /// Triangulates the specified polygon
        /// </summary>
        /// <param name="verts">Polygon vertices</param>
        /// <returns>Returns the resulting triangle list</returns>
        public static (bool Result, Int3[] Tris) Triangulate(Int3[] verts)
        {
            // Initializes index array if no set using a sequential value list
            var indices = Helper.CreateSequentialArray(verts.Length);

            return Triangulate(verts, indices);
        }
        /// <summary>
        /// Triangulates the specified polygon
        /// </summary>
        /// <param name="verts">Polygon vertices</param>
        /// <param name="indices">Polygon indices</param>
        /// <returns>Returns the resulting triangle list</returns>
        public static (bool Result, Int3[] Tris) Triangulate(Int3[] verts, int[] indices)
        {
            if ((verts?.Length ?? 0) == 0 || (indices?.Length ?? 0) != 0)
            {
                return (false, []);
            }

            List<Int3> dst = [];

            // The last bit of the index is used to indicate if the vertex can be removed.
            var idx = SetRemovableIndices(verts, indices);

            int nidx = idx.Length;
            while (nidx > 3)
            {
                int minIndex = FindMinIndex2D(idx, nidx, verts);
                if (minIndex == -1)
                {
                    // Should not happen.
                    return (false, dst.ToArray());
                }

                int i0 = minIndex;
                int i1 = ArrayUtils.Next(i0, nidx);
                int i2 = ArrayUtils.Next(i1, nidx);

                dst.Add(new()
                {
                    X = idx[i0] & REM_REMOVABLE_INDEX,
                    Y = idx[i1] & REM_REMOVABLE_INDEX,
                    Z = idx[i2] & REM_REMOVABLE_INDEX
                });

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                nidx--;
                ArrayUtils.RemoveAt(idx, i1, nidx);

                if (i1 >= nidx)
                {
                    i1 = 0;
                }

                i0 = ArrayUtils.Prev(i1, nidx);

                // Update diagonal flags.
                if (Diagonal2D(ArrayUtils.Prev(i0, nidx), i1, idx, nidx, verts))
                {
                    idx[i0] |= SET_REMOVABLE_INDEX;
                }
                else
                {
                    idx[i0] &= REM_REMOVABLE_INDEX;
                }

                if (Diagonal2D(i0, ArrayUtils.Next(i1, nidx), idx, nidx, verts))
                {
                    idx[i1] |= SET_REMOVABLE_INDEX;
                }
                else
                {
                    idx[i1] &= REM_REMOVABLE_INDEX;
                }
            }

            // Append the remaining triangle.
            dst.Add(new()
            {
                X = idx[0] & REM_REMOVABLE_INDEX,
                Y = idx[1] & REM_REMOVABLE_INDEX,
                Z = idx[2] & REM_REMOVABLE_INDEX,
            });

            return (true, dst.ToArray());
        }
        /// <summary>
        /// Calculates each polygon vertex diagonal and sets removable indices
        /// </summary>
        /// <param name="verts">Polygon vertices</param>
        /// <param name="indices">Polygon indices</param>
        /// <returns>Returns a new collection of indices marked with the "removable index" flag</returns>
        private static int[] SetRemovableIndices(Int3[] verts, int[] indices)
        {
            var idx = indices.ToArray();

            int nidx = indices.Length;

            for (int i0 = 0; i0 < nidx; i0++)
            {
                int i1 = ArrayUtils.Next(i0, nidx);
                int i2 = ArrayUtils.Next(i1, nidx);
                if (Diagonal2D(i0, i2, idx, nidx, verts))
                {
                    idx[i1] |= SET_REMOVABLE_INDEX;
                }
            }

            return idx;
        }
        /// <summary>
        /// Evaluates each three indices (v0, v1, v2) from each index (v0) in the array, forming a triangle, and finds the v0 with the minimum distance between v0 and v2 vertices
        /// </summary>
        /// <param name="indices">Index list</param>
        /// <param name="nindices">Number of indices to evaluate</param>
        /// <param name="verts">Vertex list</param>
        /// <returns>Returns the mininum distance index</returns>
        private static int FindMinIndex2D(int[] indices, int nindices, Int3[] verts)
        {
            int mini = -1;

            int minLen = int.MaxValue;
            for (int i0 = 0; i0 < nindices; i0++)
            {
                int i1 = ArrayUtils.Next(i0, nindices);
                if ((indices[i1] & SET_REMOVABLE_INDEX) == 0)
                {
                    continue;
                }

                int i2 = ArrayUtils.Next(i1, nindices);

                var p0 = verts[indices[i0] & REM_REMOVABLE_INDEX];
                var p2 = verts[indices[i2] & REM_REMOVABLE_INDEX];

                int len = Utils.DistanceSqr2D(p0, p2);
                if (len < minLen)
                {
                    minLen = len;
                    mini = i0;
                }
            }

            return mini;
        }
        /// <summary>
        /// Gets wether p is into the ab,ac cone
        /// </summary>
        /// <param name="ca">A point</param>
        /// <param name="cb">B point</param>
        /// <param name="cc">C point</param>
        /// <param name="p">Point to test</param>
        public static bool InCone2D(Int3 ca, Int3 cb, Int3 cc, Int3 p)
        {
            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn2D(cc, ca, cb))
            {
                return Left2D(ca, p, cc) && Left2D(p, ca, cb);
            }

            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn2D(ca, p, cb) && LeftOn2D(p, ca, cc));
        }
        /// <summary>
        /// Gets wether j is into the i.i-1,i-i+1 cone
        /// </summary>
        /// <param name="i">Initial vertex index</param>
        /// <param name="j">Point to test index</param>
        /// <param name="indices">Index list</param>
        /// <param name="n">Number of indices</param>
        /// <param name="verts">Vertex list</param>
        private static bool InCone2D(int i, int j, int[] indices, int n, Int3[] verts)
        {
            var a = verts[indices[i] & REM_REMOVABLE_INDEX];
            var b = verts[indices[ArrayUtils.Next(i, n)] & REM_REMOVABLE_INDEX];
            var c = verts[indices[ArrayUtils.Prev(i, n)] & REM_REMOVABLE_INDEX];
            var p = verts[indices[j] & REM_REMOVABLE_INDEX];

            return InCone2D(a, b, c, p);
        }
        /// <summary>
        /// Gets whether the ij segment isn't intersects with any other polygon segments, and is into the cone defined by ij and his neighbours
        /// </summary>
        /// <param name="i">First segment vertex index</param>
        /// <param name="j">Second segment vertex index</param>
        /// <param name="indices">Index list</param>
        /// <param name="n">Number of indices</param>
        /// <param name="verts">Vertex list</param>
        private static bool Diagonal2D(int i, int j, int[] indices, int n, Int3[] verts)
        {
            return InCone2D(i, j, indices, n, verts) && Diagonalie2D(i, j, indices, n, verts);
        }
        /// <summary>
        /// Gets whether the ij segment isn't intersects with any other polygon segments
        /// </summary>
        /// <param name="i">First segment vertex index</param>
        /// <param name="j">Second segment vertex index</param>
        /// <param name="indices">Index list</param>
        /// <param name="n">Number of indices</param>
        /// <param name="verts">Vertex list</param>
        private static bool Diagonalie2D(int i, int j, int[] indices, int n, Int3[] verts)
        {
            var d0 = verts[indices[i] & REM_REMOVABLE_INDEX];
            var d1 = verts[indices[j] & REM_REMOVABLE_INDEX];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = ArrayUtils.Next(k, n);

                // Skip edges incident to i or j
                if ((k == i) || (k1 == i) || (k == j) || (k1 == j))
                {
                    continue;
                }

                var p0 = verts[indices[k] & REM_REMOVABLE_INDEX];
                var p1 = verts[indices[k1] & REM_REMOVABLE_INDEX];

                if (Utils.VEqual2D(d0, p0) || Utils.VEqual2D(d1, p0) || Utils.VEqual2D(d0, p1) || Utils.VEqual2D(d1, p1))
                {
                    continue;
                }

                if (Intersect2D(d0, d1, p0, p1))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a">Segment ab Point a</param>
        /// <param name="b">Segment ab Point b</param>
        /// <param name="c">Segment cd Point c</param>
        /// <param name="d">Segment cd Point d</param>
        public static bool Intersect2D(Int3 a, Int3 b, Int3 c, Int3 d)
        {
            if (IntersectProp2D(a, b, c, d))
            {
                return true;
            }
            else if (Between2D(a, b, c) || Between2D(a, b, d) || Between2D(c, d, a) || Between2D(c, d, b))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Returns true iff ab properly intersects cd: they share a point interior to both segments.
        /// The properness of the intersection is ensured by using strict leftness.
        /// </summary>
        /// <param name="a">Segment ab Point a</param>
        /// <param name="b">Segment ab Point b</param>
        /// <param name="c">Segment cd Point c</param>
        /// <param name="d">Segment cd Point d</param>
        /// <returns>Returns true iff ab properly intersects cd</returns>
        private static bool IntersectProp2D(Int3 a, Int3 b, Int3 c, Int3 d)
        {
            // Eliminate improper cases.
            if (Collinear2D(a, b, c) || Collinear2D(a, b, d) || Collinear2D(c, d, a) || Collinear2D(c, d, b))
            {
                return false;
            }

            return (Left2D(a, b, c) ^ Left2D(a, b, d)) && (Left2D(c, d, a) ^ Left2D(c, d, b));
        }
        /// <summary>
        /// Gets whether the specified ab line is collinear respect of the c point
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <param name="c">Point C</param>
        /// <returns>Returns true if collinear</returns>
        /// <remarks>Three points of a triangle are collinear if the its area is zero</remarks>
        private static bool Collinear2D(Int3 a, Int3 b, Int3 c)
        {
            return Utils.TriArea2D(a, b, c) == 0;
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <param name="c">Point C</param>
        private static bool Between2D(Int3 a, Int3 b, Int3 c)
        {
            if (!Collinear2D(a, b, c))
            {
                return false;
            }

            // If ab not vertical, check betweenness on x; else on z.
            if (a.X != b.X)
            {
                return ((a.X <= c.X) && (c.X <= b.X)) || ((a.X >= c.X) && (c.X >= b.X));
            }
            else
            {
                return ((a.Z <= c.Z) && (c.Z <= b.Z)) || ((a.Z >= c.Z) && (c.Z >= b.Z));
            }
        }
        /// <summary>
        /// Gets whether p is on the left of the segment ab
        /// </summary>
        /// <param name="sa">Segment a point</param>
        /// <param name="sb">Segment b point</param>
        /// <param name="p">Point to test</param>
        private static bool Left2D(Int3 sa, Int3 sb, Int3 p)
        {
            return Utils.TriArea2D(sa, sb, p) > 0;
        }
        /// <summary>
        /// Gets whether p is on the left of the segment ab or lies in the segment
        /// </summary>
        /// <param name="sa">Segment a point</param>
        /// <param name="sb">Segment b point</param>
        /// <param name="p">Point to test</param>
        private static bool LeftOn2D(Int3 sa, Int3 sb, Int3 p)
        {
            return Utils.TriArea2D(sa, sb, p) >= 0;
        }

        /// <summary>
        /// Triangulates a hull
        /// </summary>
        /// <param name="verts">Hull vertices</param>
        /// <param name="hull">Hull indices</param>
        /// <returns>Returns the indexed triangle list</returns>
        public static Int3[] TriangulateHull(Vector3[] verts, int[] hull)
        {
            var tris = new List<Int3>();

            int nhull = hull.Length;
            int nin = verts.Length;

            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = float.MaxValue;
            for (int ci = 0; ci < nhull; ci++)
            {
                if (hull[ci] >= nin)
                {
                    continue; // Ears are triangles with original vertices as middle vertex while others are actually line segments on edges
                }

                int pi = ArrayUtils.Prev(ci, nhull);
                int ni = ArrayUtils.Next(ci, nhull);
                var pv = verts[hull[pi]];
                var cv = verts[hull[ci]];
                var nv = verts[hull[ni]];

                float d =
                    Utils.DistanceSqr2D(pv, cv) +
                    Utils.DistanceSqr2D(cv, nv) +
                    Utils.DistanceSqr2D(nv, pv);
                if (d < dmin)
                {
                    start = ci;
                    left = ni;
                    right = pi;
                    dmin = d;
                }
            }

            // Add first triangle
            tris.Add(new Int3()
            {
                X = hull[start],
                Y = hull[left],
                Z = hull[right],
            });

            // Triangulate the polygon by moving left or right,
            // depending on which triangle has shorter perimeter.
            // This heuristic was chose emprically, since it seems
            // handle tesselated straight edges well.
            while (ArrayUtils.Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = ArrayUtils.Next(left, nhull);
                int nright = ArrayUtils.Prev(right, nhull);

                var cvleft = verts[hull[left]];
                var nvleft = verts[hull[nleft]];
                var cvright = verts[hull[right]];
                var nvright = verts[hull[nright]];

                float dleft =
                    Utils.DistanceSqr2D(cvleft, nvleft) +
                    Utils.DistanceSqr2D(nvleft, cvright);

                float dright =
                    Utils.DistanceSqr2D(cvright, nvright) +
                    Utils.DistanceSqr2D(cvleft, nvright);

                if (dleft < dright)
                {
                    tris.Add(new()
                    {
                        X = hull[left],
                        Y = hull[nleft],
                        Z = hull[right],
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new()
                    {
                        X = hull[left],
                        Y = hull[nright],
                        Z = hull[right],
                    });

                    right = nright;
                }
            }

            return [.. tris];
        }
    }
}
