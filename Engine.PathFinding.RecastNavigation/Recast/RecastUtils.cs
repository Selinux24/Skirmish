using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    static class RecastUtils
    {
        private static readonly int[] OffsetsX = new[] { -1, 0, 1, 0, };
        private static readonly int[] OffsetsY = new[] { 0, 1, 0, -1 };
        private static readonly int[] OffsetsDir = new[] { 3, 0, -1, 2, 1 };

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
        /// <summary>
        /// Pushes the specified item in front of the array
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="v">Item</param>
        /// <param name="arr">Array</param>
        /// <param name="an">Array size</param>
        public static void PushFront<T>(T v, T[] arr, ref int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = v;
        }
        /// <summary>
        /// Pushes the specified item int the array's back position
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="v">Item</param>
        /// <param name="arr">Array</param>
        /// <param name="an">Array size</param>
        public static void PushBack<T>(T v, T[] arr, ref int an)
        {
            arr[an] = v;
            an++;
        }

        public static int GetDirOffsetX(int dir)
        {
            return OffsetsX[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            return OffsetsY[dir & 0x03];
        }
        public static int GetDirForOffset(int x, int y)
        {
            return OffsetsDir[((y + 1) << 1) + x];
        }

        public static int CalcAreaOfPolygon2D(IEnumerable<Int4> verts, int nverts)
        {
            int area = 0;

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts.ElementAt(i);
                var vj = verts.ElementAt(j);
                area += vi.X * vj.Z - vj.X * vi.Z;
            }

            return (area + 1) / 2;
        }

        public static int Triangulate(IEnumerable<Int4> verts, ref int[] indices, out IEnumerable<Int3> tris)
        {
            List<Int3> dst = new List<Int3>();

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
        private static void SetRemovableIndices(IEnumerable<Int4> verts, ref int[] indices)
        {
            int n = verts.Count();

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
        private static int FindMinIndex(int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            int minLen = -1;
            int mini = -1;

            for (int ix = 0; ix < n; ix++)
            {
                int i1x = Next(ix, n);

                if ((indices.ElementAt(i1x) & 0x8000) == 0)
                {
                    continue;
                }

                var p0 = verts.ElementAt(indices.ElementAt(ix) & 0x7fff);
                var p2 = verts.ElementAt(indices.ElementAt(Next(i1x, n)) & 0x7fff);

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
        private static int Area2(Int4 a, Int4 b, Int4 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }

        private static float DistPtTri(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(p, a);

            float dot00 = Vector2.Dot(v0.XZ(), v0.XZ());
            float dot01 = Vector2.Dot(v0.XZ(), v1.XZ());
            float dot02 = Vector2.Dot(v0.XZ(), v2.XZ());
            float dot11 = Vector2.Dot(v1.XZ(), v1.XZ());
            float dot12 = Vector2.Dot(v1.XZ(), v2.XZ());

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // If point lies inside the triangle, return interpolated y-coord.
            float EPS = float.Epsilon;
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                float y = a.Y + v0.Y * u + v1.Y * v;

                return Math.Abs(y - p.Y);
            }

            return float.MaxValue;
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
        private static bool Left(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) < 0;
        }
        private static bool LeftOn(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) <= 0;
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
        private static bool Collinear(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) == 0;
        }
        private static bool Diagonalie(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            var d0 = verts.ElementAt(indices.ElementAt(i) & 0x7fff);
            var d1 = verts.ElementAt(indices.ElementAt(j) & 0x7fff);

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
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
        private static bool InCone(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            var pi = verts.ElementAt(indices.ElementAt(i) & 0x7fff);
            var pj = verts.ElementAt(indices.ElementAt(j) & 0x7fff);
            var pi1 = verts.ElementAt(indices.ElementAt(Next(i, n)) & 0x7fff);
            var pin1 = verts.ElementAt(indices.ElementAt(Prev(i, n)) & 0x7fff);

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        private static bool Diagonal(int i, int j, int n, IEnumerable<Int4> verts, IEnumerable<int> indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }

        public static float VCross2(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;
            return u1 * v2 - v1 * u2;
        }
        public static int OverlapSegSeg2d(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = VCross2(a, b, d);
            float a2 = VCross2(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = VCross2(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return 1;
                }
            }
            return 0;
        }
        public static float DistancePtSeg(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqy = q.Y - p.Y;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dy = pt.Y - p.Y;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqy * pqy + pqz * pqz;
            float t = pqx * dx + pqy * dy + pqz * dz;

            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p.X + t * pqx - pt.X;
            dy = p.Y + t * pqy - pt.Y;
            dz = p.Z + t * pqz - pt.Z;

            return dx * dx + dy * dy + dz * dz;
        }
        public static float DistancePtSeg2D(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;

            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p.X + t * pqx - pt.X;
            dz = p.Z + t * pqz - pt.Z;

            return dx * dx + dz * dz;
        }
        public static float DistancePtSeg2D(int ptx, int ptz, int px, int pz, int qx, int qz)
        {
            float pqx = (qx - px);
            float pqz = (qz - pz);
            float dx = (ptx - px);
            float dz = (ptz - pz);
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;

            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = px + t * pqx - ptx;
            dz = pz + t * pqz - ptz;

            return dx * dx + dz * dz;
        }
        public static float DistToTriMesh(IEnumerable<Vector3> verts, IEnumerable<Int3> triPoints, Vector3 p)
        {
            float dmin = float.MaxValue;

            foreach (var tri in triPoints)
            {
                var va = verts.ElementAt(tri.X);
                var vb = verts.ElementAt(tri.Y);
                var vc = verts.ElementAt(tri.Z);

                float d = DistPtTri(p, va, vb, vc);
                if (d < dmin)
                {
                    dmin = d;
                }
            }

            if (dmin == float.MaxValue)
            {
                return -1;
            }

            return dmin;
        }
        public static float DistToPoly(IEnumerable<Vector3> verts, Vector3 p)
        {
            float dmin = float.MaxValue;
            int nvert = verts.Count();
            bool c = false;
            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                Vector3 vi = verts.ElementAt(i);
                Vector3 vj = verts.ElementAt(j);
                if (((vi.Z > p.Z) != (vj.Z > p.Z)) && (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
                dmin = Math.Min(dmin, DistancePtSeg2D(p, vj, vi));
            }
            return c ? -dmin : dmin;
        }
        public static float PolyMinExtent(IEnumerable<Vector3> verts)
        {
            float minDist = float.MaxValue;

            for (int i = 0; i < verts.Count(); i++)
            {
                int ni = (i + 1) % verts.Count();

                Vector3 p1 = verts.ElementAt(i);
                Vector3 p2 = verts.ElementAt(ni);

                float maxEdgeDist = 0;
                for (int j = 0; j < verts.Count(); j++)
                {
                    if (j == i || j == ni)
                    {
                        continue;
                    }

                    float d = DistancePtSeg2D(verts.ElementAt(j), p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }

                minDist = Math.Min(minDist, maxEdgeDist);
            }

            return (float)Math.Sqrt(minDist);
        }
        public static IEnumerable<Int3> TriangulateHull(IEnumerable<Vector3> verts, IEnumerable<int> hull)
        {
            List<Int3> tris = new List<Int3>();

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

                int pi = Prev(i, nhull);
                int ni = Next(i, nhull);
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
            while (Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = Next(left, nhull);
                int nright = Prev(right, nhull);

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
        public static bool InCone(int i, int n, IEnumerable<Int4> verts, Int4 pj)
        {
            var pi = verts.ElementAt(i);
            var pi1 = verts.ElementAt(Next(i, n));
            var pin1 = verts.ElementAt(Prev(i, n));

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
    }
}
