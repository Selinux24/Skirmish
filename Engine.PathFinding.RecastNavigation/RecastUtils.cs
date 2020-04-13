using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    static class RecastUtils
    {
        #region Constants

        /// <summary>
        /// Defines the number of bits allocated to rcSpan::smin and rcSpan::smax.
        /// </summary>
        public const int RC_SPAN_HEIGHT_BITS = 13;
        /// <summary>
        /// Defines the maximum value for rcSpan::smin and rcSpan::smax.
        /// </summary>
        public const int RC_SPAN_MAX_HEIGHT = (1 << RC_SPAN_HEIGHT_BITS) - 1;
        /// <summary>
        /// The number of spans allocated per span spool.
        /// </summary>
        public const int RC_SPANS_PER_POOL = 2048;
        /// <summary>
        /// Heighfield border flag.
        /// If a heightfield region ID has this bit set, then the region is a border 
        /// region and its spans are considered unwalkable.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_BORDER_REG = 0x8000;
        /// <summary>
        /// Polygon touches multiple regions.
        /// If a polygon has this region ID it was merged with or created
        /// from polygons of different regions during the polymesh
        /// build step that removes redundant border vertices. 
        /// (Used during the polymesh and detail polymesh build processes)
        /// </summary>
        public const int RC_MULTIPLE_REGS = 0;
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int RC_BORDER_VERTEX = 0x10000;
        /// <summary>
        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_AREA_BORDER = 0x20000;
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        public const int RC_CONTOUR_REG_MASK = 0xffff;
        /// <summary>
        /// An value which indicates an invalid index within a mesh.
        /// </summary>
        public const int RC_MESH_NULL_IDX = -1;

        public const int RC_NULL_NEI = -1;
        public const int RC_UNSET_HEIGHT = 0xffff;

        public const int VERTEX_BUCKET_COUNT = (1 << 12);

        #endregion

        #region RECASTAREA

        public static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        public static int RotateCW(int dir)
        {
            return (dir + 1) & 0x3;
        }
        public static int RotateCCW(int dir)
        {
            return (dir + 3) & 0x3;
        }

        #endregion

        #region RECASTCONTOUR

        public static float DistancePtSeg2D(int x, int z, int px, int pz, int qx, int qz)
        {
            float pqx = (qx - px);
            float pqz = (qz - pz);
            float dx = (x - px);
            float dz = (z - pz);
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

            dx = px + t * pqx - x;
            dz = pz + t * pqz - z;

            return dx * dx + dz * dz;
        }

        #endregion

        #region RECASTMESH

        public static bool DiagonalieLoose(int i, int j, int n, Int4[] verts, int[] indices)
        {
            Int4 d0 = verts[(indices[i] & 0x0fffffff)];
            Int4 d1 = verts[(indices[j] & 0x0fffffff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    Int4 p0 = verts[(indices[k] & 0x0fffffff)];
                    Int4 p1 = verts[(indices[k1] & 0x0fffffff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                    {
                        continue;
                    }

                    if (IntersectProp(d0, d1, p0, p1))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool InConeLoose(int i, int j, int n, Int4[] verts, int[] indices)
        {
            Int4 pi = verts[(indices[i] & 0x0fffffff) * 4];
            Int4 pj = verts[(indices[j] & 0x0fffffff) * 4];
            Int4 pi1 = verts[(indices[Next(i, n)] & 0x0fffffff)];
            Int4 pin1 = verts[(indices[Prev(i, n)] & 0x0fffffff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return LeftOn(pi, pj, pin1) && LeftOn(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        public static int Triangulate(Int4[] verts, out int[] indices, out Int3[] tris)
        {
            int n = verts.Length;
            indices = PrepareIndices(verts);

            List<Int3> dst = new List<Int3>();

            while (n > 3)
            {
                int mini = FindMin(indices, n, verts);
                if (mini == -1)
                {
                    // Should not happen.
                    tris = null;
                    return -dst.Count;
                }

                int i0 = mini;
                int i1 = Next(i0, n);
                int i2 = Next(i1, n);

                dst.Add(new Int3()
                {
                    X = indices[i0] & 0x7fff,
                    Y = indices[i1] & 0x7fff,
                    Z = indices[i2] & 0x7fff
                });

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                for (int k = i1; k < n; k++)
                {
                    indices[k] = indices[k + 1];
                }

                if (i1 >= n)
                {
                    i1 = 0;
                }

                // Update diagonal flags.
                i0 = Prev(i1, n);
                if (Diagonal(Prev(i0, n), i1, n, verts, indices))
                {
                    indices[i0] |= 0x8000;
                }
                else
                {
                    indices[i0] &= 0x7fff;
                }

                if (Diagonal(i0, Next(i1, n), n, verts, indices))
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
        private static int[] PrepareIndices(Int4[] verts)
        {
            int n = verts.Length;

            int[] indices = new int[n];
            for (int j = 0; j < n; ++j)
            {
                indices[j] = j;
            }

            // The last bit of the index is used to indicate if the vertex can be removed.
            for (int i = 0; i < n; i++)
            {
                int i1 = Next(i, n);
                int i2 = Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }

            return indices;
        }
        private static int FindMin(int[] indices, int n, Int4[] verts)
        {
            int mini = -1;
            int minLen = -1;
            for (int ix = 0; ix < n; ix++)
            {
                int i1x = Next(ix, n);
                if ((indices[i1x] & 0x8000) != 0)
                {
                    var p0 = verts[indices[ix] & 0x7fff];
                    var p2 = verts[indices[Next(i1x, n)] & 0x7fff];

                    int dx = p2.X - p0.X;
                    int dz = p2.Z - p0.Z;
                    int len = dx * dx + dz * dz;
                    if (minLen < 0 || len < minLen)
                    {
                        mini = ix;
                        minLen = len;
                    }
                }
            }

            return mini;
        }
        public static void PushFront<T>(T v, T[] arr, ref int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = v;
        }
        public static void PushBack<T>(T v, T[] arr, ref int an)
        {
            arr[an] = v;
            an++;
        }

        #endregion

        #region RECASTMESHDETAIL

        public static float VDot2(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Z * b.Z;
        }
        public static float VDistSq2(Vector3 p, Vector3 q)
        {
            float dx = q.X - p.X;
            float dy = q.Z - p.Z;

            return dx * dx + dy * dy;
        }
        public static float VDist2(Vector3 p, Vector3 q)
        {
            return (float)Math.Sqrt(VDistSq2(p, q));
        }
        public static float VCross2(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;

            return u1 * v2 - v1 * u2;
        }
        public static float DistPtTri(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0, v1, v2;
            v0 = Vector3.Subtract(c, a);
            v1 = Vector3.Subtract(b, a);
            v2 = Vector3.Subtract(p, a);

            float dot00 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v0.X, v0.Z));
            float dot01 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v1.X, v1.Z));
            float dot02 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v2.X, v2.Z));
            float dot11 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v1.X, v1.Z));
            float dot12 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));

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
        public static float DistancePtSeg2d(Vector3 pt, Vector3 p, Vector3 q)
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

        #endregion

        #region COMMON / REPEATED

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
        public static int Area2(Int4 a, Int4 b, Int4 c)
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
        public static bool Xorb(bool x, bool y)
        {
            return !x ^ !y;
        }
        public static bool Left(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) < 0;
        }
        public static bool LeftOn(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) <= 0;
        }
        public static bool Collinear(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) == 0;
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
        public static bool IntersectProp(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
            {
                return false;
            }

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="aV"></param>
        /// <param name="bV"></param>
        /// <param name="cV"></param>
        /// <returns></returns>
        public static bool Between(Int4 aV, Int4 bV, Int4 cV)
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
            else if (Between(a, b, c) || Between(a, b, d) ||
                     Between(c, d, a) || Between(c, d, b))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool Vequal(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        public static bool Diagonalie(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var d0 = verts[(indices[i] & 0x7fff)];
            var d1 = verts[(indices[j] & 0x7fff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts[(indices[k] & 0x7fff)];
                    var p1 = verts[(indices[k1] & 0x7fff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                    {
                        continue;
                    }

                    if (Intersect(d0, d1, p0, p1))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool InCone(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var pi = verts[(indices[i] & 0x7fff)];
            var pj = verts[(indices[j] & 0x7fff)];
            var pi1 = verts[(indices[Next(i, n)] & 0x7fff)];
            var pin1 = verts[(indices[Prev(i, n)] & 0x7fff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
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
        public static bool Diagonal(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }

        #endregion
    }
}
