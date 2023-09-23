using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    static class Utils
    {
        static readonly float EqualityTHR = (float)Math.Pow(1.0f / 16384.0f, 2);

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
        /// <summary>
        /// Removes n items from i position in the specified array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="i">Start position</param>
        /// <param name="n">Number of items</param>
        /// <returns>Returns the resulting array</returns>
        public static T[] RemoveRange<T>(T[] arr, int i, int n)
        {
            //Copy array
            var res = arr.ToArray();

            for (int k = i; k < n; k++)
            {
                res[k] = res[k + 1];
            }

            return res;
        }

        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }
        /// <summary>
        /// Derives the y-axis height of the closest point on the triangle from the specified reference point.
        /// </summary>
        /// <param name="p">The reference point from which to test.</param>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC.</param>
        /// <param name="c">Vertex C of triangle ABC.</param>
        /// <param name="h">The resulting height.</param>
        /// <returns>Returns true if point lies inside the triangle.</returns>
        public static bool ClosestHeightPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float h)
        {
            h = float.MaxValue;

            float EPS = 1e-6f;
            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(p, a);

            // Compute scaled barycentric coordinates
            float denom = v0.X * v1.Z - v0.Z * v1.X;
            if (Math.Abs(denom) < EPS)
            {
                return false;
            }

            float u = v1.Z * v2.X - v1.X * v2.Z;
            float v = v0.X * v2.Z - v0.Z * v2.X;

            if (denom < 0)
            {
                denom = -denom;
                u = -u;
                v = -v;
            }

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= 0.0f && v >= 0.0f && (u + v) <= denom)
            {
                h = a.Y + (v0.Y * u + v1.Y * v) / denom;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets if a point is into a polygon.
        /// </summary>
        /// <param name="p">Point to test</param>
        /// <param name="polygon">Polygon vertex list</param>
        /// <returns>Returns true if the point is into the polygon</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool PointInPolygon2D(Vector3 p, Vector3[] polygon)
        {
            bool c = false;

            int nvert = polygon.Length;

            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                var vi = polygon[i];
                var vj = polygon[j];

                if (((vi.Z > p.Z) != (vj.Z > p.Z)) &&
                    (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
            }

            return c;
        }
        /// <summary>
        /// Gets the cylinder bounds
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="r">Radius</param>
        /// <param name="h">Height</param>
        /// <param name="bmin">Resulting minimum bounding box point</param>
        /// <param name="bmax">Resulting maximum bounding box point</param>
        public static void GetCylinderBounds(Vector3 pos, float r, float h, out Vector3 bmin, out Vector3 bmax)
        {
            bmin.X = pos.X - r;
            bmin.Y = pos.Y;
            bmin.Z = pos.Z - r;
            bmax.X = pos.X + r;
            bmax.Y = pos.Y + h;
            bmax.Z = pos.Z + r;
        }
        /// <summary>
        /// Gets the polygon bounds
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="bmin">Resulting minimum bounding box point</param>
        /// <param name="bmax">Resulting maximum bounding box point</param>
        public static void GetPolygonBounds(Vector3[] polygon, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = polygon[0];
            bmax = polygon[0];

            for (int i = 1; i < polygon.Length; ++i)
            {
                bmin = Vector3.Min(bmin, polygon[i]);
                bmax = Vector3.Max(bmax, polygon[i]);
            }
        }
        /// <summary>
        /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
        /// </summary>
        /// <param name="a">Vertex A. [(x, y, z)]</param>
        /// <param name="b">Vertex B. [(x, y, z)]</param>
        /// <param name="c">Vertex C. [(x, y, z)]</param>
        /// <returns>The signed xz-plane area of the triangle</returns>
        public static float TriArea2D(Vector3 a, Vector3 b, Vector3 c)
        {
            return (c.X - a.X) * (b.Z - a.Z) - (b.X - a.X) * (c.Z - a.Z);
        }
        /// <summary>
        /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
        /// </summary>
        /// <param name="a">Vertex A. [(x, y, z)]</param>
        /// <param name="b">Vertex B. [(x, y, z)]</param>
        /// <param name="c">Vertex C. [(x, y, z)]</param>
        /// <returns>The signed xz-plane area of the triangle</returns>
        public static int TriArea2D(Int4 a, Int4 b, Int4 c)
        {
            return (c.X - a.X) * (b.Z - a.Z) - (b.X - a.X) * (c.Z - a.Z);
        }
        /// <summary>
        /// Gets whether the specified points are equals in the xz plane
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        public static bool VEqual2D(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        /// <summary>
        /// Gets if a point is into a polygon, an all the distances to the polygon edges
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="verts">Polygon vertex list</param>
        /// <param name="ed">Distance to edges array</param>
        /// <param name="et">Distance from first edge point to closest point list</param>
        /// <returns>Returns true if the point is into the polygon</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool DistancePtPolyEdgesSqr2D(Vector3 pt, Vector3[] verts, out float[] ed, out float[] et)
        {
            int nverts = verts.Length;

            ed = new float[nverts];
            et = new float[nverts];

            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                if (((vi.Z > pt.Z) != (vj.Z > pt.Z)) &&
                    (pt.X < (vj.X - vi.X) * (pt.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
                ed[j] = DistancePtSegSqr2D(pt, vj, vi, out et[j]);
            }
            return c;
        }
        /// <summary>
        /// Gets the minimum distance from the pt point to the (p,q) segment
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="p">P segment point</param>
        /// <param name="q">Q segment point</param>
        /// <returns>Returns the distance from pt to closest point</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float DistancePtSegSqr2D(Vector3 pt, Vector3 p, Vector3 q)
        {
            return DistancePtSegSqr2D(pt, p, q, out _);
        }
        /// <summary>
        /// Gets the minimum distance from the pt point to the (p,q) segment
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="p">P segment point</param>
        /// <param name="q">Q segment point</param>
        /// <param name="t">The distance from P to closest point</param>
        /// <returns>Returns the distance from pt to closest point</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float DistancePtSegSqr2D(Vector3 pt, Vector3 p, Vector3 q, out float t)
        {
            float pqx = q.X - p.X;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqz * pqz;
            t = pqx * dx + pqz * dz;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = p.X + t * pqx - pt.X;
            dz = p.Z + t * pqz - pt.Z;
            return dx * dx + dz * dz;
        }
        public static bool OverlapPolyPoly2D(Vector3[] polya, int npolya, Vector3[] polyb, int npolyb)
        {
            float eps = 1e-4f;

            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                var va = polya[j];
                var vb = polya[i];
                var n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly2D(n, polya, npolya, out float amin, out float amax);
                ProjectPoly2D(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                var va = polyb[j];
                var vb = polyb[i];
                var n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly2D(n, polya, npolya, out float amin, out float amax);
                ProjectPoly2D(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            return true;
        }
        public static void ProjectPoly2D(Vector3 axis, Vector3[] poly, int npoly, out float rmin, out float rmax)
        {
            rmin = rmax = Vector2.Dot(axis.XZ(), poly[0].XZ());
            for (int i = 1; i < npoly; ++i)
            {
                float d = Vector2.Dot(axis.XZ(), poly[i].XZ());
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }
        public static float VCross2D(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;
            return u1 * v2 - v1 * u2;
        }
        public static float VPerp2D(Vector3 u, Vector3 v)
        {
            return u.Z * v.X - u.X * v.Z;
        }
        public static int OverlapSegSeg2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = VCross2D(a, b, d);
            float a2 = VCross2D(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = VCross2D(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return 1;
                }
            }
            return 0;
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
        public static float DistPtTri2D(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
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
        public static float DistToPoly2D(Vector3[] verts, Vector3 p)
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
        public static float PolyMinExtent2D(Vector3[] verts)
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
        public static bool IntersectSegmentPoly2D(Vector3 p0, Vector3 p1, Vector3[] verts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            float EPS = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            Vector3 dir = Vector3.Subtract(p1, p0);

            for (int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
            {
                Vector3 edge = Vector3.Subtract(verts[i], verts[j]);
                Vector3 diff = Vector3.Subtract(p0, verts[j]);
                float n = VPerp2D(edge, diff);
                float d = VPerp2D(dir, edge);
                if (Math.Abs(d) < EPS)
                {
                    // S is nearly parallel to this edge
                    if (n < 0)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (!EvaluateSegment(j, n, d, ref tmin, ref tmax, ref segMin, ref segMax))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool IntersectSegments2D(Vector3 ap, Vector3 aq, Vector3 bp, Vector3 bq, out float s, out float t)
        {
            s = 0;
            t = 0;

            Vector3 u = Vector3.Subtract(aq, ap);
            Vector3 v = Vector3.Subtract(bq, bp);
            Vector3 w = Vector3.Subtract(ap, bp);
            float d = VPerp2D(u, v);
            if (Math.Abs(d) < 1e-6f) return false;
            s = VPerp2D(v, w) / d;
            t = VPerp2D(u, w) / d;
            return true;
        }

        /// <summary>
        /// Gets the closest point on the closest edge
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="edged">Distance to edges array</param>
        /// <param name="edget">Distance from first edge point to closest point list</param>
        /// <returns>Returns the closest position</returns>
        public static Vector3 GetClosestPointOutsidePoly(Vector3[] verts, float[] edged, float[] edget)
        {
            float dmin = edged[0];
            int imin = 0;
            for (int i = 1; i < verts.Length; i++)
            {
                if (edged[i] < dmin)
                {
                    dmin = edged[i];
                    imin = i;
                }
            }
            var va = verts[imin];
            var vb = verts[(imin + 1) % verts.Length];
            return Vector3.Lerp(va, vb, edget[imin]);
        }
        /// <summary>
        /// Returns a random point in a convex polygon.
        /// Adapted from Graphics Gems article.
        /// </summary>
        public static void RandomPointInConvexPoly(Vector3[] pts, out float[] areas, float s, float t, out Vector3 outPoint)
        {
            areas = new float[IndexedPolygon.DT_VERTS_PER_POLYGON];

            // Calc triangle areas
            float areasum = 0.0f;
            for (int i = 2; i < pts.Length; i++)
            {
                areas[i] = TriArea2D(pts[0], pts[i - 1], pts[i]);
                areasum += Math.Max(0.001f, areas[i]);
            }
            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = pts.Length - 1;
            for (int i = 2; i < pts.Length; i++)
            {
                float dacc = areas[i];
                if (thr >= acc && thr < (acc + dacc))
                {
                    u = (thr - acc) / dacc;
                    tri = i;
                    break;
                }
                acc += dacc;
            }

            float v = (float)Math.Sqrt(t);

            float a = 1 - v;
            float b = (1 - u) * v;
            float c = u * v;
            Vector3 pa = pts[0];
            Vector3 pb = pts[tri - 1];
            Vector3 pc = pts[tri];

            outPoint = a * pa + b * pb + c * pc;
        }
        /// <summary>
        /// Gets whether the specified points are closest enough to be nearest equal
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        public static bool VClosest(Vector3 a, Vector3 b)
        {
            return Vector3.DistanceSquared(a, b) < EqualityTHR;
        }
        private static bool EvaluateSegment(int index, float n, float d, ref float tmin, ref float tmax, ref int segMin, ref int segMax)
        {
            float t = n / d;
            if (d < 0)
            {
                // segment S is entering across this edge
                if (t > tmin)
                {
                    tmin = t;
                    segMin = index;
                    // S enters after leaving polygon
                    if (tmin > tmax)
                    {
                        return false;
                    }
                }
            }
            else
            {
                // segment S is leaving across this edge
                if (t < tmax)
                {
                    tmax = t;
                    segMax = index;
                    // S leaves before entering polygon
                    if (tmax < tmin)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Determines if two axis-aligned bounding boxes overlap.
        /// </summary>
        /// <param name="amin">Minimum bounds of box A</param>
        /// <param name="amax">Maximum bounds of box A</param>
        /// <param name="bmin">Minimum bounds of box B</param>
        /// <param name="bmax">Maximum bounds of box B</param>
        /// <returns>True if the two AABB's overlap</returns>
        public static bool OverlapQuantBounds(Int3 amin, Int3 amax, Int3 bmin, Int3 bmax)
        {
            return
                !(amin.X > bmax.X || amax.X < bmin.X) &&
                !(amin.Y > bmax.Y || amax.Y < bmin.Y) &&
                !(amin.Z > bmax.Z || amax.Z < bmin.Z);
        }
        /// <summary>
        /// Determines if two axis-aligned bounding boxes overlap.
        /// </summary>
        /// <param name="amin">Minimum bounds of box A</param>
        /// <param name="amax">Maximum bounds of box A</param>
        /// <param name="bmin">Minimum bounds of box B</param>
        /// <param name="bmax">Maximum bounds of box B</param>
        /// <returns>True if the two AABB's overlap.</returns>
        public static bool OverlapBounds(Vector3 amin, Vector3 amax, Vector3 bmin, Vector3 bmax)
        {
            return
                !(amin.X > bmax.X || amax.X < bmin.X) &&
                !(amin.Y > bmax.Y || amax.Y < bmin.Y) &&
                !(amin.Z > bmax.Z || amax.Z < bmin.Z);
        }
        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return !((amin + eps) > bmax || (amax - eps) < bmin);
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
        public static float DistToTriMesh(Vector3[] verts, Int3[] triPoints, Vector3 p)
        {
            float dmin = float.MaxValue;

            foreach (var tri in triPoints)
            {
                var va = verts[tri.X];
                var vb = verts[tri.Y];
                var vc = verts[tri.Z];

                float d = DistPtTri2D(p, va, vb, vc);
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
    }
}
