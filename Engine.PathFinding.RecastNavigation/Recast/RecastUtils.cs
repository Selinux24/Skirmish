using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    static class RecastUtils
    {
        private static readonly int[] OffsetsX = new[] { -1, 0, 1, 0, };
        private static readonly int[] OffsetsY = new[] { 0, 1, 0, -1 };
        private static readonly int[] OffsetsDir = new[] { 3, 0, -1, 2, 1 };

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
            return DistancePtSeg2D(pt.X, pt.Z, p.X, p.Z, q.X, q.Z);
        }
        public static float DistancePtSeg2D(float ptx, float ptz, float px, float pz, float qx, float qz)
        {
            float pqx = qx - px;
            float pqz = qz - pz;
            float dx = ptx - px;
            float dz = ptz - pz;
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
        public static float DistToTriMesh(Vector3[] verts, Int3[] triPoints, Vector3 p)
        {
            float dmin = float.MaxValue;

            foreach (var tri in triPoints)
            {
                var va = verts[tri.X];
                var vb = verts[tri.Y];
                var vc = verts[tri.Z];

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
        public static float DistToPoly(Vector3[] verts, Vector3 p)
        {
            float dmin = float.MaxValue;
            int nvert = verts.Length;
            bool c = false;
            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                if (((vi.Z > p.Z) != (vj.Z > p.Z)) && (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
                dmin = Math.Min(dmin, DistancePtSeg2D(p, vj, vi));
            }
            return c ? -dmin : dmin;
        }
        public static float PolyMinExtent(Vector3[] verts)
        {
            float minDist = float.MaxValue;

            for (int i = 0; i < verts.Length; i++)
            {
                int ni = (i + 1) % verts.Length;

                var p1 = verts[i];
                var p2 = verts[ni];

                float maxEdgeDist = 0;
                for (int j = 0; j < verts.Length; j++)
                {
                    if (j == i || j == ni)
                    {
                        continue;
                    }

                    float d = DistancePtSeg2D(verts[j], p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }

                minDist = Math.Min(minDist, maxEdgeDist);
            }

            return (float)Math.Sqrt(minDist);
        }
    }
}
