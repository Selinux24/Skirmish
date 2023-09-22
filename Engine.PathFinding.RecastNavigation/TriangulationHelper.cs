using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
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

        public static int Triangulate(Int4[] verts, ref int[] indices, out Int3[] tris)
        {
            var dst = new List<Int3>();

            // The last bit of the index is used to indicate if the vertex can be removed.
            indices = SetRemovableIndices(verts, indices);

            int n = verts.Length;
            while (n > 3)
            {
                int mini = FindMinIndex2D(n, verts, indices);

                if (mini == -1)
                {
                    // Should not happen.
                    tris = null;
                    return -dst.Count;
                }

                int i = mini;
                int i1 = Utils.Next(i, n);
                int i2 = Utils.Next(i1, n);

                dst.Add(new Int3()
                {
                    X = indices[i] & REM_REMOVABLE_INDEX,
                    Y = indices[i1] & REM_REMOVABLE_INDEX,
                    Z = indices[i2] & REM_REMOVABLE_INDEX
                });

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                indices = Utils.RemoveRange(indices, i1, n);

                if (i1 >= n)
                {
                    i1 = 0;
                }

                i = Utils.Prev(i1, n);

                // Update diagonal flags.
                if (Diagonal2D(Utils.Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= SET_REMOVABLE_INDEX;
                }
                else
                {
                    indices[i] &= REM_REMOVABLE_INDEX;
                }

                if (Diagonal2D(i, Utils.Next(i1, n), n, verts, indices))
                {
                    indices[i1] |= SET_REMOVABLE_INDEX;
                }
                else
                {
                    indices[i1] &= REM_REMOVABLE_INDEX;
                }
            }

            // Append the remaining triangle.
            dst.Add(new Int3
            {
                X = indices[0] & REM_REMOVABLE_INDEX,
                Y = indices[1] & REM_REMOVABLE_INDEX,
                Z = indices[2] & REM_REMOVABLE_INDEX,
            });

            tris = dst.ToArray();

            return dst.Count;
        }
        private static int[] SetRemovableIndices(Int4[] verts, int[] indices)
        {
            var res = indices.ToArray();

            int n = verts.Length;

            for (int i = 0; i < n; i++)
            {
                int i1 = Utils.Next(i, n);
                int i2 = Utils.Next(i1, n);
                if (Diagonal2D(i, i2, n, verts, res))
                {
                    res[i1] |= SET_REMOVABLE_INDEX;
                }
            }

            return res;
        }
        private static bool IsRemovable(int index)
        {
            return (index & SET_REMOVABLE_INDEX) == 0;
        }

        private static int FindMinIndex2D(int n, Int4[] verts, int[] indices)
        {
            int minLen = -1;
            int mini = -1;

            for (int ix = 0; ix < n; ix++)
            {
                int i1x = Utils.Next(ix, n);

                if (IsRemovable(indices[i1x]))
                {
                    continue;
                }

                var p0 = verts[indices[ix] & REM_REMOVABLE_INDEX];
                var p2 = verts[indices[Utils.Next(i1x, n)] & REM_REMOVABLE_INDEX];

                int dx = p2.X - p0.X;
                int dz = p2.Z - p0.Z;
                int len = dx * dx + dz * dz;
                if (minLen < 0 || len < minLen)
                {
                    minLen = len;
                    mini = ix;
                }
            }

            return mini;
        }
        private static bool Diagonal2D(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InCone2D(i, j, n, verts, indices) && Diagonalie2D(i, j, n, verts, indices);
        }
        private static bool InCone2D(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var pi = verts[indices[i] & REM_REMOVABLE_INDEX];
            var pj = verts[indices[j] & REM_REMOVABLE_INDEX];
            var pi1 = verts[indices[Utils.Next(i, n)] & REM_REMOVABLE_INDEX];
            var pin1 = verts[indices[Utils.Prev(i, n)] & REM_REMOVABLE_INDEX];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn2D(pin1, pi, pi1))
            {
                return Left2D(pi, pj, pin1) && Left2D(pj, pi, pi1);
            }

            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn2D(pi, pj, pi1) && LeftOn2D(pj, pi, pin1));
        }
        private static bool Diagonalie2D(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var d0 = verts[indices[i] & REM_REMOVABLE_INDEX];
            var d1 = verts[indices[j] & REM_REMOVABLE_INDEX];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Utils.Next(k, n);

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
        public static bool Intersect2D(Int4 a, Int4 b, Int4 c, Int4 d)
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
        private static bool IntersectProp2D(Int4 a, Int4 b, Int4 c, Int4 d)
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
        private static bool Collinear2D(Int4 a, Int4 b, Int4 c)
        {
            return Utils.TriArea2D(a, b, c) == 0;
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <param name="c">Point C</param>
        private static bool Between2D(Int4 a, Int4 b, Int4 c)
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

        private static bool Left2D(Int4 a, Int4 b, Int4 c)
        {
            return Utils.TriArea2D(a, b, c) > 0;
        }

        private static bool LeftOn2D(Int4 a, Int4 b, Int4 c)
        {
            return Utils.TriArea2D(a, b, c) >= 0;
        }

        public static bool InCone2D(int i, int n, Int4[] verts, Int4 pj)
        {
            var pi = verts[i];
            var pi1 = verts[Utils.Next(i, n)];
            var pin1 = verts[Utils.Prev(i, n)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn2D(pin1, pi, pi1))
            {
                return Left2D(pi, pj, pin1) && Left2D(pj, pi, pi1);
            }

            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn2D(pi, pj, pi1) && LeftOn2D(pj, pi, pin1));
        }

        public static Int3[] TriangulateHull(Vector3[] verts, int[] hull)
        {
            var tris = new List<Int3>();

            int nhull = hull.Length;
            int nin = verts.Length;

            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = float.MaxValue;
            for (int i = 0; i < nhull; i++)
            {
                if (hull[i] >= nin)
                {
                    continue; // Ears are triangles with original vertices as middle vertex while others are actually line segments on edges
                }

                int pi = Utils.Prev(i, nhull);
                int ni = Utils.Next(i, nhull);
                var pv = verts[hull[pi]];
                var cv = verts[hull[i]];
                var nv = verts[hull[ni]];

                float d =
                    Vector2.Distance(new Vector2(pv.X, pv.Z), new Vector2(cv.X, cv.Z)) +
                    Vector2.Distance(new Vector2(cv.X, cv.Z), new Vector2(nv.X, nv.Z)) +
                    Vector2.Distance(new Vector2(nv.X, nv.Z), new Vector2(pv.X, pv.Z));
                if (d < dmin)
                {
                    start = i;
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
            while (Utils.Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = Utils.Next(left, nhull);
                int nright = Utils.Prev(right, nhull);

                var cvleft = verts[hull[left]];
                var nvleft = verts[hull[nleft]];
                var cvright = verts[hull[right]];
                var nvright = verts[hull[nright]];
                float dleft =
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvleft.X, nvleft.Z)) +
                    Vector2.Distance(new Vector2(nvleft.X, nvleft.Z), new Vector2(cvright.X, cvright.Z));

                float dright =
                    Vector2.Distance(new Vector2(cvright.X, cvright.Z), new Vector2(nvright.X, nvright.Z)) +
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvright.X, nvright.Z));

                if (dleft < dright)
                {
                    tris.Add(new Int3()
                    {
                        X = hull[left],
                        Y = hull[nleft],
                        Z = hull[right],
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new Int3()
                    {
                        X = hull[left],
                        Y = hull[nright],
                        Z = hull[right],
                    });

                    right = nright;
                }
            }

            return tris.ToArray();
        }
    }
}
