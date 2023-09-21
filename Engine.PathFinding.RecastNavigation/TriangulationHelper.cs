using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    static class TriangulationHelper
    {
        public static int Triangulate(IEnumerable<Int4> verts, ref int[] indices, out IEnumerable<Int3> tris)
        {
            var dst = new List<Int3>();

            // The last bit of the index is used to indicate if the vertex can be removed.
            SetRemovableIndices(verts, ref indices);

            int n = verts.Count();
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
                int i1 = Utils.Next(i, n);
                int i2 = Utils.Next(i1, n);

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

                i = Utils.Prev(i1, n);

                // Update diagonal flags.
                if (Diagonal(Utils.Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= 0x8000;
                }
                else
                {
                    indices[i] &= 0x7fff;
                }

                if (Diagonal(i, Utils.Next(i1, n), n, verts, indices))
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
        private static void SetRemovableIndices(IEnumerable<Int4> verts, ref int[] indices)
        {
            int n = verts.Count();

            for (int i = 0; i < n; i++)
            {
                int i1 = Utils.Next(i, n);
                int i2 = Utils.Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }
        }
        private static int FindMinIndex(int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            int minLen = -1;
            int mini = -1;

            for (int ix = 0; ix < n; ix++)
            {
                int i1x = Utils.Next(ix, n);

                if ((indices.ElementAt(i1x) & 0x8000) == 0)
                {
                    continue;
                }

                var p0 = verts.ElementAt(indices.ElementAt(ix) & 0x7fff);
                var p2 = verts.ElementAt(indices.ElementAt(Utils.Next(i1x, n)) & 0x7fff);

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
        private static bool Diagonal(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        private static bool InCone(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            var pi = verts.ElementAt(indices.ElementAt(i) & 0x7fff);
            var pj = verts.ElementAt(indices.ElementAt(j) & 0x7fff);
            var pi1 = verts.ElementAt(indices.ElementAt(Utils.Next(i, n)) & 0x7fff);
            var pin1 = verts.ElementAt(indices.ElementAt(Utils.Prev(i, n)) & 0x7fff);

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        private static bool Diagonalie(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            var d0 = verts.ElementAt(indices.ElementAt(i) & 0x7fff);
            var d1 = verts.ElementAt(indices.ElementAt(j) & 0x7fff);

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Utils.Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts.ElementAt(indices.ElementAt(k) & 0x7fff);
                    var p1 = verts.ElementAt(indices.ElementAt(k1) & 0x7fff);

                    if (VEqual(d0, p0) || VEqual(d1, p0) || VEqual(d0, p1) || VEqual(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
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
        private static bool Collinear(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) == 0;
        }
        private static int Area2(Int4 a, Int4 b, Int4 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
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
        private static bool VEqual(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        private static bool Left(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) < 0;
        }
        private static bool LeftOn(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) <= 0;
        }
        public static bool InCone(int i, int n, IEnumerable<Int4> verts, Int4 pj)
        {
            var pi = verts.ElementAt(i);
            var pi1 = verts.ElementAt(Utils.Next(i, n));
            var pin1 = verts.ElementAt(Utils.Prev(i, n));

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }

        public static IEnumerable<Int3> TriangulateHull(IEnumerable<Vector3> verts, IEnumerable<int> hull)
        {
            var tris = new List<Int3>();

            int nhull = hull.Count();
            int nin = verts.Count();

            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = float.MaxValue;
            for (int i = 0; i < nhull; i++)
            {
                if (hull.ElementAt(i) >= nin)
                {
                    continue; // Ears are triangles with original vertices as middle vertex while others are actually line segments on edges
                }

                int pi = Utils.Prev(i, nhull);
                int ni = Utils.Next(i, nhull);
                var pv = verts.ElementAt(hull.ElementAt(pi));
                var cv = verts.ElementAt(hull.ElementAt(i));
                var nv = verts.ElementAt(hull.ElementAt(ni));

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
                X = hull.ElementAt(start),
                Y = hull.ElementAt(left),
                Z = hull.ElementAt(right),
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

                var cvleft = verts.ElementAt(hull.ElementAt(left));
                var nvleft = verts.ElementAt(hull.ElementAt(nleft));
                var cvright = verts.ElementAt(hull.ElementAt(right));
                var nvright = verts.ElementAt(hull.ElementAt(nright));
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
                        X = hull.ElementAt(left),
                        Y = hull.ElementAt(nleft),
                        Z = hull.ElementAt(right),
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new Int3()
                    {
                        X = hull.ElementAt(left),
                        Y = hull.ElementAt(nright),
                        Z = hull.ElementAt(right),
                    });

                    right = nright;
                }
            }

            return tris.ToArray();
        }
    }
}
