using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    static class TriangulateHelper
    {
        /// <summary>
        /// Gets the next index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the next index</returns>
        public static int Next(int i, int length)
        {
            return i + 1 < length ? i + 1 : 0;
        }
        /// <summary>
        /// Gets the previous index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the previous index</returns>
        public static int Prev(int i, int length)
        {
            return i - 1 >= 0 ? i - 1 : length - 1;
        }

        public static int Triangulate(Int4[] verts, ref int[] indices, out Int3[] tris)
        {
            var dst = new List<Int3>();

            // The last bit of the index is used to indicate if the vertex can be removed.
            SetRemovableIndices(verts, ref indices);

            int n = verts.Length;
            while (n > 3)
            {
                int mini = FindMinIndex(n, verts, indices);

                if (mini == -1)
                {
                    // Should not happen.
                    tris = null;
                    return -dst.Count;
                }

                int i = mini;
                int i1 = Next(i, n);
                int i2 = Next(i1, n);

                dst.Add(new Int3()
                {
                    X = indices[i] & 0x7fff,
                    Y = indices[i1] & 0x7fff,
                    Z = indices[i2] & 0x7fff
                });

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                RemoveIndex(i1, n, ref indices);

                if (i1 >= n)
                {
                    i1 = 0;
                }

                i = Prev(i1, n);

                // Update diagonal flags.
                if (Diagonal(Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= 0x8000;
                }
                else
                {
                    indices[i] &= 0x7fff;
                }

                if (Diagonal(i, Next(i1, n), n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
                else
                {
                    indices[i1] &= 0x7fff;
                }
            }

            // Append the remaining triangle.
            dst.Add(new Int3
            {
                X = indices[0] & 0x7fff,
                Y = indices[1] & 0x7fff,
                Z = indices[2] & 0x7fff,
            });

            tris = dst.ToArray();

            return dst.Count;
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

                int pi = Prev(i, nhull);
                int ni = Next(i, nhull);
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
            while (Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = Next(left, nhull);
                int nright = Prev(right, nhull);

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
        public static bool Intersect(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            if (IntersectProp(a, b, c, d))
            {
                return true;
            }
            else if (Between(a, b, c) || Between(a, b, d) || Between(c, d, a) || Between(c, d, b))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        /// <param name="verts"></param>
        /// <param name="pj"></param>
        /// <returns></returns>
        public static bool InCone(int i, int n, Int4[] verts, Int4 pj)
        {
            var pi = verts[i];
            var pi1 = verts[Next(i, n)];
            var pin1 = verts[Prev(i, n)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }

        private static void SetRemovableIndices(Int4[] verts, ref int[] indices)
        {
            int n = verts.Length;

            for (int i = 0; i < n; i++)
            {
                int i1 = Next(i, n);
                int i2 = Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }
        }
        private static int FindMinIndex(int n, Int4[] verts, int[] indices)
        {
            int minLen = -1;
            int mini = -1;

            for (int ix = 0; ix < n; ix++)
            {
                int i1x = Next(ix, n);

                if ((indices[i1x] & 0x8000) == 0)
                {
                    continue;
                }

                var p0 = verts[indices[ix] & 0x7fff];
                var p2 = verts[indices[Next(i1x, n)] & 0x7fff];

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
        private static void RemoveIndex(int i1, int n, ref int[] indices)
        {
            for (int k = i1; k < n; k++)
            {
                indices[k] = indices[k + 1];
            }
        }
        private static bool VEqual(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        private static bool Diagonal(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        private static bool Diagonalie(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var d0 = verts[indices[i] & 0x7fff];
            var d1 = verts[indices[j] & 0x7fff];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts[indices[k] & 0x7fff];
                    var p1 = verts[indices[k1] & 0x7fff];

                    if (VEqual(d0, p0) || VEqual(d1, p0) || VEqual(d0, p1) || VEqual(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }

            return true;
        }
        private static bool InCone(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var pi = verts[indices[i] & 0x7fff];
            var pj = verts[indices[j] & 0x7fff];
            var pi1 = verts[indices[Next(i, n)] & 0x7fff];
            var pin1 = verts[indices[Prev(i, n)] & 0x7fff];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        private static bool Left(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) < 0;
        }
        private static bool LeftOn(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) <= 0;
        }
        private static int Area2(Int4 a, Int4 b, Int4 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="aV"></param>
        /// <param name="bV"></param>
        /// <param name="cV"></param>
        /// <returns></returns>
        private static bool Between(Int4 aV, Int4 bV, Int4 cV)
        {
            if (!Collinear(aV, bV, cV))
            {
                return false;
            }

            // If ab not vertical, check betweenness on x; else on y.
            if (aV.X != bV.X)
            {
                return ((aV.X <= cV.X) && (cV.X <= bV.X)) || ((aV.X >= cV.X) && (cV.X >= bV.X));
            }
            else
            {
                return ((aV.Z <= cV.Z) && (cV.Z <= bV.Z)) || ((aV.Z >= cV.Z) && (cV.Z >= bV.Z));
            }
        }
        /// <summary>
        /// Returns true iff ab properly intersects cd: they share 
        /// a point interior to both segments.
        /// The properness of the intersection is ensured by using strict leftness.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool IntersectProp(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
                return false;

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        /// <summary>
        /// Exclusive or: true iff exactly one argument is true.
        /// The arguments are negated to ensure that they are 0/1 values.
        /// Then the bitwise Xor operator may apply.
        /// (This idea is due to Michael Baldwin.)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool Xorb(bool x, bool y)
        {
            return !x ^ !y;
        }
        private static bool Collinear(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) == 0;
        }
    }
}
