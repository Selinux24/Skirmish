using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    static class DetourUtils
    {
        #region Constants

        /// <summary>
        /// The maximum number of vertices per navigation polygon.
        /// </summary>
        public const int DT_VERTS_PER_POLYGON = 6;
        /// <summary>
        /// A magic number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_MAGIC = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_VERSION = 7;
        /// <summary>
        /// A magic number used to detect the compatibility of navigation tile states.
        /// </summary>
        public const int DT_NAVMESH_STATE_MAGIC = 'D' << 24 | 'N' << 16 | 'M' << 8 | 'S';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile states.
        /// </summary>
        public const int DT_NAVMESH_STATE_VERSION = 1;
        /// <summary>
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;
        /// <summary>
        /// A value that indicates the entity does not references to anything.
        /// </summary>
        public const int DT_NULL_IDX = -1;
        /// <summary>
        /// A value that indicates the entity does not link to anything.
        /// </summary>
        public const int DT_NULL_LINK = int.MaxValue;
        /// <summary>
        /// A flag that indicates that an off-mesh connection can be traversed in both directions. (Is bidirectional.)
        /// </summary>
        public const int DT_OFFMESH_CON_BIDIR = 1;
        /// <summary>
        /// The maximum number of user defined area ids.
        /// </summary>
        public const int DT_MAX_AREAS = 64;
        /// <summary>
        /// Limit raycasting during any angle pahfinding
        /// The limit is given as a multiple of the character radius
        /// </summary>
        public const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;

        public const int DT_NODE_PARENT_BITS = 24;
        public const int DT_NODE_STATE_BITS = 2;
        /// <summary>
        /// Number of extra states per node. See dtNode::state
        /// </summary>
        public const int DT_MAX_STATES_PER_NODE = 1 << DT_NODE_STATE_BITS;

        public const int MESH_NULL_IDX = -1;
        /// <summary>
        /// Search heuristic scale.
        /// </summary>
        public const float H_SCALE = 0.999f;

        #endregion

        #region DETOURCOMMON

        /// <summary>
        /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
        /// </summary>
        /// <param name="a">Vertex A. [(x, y, z)]</param>
        /// <param name="b">Vertex B. [(x, y, z)]</param>
        /// <param name="c">Vertex C. [(x, y, z)]</param>
        /// <returns>The signed xz-plane area of the triangle</returns>
        public static float TriArea2D(Vector3 a, Vector3 b, Vector3 c)
        {
            float abx = b.X - a.X;
            float abz = b.Z - a.Z;
            float acx = c.X - a.X;
            float acz = c.Z - a.Z;
            return acx * abz - abx * acz;
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

        public static void ClosestPtPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out Vector3 closest)
        {
            // Check if P in vertex region outside A
            Vector3 ab = Vector3.Subtract(b, a);
            Vector3 ac = Vector3.Subtract(c, a);
            Vector3 ap = Vector3.Subtract(p, a);
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                // barycentric coordinates (1,0,0)
                closest = a;
                return;
            }

            // Check if P in vertex region outside B
            Vector3 bp = Vector3.Subtract(p, b);
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                // barycentric coordinates (0,1,0)
                closest = b;
                return;
            }

            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                // barycentric coordinates (1-v,v,0)
                float v = d1 / (d1 - d3);
                closest.X = a.X + v * ab.X;
                closest.Y = a.Y + v * ab.Y;
                closest.Z = a.Z + v * ab.Z;
                return;
            }

            // Check if P in vertex region outside C
            Vector3 cp = Vector3.Subtract(p, c);
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                // barycentric coordinates (0,0,1)
                closest = c;
                return;
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                // barycentric coordinates (1-w,0,w)
                float w = d2 / (d2 - d6);
                closest.X = a.X + w * ac.X;
                closest.Y = a.Y + w * ac.Y;
                closest.Z = a.Z + w * ac.Z;
                return;
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                // barycentric coordinates (0,1-w,w)
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                closest.X = b.X + w * (c.X - b.X);
                closest.Y = b.Y + w * (c.Y - b.Y);
                closest.Z = b.Z + w * (c.Z - b.Z);
                return;
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float closestV = vb * denom;
            float closestW = vc * denom;
            closest.X = a.X + ab.X * closestV + ac.X * closestW;
            closest.Y = a.Y + ab.Y * closestV + ac.Y * closestW;
            closest.Z = a.Z + ab.Z * closestV + ac.Z * closestW;
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

        public static bool IntersectSegmentPoly2D(Vector3 p0, Vector3 p1, IEnumerable<Vector3> polyVerts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            float EPS = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            Vector3 dir = Vector3.Subtract(p1, p0);
            Vector3[] verts = polyVerts.ToArray();

            for (int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
            {
                Vector3 edge = Vector3.Subtract(verts[i], verts[j]);
                Vector3 diff = Vector3.Subtract(p0, verts[j]);
                float n = Vperp2D(edge, diff);
                float d = Vperp2D(dir, edge);
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
                float t = n / d;
                if (d < 0)
                {
                    // segment S is entering across this edge
                    if (t > tmin)
                    {
                        tmin = t;
                        segMin = j;
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
                        segMax = j;
                        // S leaves before entering polygon
                        if (tmax < tmin)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool IntersectSegSeg2D(Vector3 ap, Vector3 aq, Vector3 bp, Vector3 bq, out float s, out float t)
        {
            s = 0;
            t = 0;

            Vector3 u = Vector3.Subtract(aq, ap);
            Vector3 v = Vector3.Subtract(bq, bp);
            Vector3 w = Vector3.Subtract(ap, bp);
            float d = VperpXZ(u, v);
            if (Math.Abs(d) < 1e-6f) return false;
            s = VperpXZ(v, w) / d;
            t = VperpXZ(u, w) / d;
            return true;
        }
        /// <summary>
        /// Gets if a point is into a polygon.
        /// </summary>
        /// <param name="pt">Point to test</param>
        /// <param name="vertices">Polygon vertex list</param>
        /// <returns>Returns true if the point is into the polygon</returns>
        /// <remarks>All points are projected onto the xz-plane, so the y-values are ignored.</remarks>
        public static bool PointInPolygon(Vector3 pt, IEnumerable<Vector3> vertices)
        {
            bool c = false;
            var verts = vertices.ToArray();
            for (int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi.Z > pt.Z) != (vj.Z > pt.Z)) &&
                    (pt.X < (vj.X - vi.X) * (pt.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
            }
            return c;
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
        public static bool DistancePtPolyEdgesSqr(Vector3 pt, IEnumerable<Vector3> verts, out float[] ed, out float[] et)
        {
            int nverts = verts.Count();

            ed = new float[nverts];
            et = new float[nverts];

            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts.ElementAt(i);
                var vj = verts.ElementAt(j);
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
                Vector3 va = polya[j];
                Vector3 vb = polya[i];
                Vector3 n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly(n, polya, npolya, out float amin, out float amax);
                ProjectPoly(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                Vector3 va = polyb[j];
                Vector3 vb = polyb[i];
                Vector3 n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly(n, polya, npolya, out float amin, out float amax);
                ProjectPoly(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            return true;
        }

        public static void ProjectPoly(Vector3 axis, Vector3[] poly, int npoly, out float rmin, out float rmax)
        {
            rmin = rmax = Vector2.Dot(axis.XZ(), poly[0].XZ());
            for (int i = 1; i < npoly; ++i)
            {
                float d = Vector2.Dot(axis.XZ(), poly[i].XZ());
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }

        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return !((amin + eps) > bmax || (amax - eps) < bmin);
        }

        public static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }
        /// <summary>
        /// Returns a random point in a convex polygon.
        /// Adapted from Graphics Gems article.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="areas"></param>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <param name="outPoint"></param>
        public static void RandomPointInConvexPoly(IEnumerable<Vector3> points, out float[] areas, float s, float t, out Vector3 outPoint)
        {
            areas = new float[DT_VERTS_PER_POLYGON];

            var pts = points.ToArray();

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

        public static float VperpXZ(Vector3 a, Vector3 b)
        {
            return a.X * b.Z - a.Z * b.X;
        }

        public static float Vperp2D(Vector3 u, Vector3 v)
        {
            return u.Z * v.X - u.X * v.Z;
        }

        public static bool Vequal(Vector3 p0, Vector3 p1)
        {
            float thr = (float)Math.Pow(1.0f / 16384.0f, 2);
            float d = Vector3.DistanceSquared(p0, p1);
            return d < thr;
        }

        #endregion

        #region DETOURNAVMESH

        public static bool OverlapSlabs(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax, float px, float py)
        {
            // Check for horizontal overlap.
            // The segment is shrunken a little so that slabs which touch
            // at end points are not connected.
            float minx = Math.Max(amin.X + px, bmin.X + px);
            float maxx = Math.Min(amax.X - px, bmax.X - px);
            if (minx > maxx)
            {
                return false;
            }

            // Check vertical overlap.
            float ad = (amax.Y - amin.Y) / (amax.X - amin.X);
            float ak = amin.Y - ad * amin.X;
            float bd = (bmax.Y - bmin.Y) / (bmax.X - bmin.X);
            float bk = bmin.Y - bd * bmin.X;
            float aminy = ad * minx + ak;
            float amaxy = ad * maxx + ak;
            float bminy = bd * minx + bk;
            float bmaxy = bd * maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;

            // Crossing segments always overlap.
            if (dmin * dmax < 0)
            {
                return true;
            }

            // Check for overlap at endpoints.
            float thr = (float)Math.Sqrt(py * 2);
            if (dmin * dmin <= thr || dmax * dmax <= thr)
            {
                return true;
            }

            return false;
        }
        public static float GetSlabCoord(Vector3 va, int side)
        {
            if (side == 0 || side == 4)
            {
                return va.X;
            }
            else if (side == 2 || side == 6)
            {
                return va.Z;
            }
            return 0;
        }
        public static void CalcSlabEndPoints(Vector3 va, Vector3 vb, out Vector2 bmin, out Vector2 bmax, int side)
        {
            bmin = new Vector2();
            bmax = new Vector2();

            if (side == 0 || side == 4)
            {
                if (va.Z < vb.Z)
                {
                    bmin.X = va.Z;
                    bmin.Y = va.Y;
                    bmax.X = vb.Z;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.Z;
                    bmin.Y = vb.Y;
                    bmax.X = va.Z;
                    bmax.Y = va.Y;
                }
            }
            else if (side == 2 || side == 6)
            {
                if (va.X < vb.X)
                {
                    bmin.X = va.X;
                    bmin.Y = va.Y;
                    bmax.X = vb.X;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.X;
                    bmin.Y = vb.Y;
                    bmax.X = va.X;
                    bmax.Y = va.Y;
                }
            }
        }
        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }

        #endregion
    }
}
