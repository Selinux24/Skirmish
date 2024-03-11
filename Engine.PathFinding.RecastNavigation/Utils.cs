using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Utils
    /// </summary>
    static class Utils
    {
        static readonly float EqualityTHR = (float)Math.Pow(1.0f / 16384.0f, 2);

        /// <summary>
        /// Zero tolerance step
        /// </summary>
        public const float ZeroTolerance = 1e-6f;

        const uint HDX = 0x8da6b343;
        const uint HDY = 0xd8163841;
        const uint HDZ = 0xcb1ab31f;

        /// <summary>
        /// Computes a tile hash
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="mask">Tile lut mask</param>
        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = HDX; // Large multiplicative constants
            uint h2 = HDY; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }
        /// <summary>
        /// Computes the vertex hash
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <returns>Returns the hash value</returns>
        /// <remarks>
        /// Using the vertex coordinates, calculates a unique bucket number for easy storing and retrieval.
        /// </remarks>
        public static int ComputeVertexHash(int x, int y, int z, int mask)
        {
            uint h1 = HDX; // Large multiplicative constants
            uint h2 = HDY; // here arbitrarily chosen primes
            uint h3 = HDZ;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & mask);
        }

        public static float GetJitterX(int i)
        {
            return GetJitter(i, HDX);
        }
        public static float GetJitterY(int i)
        {
            return GetJitter(i, HDY);
        }
        public static float GetJitterZ(int i)
        {
            return GetJitter(i, HDZ);
        }
        public static float GetJitter(int i, uint v)
        {
            return (((i * v) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
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

                if (TestPtEdges2D(p, vi, vj))
                {
                    c = !c;
                }
            }

            return c;
        }
        /// <summary>
        /// Gets if a point is into a polygon, an all the distances to the polygon edges
        /// </summary>
        /// <param name="p">Point to test</param>
        /// <param name="polygon">Polygon vertex list</param>
        /// <param name="ed">Distance to edges array</param>
        /// <param name="et">Distance from first edge point to closest point list</param>
        /// <returns>Returns true if the point is into the polygon</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool PointInPolygon2D(Vector3 p, Vector3[] polygon, out float[] ed, out float[] et)
        {
            bool c = false;

            int nverts = polygon.Length;
            ed = new float[nverts];
            et = new float[nverts];

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = polygon[i];
                var vj = polygon[j];

                if (TestPtEdges2D(p, vi, vj))
                {
                    c = !c;
                }

                ed[j] = DistancePtSegSqr2D(p, vj, vi, out et[j]);
            }

            return c;
        }
        /// <summary>
        /// Tests point to edges
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="vi">First vertex</param>
        /// <param name="vj">Second vertex</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        private static bool TestPtEdges2D(Vector3 p, Vector3 vi, Vector3 vj)
        {
            if (((vi.Z > p.Z) != (vj.Z > p.Z)) &&
                (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
        /// </summary>
        /// <param name="a">Vertex A. [(x, y, z)]</param>
        /// <param name="b">Vertex B. [(x, y, z)]</param>
        /// <param name="c">Vertex C. [(x, y, z)]</param>
        /// <returns>The signed xz-plane area of the triangle</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
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
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static int TriArea2D(Int3 a, Int3 b, Int3 c)
        {
            return (c.X - a.X) * (b.Z - a.Z) - (b.X - a.X) * (c.Z - a.Z);
        }
        /// <summary>
        /// Gets whether the specified points are equals in the xz plane
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool VEqual2D(Int3 a, Int3 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        /// <summary>
        /// Gets the squared distance between two points in the xz plane
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static int DistanceSqr2D(Int3 a, Int3 b)
        {
            int dx = b.X - a.X;
            int dz = b.Z - a.Z;
            return dx * dx + dz * dz;
        }
        /// <summary>
        /// Gets the squared distance between two points in the xz plane
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float DistanceSqr2D(Vector3 a, Vector3 b)
        {
            float dx = b.X - a.X;
            float dz = b.Z - a.Z;
            return dx * dx + dz * dz;
        }
        /// <summary>
        /// Gets the distance between two points in the xz plane
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float Distance2D(Vector3 a, Vector3 b)
        {
            float dx = b.X - a.X;
            float dz = b.Z - a.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Gets the squared minimum distance from the pt point to the (p,q) segment
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
        /// Gets the squared minimum distance from the pt point to the (p,q) segment
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
        /// <summary>
        /// Gets the squared minimum distance from the pt point to the (p,q) segment
        /// </summary>
        /// <param name="ptx">X point coordinate to test</param>
        /// <param name="ptz">Y point coordinate to test</param>
        /// <param name="px">P segment x point coordinate</param>
        /// <param name="pz">P segment y point coordinate</param>
        /// <param name="qx">Q segment x point coordinate</param>
        /// <param name="qz">Q segment y point coordinate</param>
        /// <returns>Returns the distance from pt to closest point</returns>
        public static float DistancePtSegSqr2D(int ptx, int ptz, int px, int pz, int qx, int qz)
        {
            float pqx = qx - px;
            float pqz = qz - pz;
            float dx = ptx - px;
            float dz = ptz - pz;
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = px + t * pqx - ptx;
            dz = pz + t * pqz - ptz;
            return dx * dx + dz * dz;
        }
        /// <summary>
        /// Gets the minimum distance from the pt point to the (a,b,c) triangle
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="a">Triangle point A</param>
        /// <param name="b">Triangle point B</param>
        /// <param name="c">Triangle point C</param>
        /// <returns>Returns the distance from pt to closest point</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float DistancePtTri2D(Vector3 pt, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(pt, a);

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
            const float eps = 1e-4f;
            if (u >= -eps && v >= -eps && (u + v) <= 1 + eps)
            {
                float y = a.Y + v0.Y * u + v1.Y * v;

                return Math.Abs(y - pt.Y);
            }

            return float.MaxValue;
        }
        /// <summary>
        /// Gets the minimum distance from p to the specified polygon
        /// </summary>
        /// <param name="p">Point to test</param>
        /// <param name="polygon">Polygon vertex list</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float DistancePtPoly2D(Vector3 p, Vector3[] polygon)
        {
            bool c = false;

            float dmin = float.MaxValue;
            int nvert = polygon.Length;

            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                var vi = polygon[i];
                var vj = polygon[j];

                if (TestPtEdges2D(p, vi, vj))
                {
                    c = !c;
                }

                dmin = Math.Min(dmin, DistancePtSegSqr2D(p, vj, vi));
            }

            return c ? -dmin : dmin;
        }

        /// <summary>
        /// Gets whether the specified polygons overlaps in the x-z plane
        /// </summary>
        /// <param name="polya">A polygon vertices</param>
        /// <param name="npolya">Number of vertices in the A polygon</param>
        /// <param name="polyb">B polygon vertices</param>
        /// <param name="npolyb">Number of vertices in the B polygon</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
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
        /// <summary>
        /// Gets whether the specified segments (a,b) and (c,d) overlaps in the x-z plane
        /// </summary>
        /// <param name="a">First segment point A</param>
        /// <param name="b">First segment point B</param>
        /// <param name="c">Second segment point C</param>
        /// <param name="d">Second segment point D</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool OverlapSegSeg2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = VCross2D(a, b, d);
            float a2 = VCross2D(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = VCross2D(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the segment (a,b) instersects the specified polygon
        /// </summary>
        /// <param name="a">Segment point A</param>
        /// <param name="b">Segment point B</param>
        /// <param name="polygon">Polygon vertices</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool IntersectSegmentPoly2D(Vector3 a, Vector3 b, Vector3[] polygon, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            const float eps = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            var dir = Vector3.Subtract(b, a);

            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var edge = Vector3.Subtract(polygon[i], polygon[j]);
                var diff = Vector3.Subtract(a, polygon[j]);
                float n = VPerp2D(edge, diff);
                float d = VPerp2D(dir, edge);
                if (Math.Abs(d) < eps)
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
        /// Gets whether the specified segments (a,b) and (c,d) intersects
        /// </summary>
        /// <param name="a">First segment point A</param>
        /// <param name="b">First segment point B</param>
        /// <param name="c">Second segment point C</param>
        /// <param name="d">Second segment point D</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool IntersectSegments2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out float s, out float t)
        {
            s = 0;
            t = 0;

            var u = Vector3.Subtract(b, a);
            var v = Vector3.Subtract(d, c);
            var w = Vector3.Subtract(a, c);

            float z = VPerp2D(u, v);
            if (Math.Abs(z) < ZeroTolerance)
            {
                return false;
            }

            s = VPerp2D(v, w) / z;
            t = VPerp2D(u, w) / z;

            return true;
        }

        /// <summary>
        /// Projects the specified polygon along the axis
        /// </summary>
        /// <param name="axis">Axis</param>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="npoly">Number of vertices in the polygon</param>
        /// <param name="rmin">Resulting minimum magnitude</param>
        /// <param name="rmax">Resulting maximum magnitude</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static void ProjectPoly2D(Vector3 axis, Vector3[] polygon, int npoly, out float rmin, out float rmax)
        {
            rmin = rmax = Vector2.Dot(axis.XZ(), polygon[0].XZ());
            for (int i = 1; i < npoly; ++i)
            {
                float d = Vector2.Dot(axis.XZ(), polygon[i].XZ());
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }
        /// <summary>
        /// Gets the area between the specified points in the XZ plane
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="p3">Point 3</param>
        /// <returns>Returns the value of the area between the specified points in the XZ plane</returns>
        public static float VCross2D(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;
            return u1 * v2 - v1 * u2;
        }
        /// <summary>
        /// Gets the Z magnitude of the cross product between the specified vectors, in the XZ plane
        /// </summary>
        /// <param name="u">U vector</param>
        /// <param name="v">V vector</param>
        /// <returns>Returns the Z magnitude of the cross product between the specified vectors, in the XZ plane</returns>
        private static float VPerp2D(Vector3 u, Vector3 v)
        {
            return u.Z * v.X - u.X * v.Z;
        }
        /// <summary>
        /// Calculates the minimum extent of the polygon
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static float PolyMinExtent2D(Vector3[] polygon)
        {
            float minDist = float.MaxValue;

            int nverts = polygon.Length;

            for (int i = 0; i < nverts; i++)
            {
                int ni = (i + 1) % nverts;

                var p1 = polygon[i];
                var p2 = polygon[ni];

                float maxEdgeDist = 0;
                for (int j = 0; j < nverts; j++)
                {
                    if (j == i || j == ni)
                    {
                        continue;
                    }

                    float d = DistancePtSegSqr2D(polygon[j], p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }

                minDist = Math.Min(minDist, maxEdgeDist);
            }

            return (float)Math.Sqrt(minDist);
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

            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(p, a);

            // Compute scaled barycentric coordinates
            float denom = v0.X * v1.Z - v0.Z * v1.X;
            if (Math.Abs(denom) < ZeroTolerance)
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
        /// Gets the closest point on the closest edge
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="edged">Distance to edges array</param>
        /// <param name="edget">Distance from first edge point to closest point list</param>
        /// <returns>Returns the closest position</returns>
        public static Vector3 ClosestPointOutsidePoly(Vector3[] verts, float[] edged, float[] edget)
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
        /// </summary>
        /// <remarks>
        /// Adapted from Graphics Gems article.
        /// </remarks>
        /// <param name="polygon">Polygon point list</param>
        /// <returns>Returns a a random point</returns>
        public static Vector3 RandomPointInConvexPoly(Vector3[] polygon)
        {
            float s = Helper.RandomGenerator.NextFloat(0, 1);
            float t = Helper.RandomGenerator.NextFloat(0, 1);

            List<float> areas = [];

            // Calc triangle areas
            float areasum = 0.0f;
            for (int i = 2; i < polygon.Length; i++)
            {
                var area = TriArea2D(polygon[0], polygon[i - 1], polygon[i]);
                areasum += Math.Max(0.001f, area);
                areas.Add(area);
            }

            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = polygon.Length - 1;
            for (int i = 2; i < polygon.Length; i++)
            {
                float dacc = areas[i - 2];
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
            var pa = polygon[0];
            var pb = polygon[tri - 1];
            var pc = polygon[tri];

            return a * pa + b * pb + c * pc;
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
        /// <summary>
        /// Gets the longest axis
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="z">Z position</param>
        public static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
            }
            return axis;
        }
        /// <summary>
        /// Gets the linear interpolation of t between t0 and t1
        /// </summary>
        /// <param name="t">Value to interpolate</param>
        /// <param name="t0">Minimum value</param>
        /// <param name="t1">Maximum value</param>
        /// <returns>Returns a value between 0 and 1</returns>
        public static float Tween(float t, float t0, float t1)
        {
            return MathUtil.Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }
        /// <summary>
        /// Gets whether the point b is into the cylinder defined by the a vector, and the specified radius and height
        /// </summary>
        /// <param name="a">A vector</param>
        /// <param name="b">B vector</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <returns>Returns true if the point b is into the cylinder defined by the a vector, and the specified radius and height</returns>
        public static bool InRange(Vector3 a, Vector3 b, float radius, float height)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float dz = b.Z - a.Z;

            return (dx * dx + dz * dz) < (radius * radius) && Math.Abs(dy) < height;
        }

        /// <summary>
        /// Gets the minimum distance from the pt point to the (p,q) segment
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="p">P segment point</param>
        /// <param name="q">Q segment point</param>
        /// <returns>Returns the distance from pt to closest point</returns>
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
        /// <summary>
        /// Gets the minimum distance from the pt point to the specified mesh
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="verts">Vertices array</param>
        /// <param name="triPoints">Triangle indices array</param>
        /// <returns>Returns the distance from pt to closest point</returns>
        public static float DistanceTriMesh(Vector3 pt, Vector3[] verts, Int3[] triPoints)
        {
            float dmin = float.MaxValue;

            foreach (var tri in triPoints)
            {
                var va = verts[tri.X];
                var vb = verts[tri.Y];
                var vc = verts[tri.Z];

                float d = DistancePtTri2D(pt, va, vb, vc);
                if (d < dmin)
                {
                    dmin = d;
                }
            }

            if (dmin < float.MaxValue)
            {
                return dmin;
            }

            return -1;
        }

        /// <summary>
        /// Get minimum and maximum bounds of the specified vector list
        /// </summary>
        /// <param name="point">Point list</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Length</param>
        public static BoundingBox GetMinMaxBounds(Vector3[] point, int startIndex, int length)
        {
            var bmin = point[startIndex];
            var bmax = point[startIndex];
            for (int j = 1; j < length; j++)
            {
                bmin = Vector3.Min(bmin, point[startIndex + j]);
                bmax = Vector3.Max(bmax, point[startIndex + j]);
            }

            return new(bmin, bmax);
        }
        /// <summary>
        /// Gets the polygon bounds
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        public static BoundingBox GetPolygonBounds(Vector3[] polygon)
        {
            var bmin = polygon[0];
            var bmax = polygon[0];

            for (int i = 1; i < polygon.Length; ++i)
            {
                bmin = Vector3.Min(bmin, polygon[i]);
                bmax = Vector3.Max(bmax, polygon[i]);
            }

            return new(bmin, bmax);
        }
        /// <summary>
        /// Determines if two axis-aligned bounding boxes overlap.
        /// </summary>
        /// <param name="amin">Minimum bounds of box A</param>
        /// <param name="amax">Maximum bounds of box A</param>
        /// <param name="bmin">Minimum bounds of box B</param>
        /// <param name="bmax">Maximum bounds of box B</param>
        /// <returns>True if the two AABB's overlap</returns>
        public static bool OverlapBounds(Int3 amin, Int3 amax, Int3 bmin, Int3 bmax)
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
        /// <summary>
        /// Checks wether the extents overlaps
        /// </summary>
        /// <param name="amin">A minimum rectangle point</param>
        /// <param name="amax">A maximum rectangle point</param>
        /// <param name="bounds">Bounds</param>
        /// <returns>Returns true if rectangles overlap</returns>
        public static bool OverlapRect(Vector2 amin, Vector2 amax, RectangleF bounds)
        {
            return
                !(amin.X > bounds.BottomRight.X || amax.X < bounds.TopLeft.X) &&
                !(amin.Y > bounds.BottomRight.Y || amax.Y < bounds.TopLeft.Y);
        }
        /// <summary>
        /// Checks wether the extents overlaps
        /// </summary>
        /// <param name="amin">A minimum rectangle point</param>
        /// <param name="amax">A maximum rectangle point</param>
        /// <param name="bmin">B minimum rectangle point</param>
        /// <param name="bmax">B maximum rectangle point</param>
        /// <param name="eps">Epsilon</param>
        /// <returns>Returns true if rectangles overlap</returns>
        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return !((amin + eps) > bmax || (amax - eps) < bmin);
        }
        /// <summary>
        /// Checks wether the extents overlaps
        /// </summary>
        /// <param name="amin">A minimum rectangle point</param>
        /// <param name="amax">A maximum rectangle point</param>
        /// <param name="bmin">B minimum rectangle point</param>
        /// <param name="bmax">B maximum rectangle point</param>
        /// <returns>Returns true if rectangles overlap</returns>
        public static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return !(amin >= bmax || amax <= bmin);
        }

        /// <summary>
        /// Adds the origin to the vertex list
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="orig">Origin</param>
        /// <param name="cellHeight">Cell height offset</param>
        /// <returns>Returns the tranformed vertex list</returns>
        public static Vector3[] MoveToWorldSpace(Vector3[] verts, Vector3 orig, float cellHeight)
        {
            if (orig == Vector3.Zero)
            {
                return [.. verts];
            }

            var res = new List<Vector3>();

            for (int j = 0; j < verts.Length; ++j)
            {
                var p = verts[j] + orig;

                if (!MathUtil.IsZero(cellHeight))
                {
                    p.Y += cellHeight;// Is this offset necessary?
                }

                res.Add(p);
            }

            return [.. res];
        }
        /// <summary>
        /// Adds the origin to the vertex list
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="orig">Origin</param>
        /// <returns>Returns the tranformed vertex list</returns>
        public static Vector3[] MoveToWorldSpace(Vector3[] verts, Vector3 orig)
        {
            return MoveToWorldSpace(verts, orig, 0);
        }
    }
}
