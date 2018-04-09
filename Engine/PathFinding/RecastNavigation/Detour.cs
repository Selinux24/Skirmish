using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    static class Detour
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
        public const int DT_NULL_IDX = int.MaxValue;
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

        public const int MESH_NULL_IDX = 0xffff;
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
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            overlap = (amin.Z > bmax.Z || amax.Z < bmin.Z) ? false : overlap;
            return overlap;
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
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            overlap = (amin.Z > bmax.Z || amax.Z < bmin.Z) ? false : overlap;
            return overlap;
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
            {
                float denom = 1.0f / (va + vb + vc);
                float v = vb * denom;
                float w = vc * denom;
                closest.X = a.X + ab.X * v + ac.X * w;
                closest.Y = a.Y + ab.Y * v + ac.Y * w;
                closest.Z = a.Z + ab.Z * v + ac.Z * w;
            }
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

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // The (sloppy) epsilon is needed to allow to get height of points which
            // are interpolated along the edges of the triangles.
            float EPS = 1e-4f;

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                h = a.Y + v0.Y * u + v1.Y * v;
                return true;
            }

            return false;
        }

        public static bool IntersectSegmentPoly2D(Vector3 p0, Vector3 p1, Vector3[] verts, int nverts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            float EPS = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            Vector3 dir = Vector3.Subtract(p1, p0);

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
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
        /// All points are projected onto the xz-plane, so the y-values are ignored.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="verts"></param>
        /// <param name="nverts"></param>
        /// <returns></returns>
        public static bool PointInPolygon(Vector3 pt, Vector3[] verts, int nverts)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
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

        public static bool DistancePtPolyEdgesSqr(Vector3 pt, Vector3[] verts, int nverts, out float[] ed, out float[] et)
        {
            ed = new float[nverts];
            et = new float[nverts];

            // TODO: Replace pnpoly with triArea2D tests?
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

        public static void CalcPolyCenter(int[] idx, int nidx, Vector3[] verts, out Vector3 tc)
        {
            tc = Vector3.Zero;

            for (int j = 0; j < nidx; ++j)
            {
                tc += verts[idx[j]];
            }

            tc *= 1.0f / nidx;
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
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
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
        /// <param name="npts"></param>
        /// <param name="areas"></param>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <param name="outPoint"></param>
        public static void RandomPointInConvexPoly(Vector3[] pts, int npts, out float[] areas, float s, float t, out Vector3 outPoint)
        {
            areas = new float[DT_VERTS_PER_POLYGON];

            // Calc triangle araes
            float areasum = 0.0f;
            for (int i = 2; i < npts; i++)
            {
                areas[i] = TriArea2D(pts[0], pts[(i - 1)], pts[i]);
                areasum += Math.Max(0.001f, areas[i]);
            }
            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = npts - 1;
            for (int i = 2; i < npts; i++)
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
            Vector3 pb = pts[(tri - 1)];
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

        #region DETOURNAVMESHBUILDER

        public static void CalcExtends(BVItem[] items, int nitems, int imin, int imax, ref Int3 bmin, ref Int3 bmax)
        {
            bmin.X = items[imin].bmin.X;
            bmin.Y = items[imin].bmin.Y;
            bmin.Z = items[imin].bmin.Z;

            bmax.X = items[imin].bmax.X;
            bmax.Y = items[imin].bmax.Y;
            bmax.Z = items[imin].bmax.Z;

            for (int i = imin + 1; i < imax; ++i)
            {
                BVItem it = items[i];
                if (it.bmin.X < bmin.X) bmin.X = it.bmin.X;
                if (it.bmin.Y < bmin.Y) bmin.Y = it.bmin.Y;
                if (it.bmin.Z < bmin.Z) bmin.Z = it.bmin.Z;

                if (it.bmax.X > bmax.X) bmax.X = it.bmax.X;
                if (it.bmax.Y > bmax.Y) bmax.Y = it.bmax.Y;
                if (it.bmax.Z > bmax.Z) bmax.Z = it.bmax.Z;
            }
        }
        private static int LongestAxis(int x, int y, int z)
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
        private static void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            BVNode node = new BVNode();
            nodes.Add(node);
            curNode++;

            if (inum == 1)
            {
                // Leaf
                node.bmin.X = items[imin].bmin.X;
                node.bmin.Y = items[imin].bmin.Y;
                node.bmin.Z = items[imin].bmin.Z;

                node.bmax.X = items[imin].bmax.X;
                node.bmax.Y = items[imin].bmax.Y;
                node.bmax.Z = items[imin].bmax.Z;

                node.i = items[imin].i;
            }
            else
            {
                // Split
                CalcExtends(items, nitems, imin, imax, ref node.bmin, ref node.bmax);

                int axis = LongestAxis(
                    node.bmax.X - node.bmin.X,
                    node.bmax.Y - node.bmin.Y,
                    node.bmax.Z - node.bmin.Z);

                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, BVItem.XComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, BVItem.YComparer);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, BVItem.ZComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items, nitems, imin, isplit, ref curNode, ref nodes);
                // Right
                Subdivide(items, nitems, isplit, imax, ref curNode, ref nodes);

                int iescape = curNode - icur;
                // Negative index means escape.
                node.i = -iescape;
            }
        }
        public static int CreateBVTree(NavMeshCreateParams param, ref List<BVNode> nodes)
        {
            // Build tree
            float quantFactor = 1 / param.cs;
            BVItem[] items = new BVItem[param.polyCount];
            for (int i = 0; i < param.polyCount; i++)
            {
                BVItem it = items[i];
                it.i = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.detailMeshes != null)
                {
                    int vb = param.detailMeshes[i][0];
                    int ndv = param.detailMeshes[i][1];
                    Vector3 bmin = param.detailVerts[vb];
                    Vector3 bmax = param.detailVerts[vb];

                    for (int j = 1; j < ndv; j++)
                    {
                        bmin = Vector3.Min(bmin, param.detailVerts[vb + j]);
                        bmax = Vector3.Max(bmax, param.detailVerts[vb + j]);
                    }

                    // BV-tree uses cs for all dimensions
                    it.bmin.X = MathUtil.Clamp((int)((bmin.X - param.bmin.X) * quantFactor), 0, 0xffff);
                    it.bmin.Y = MathUtil.Clamp((int)((bmin.Y - param.bmin.Y) * quantFactor), 0, 0xffff);
                    it.bmin.Z = MathUtil.Clamp((int)((bmin.Z - param.bmin.Z) * quantFactor), 0, 0xffff);

                    it.bmax.X = MathUtil.Clamp((int)((bmax.X - param.bmin.X) * quantFactor), 0, 0xffff);
                    it.bmax.Y = MathUtil.Clamp((int)((bmax.Y - param.bmin.Y) * quantFactor), 0, 0xffff);
                    it.bmax.Z = MathUtil.Clamp((int)((bmax.Z - param.bmin.Z) * quantFactor), 0, 0xffff);
                }
                else
                {
                    var p = param.polys[i];
                    it.bmin.X = it.bmax.X = param.verts[p[0]].X;
                    it.bmin.Y = it.bmax.Y = param.verts[p[0]].Y;
                    it.bmin.Z = it.bmax.Z = param.verts[p[0]].Z;

                    for (int j = 1; j < param.nvp; ++j)
                    {
                        if (p[j] == MESH_NULL_IDX) break;
                        var x = param.verts[p[j]].X;
                        var y = param.verts[p[j]].Y;
                        var z = param.verts[p[j]].Z;

                        if (x < it.bmin.X) it.bmin.X = x;
                        if (y < it.bmin.Y) it.bmin.Y = y;
                        if (z < it.bmin.Z) it.bmin.Z = z;

                        if (x > it.bmax.X) it.bmax.X = x;
                        if (y > it.bmax.Y) it.bmax.Y = y;
                        if (z > it.bmax.Z) it.bmax.Z = z;
                    }
                    // Remap y
                    it.bmin.Y = (int)Math.Floor(it.bmin.Y * param.ch / param.cs);
                    it.bmax.Y = (int)Math.Ceiling(it.bmax.Y * param.ch / param.cs);
                }
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.polyCount, 0, param.polyCount, ref curNode, ref nodes);

            items = null;

            return curNode;
        }
        public static int ClassifyOffMeshPoint(Vector3 pt, Vector3 bmin, Vector3 bmax)
        {
            int xp = 1 << 0;
            int zp = 1 << 1;
            int xm = 1 << 2;
            int zm = 1 << 3;

            int outcode = 0;
            outcode |= (pt.X >= bmax.X) ? xp : 0;
            outcode |= (pt.Z >= bmax.Z) ? zp : 0;
            outcode |= (pt.X < bmin.X) ? xm : 0;
            outcode |= (pt.Z < bmin.Z) ? zm : 0;

            if (outcode == xp) return 0;
            if (outcode == (xp | zp)) return 1;
            if (outcode == zp) return 2;
            if (outcode == (xm | zp)) return 3;
            if (outcode == xm) return 4;
            if (outcode == (xm | zm)) return 5;
            if (outcode == zm) return 6;
            if (outcode == (xp | zm)) return 7;

            return 0xff;
        }
        public static bool CreateNavMeshData(NavMeshCreateParams param, out MeshData outData)
        {
            outData = null;

            if (param.nvp > DT_VERTS_PER_POLYGON)
            {
                return false;
            }
            if (param.vertCount >= 0xffff)
            {
                return false;
            }
            if (param.vertCount == 0 || param.verts == null)
            {
                return false;
            }
            if (param.polyCount == 0 || param.polys == null)
            {
                return false;
            }

            int nvp = param.nvp;

            // Classify off-mesh connection points. We store only the connections
            // whose start point is inside the tile.
            int[] offMeshConClass = null;
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (param.offMeshConCount > 0)
            {
                offMeshConClass = new int[param.offMeshConCount * 2];

                // Find tight heigh bounds, used for culling out off-mesh start locations.
                float hmin = float.MaxValue;
                float hmax = float.MinValue;

                if (param.detailVerts != null && param.detailVertsCount > 0)
                {
                    for (int i = 0; i < param.detailVertsCount; ++i)
                    {
                        var h = param.detailVerts[i].Y;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                else
                {
                    for (int i = 0; i < param.vertCount; ++i)
                    {
                        var iv = param.verts[i];
                        float h = param.bmin.Y + iv.Y * param.ch;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                hmin -= param.walkableClimb;
                hmax += param.walkableClimb;
                Vector3 bmin = param.bmin;
                Vector3 bmax = param.bmax;
                bmin.Y = hmin;
                bmax.Y = hmax;

                for (int i = 0; i < param.offMeshConCount; ++i)
                {
                    var p0 = param.offMeshCon[i].Start;
                    var p1 = param.offMeshCon[i].End;
                    offMeshConClass[i + 0] = ClassifyOffMeshPoint(p0, bmin, bmax);
                    offMeshConClass[i + 1] = ClassifyOffMeshPoint(p1, bmin, bmax);

                    // Zero out off-mesh start positions which are not even potentially touching the mesh.
                    if (offMeshConClass[i * 2] == 0xff)
                    {
                        if (p0.Y < bmin.Y || p0.Y > bmax.Y)
                        {
                            offMeshConClass[i * 2] = 0;
                        }
                    }

                    // Cound how many links should be allocated for off-mesh connections.
                    if (offMeshConClass[i * 2] == 0xff)
                    {
                        offMeshConLinkCount++;
                    }
                    if (offMeshConClass[i * 2 + 1] == 0xff)
                    {
                        offMeshConLinkCount++;
                    }

                    if (offMeshConClass[i * 2] == 0xff)
                    {
                        storedOffMeshConCount++;
                    }
                }
            }

            // Off-mesh connectionss are stored as polygons, adjust values.
            int totPolyCount = param.polyCount + storedOffMeshConCount;
            int totVertCount = param.vertCount + storedOffMeshConCount * 2;

            // Find portal edges which are at tile borders.
            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < param.polyCount; ++i)
            {
                var p = param.polys[i];
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == MESH_NULL_IDX)
                    {
                        break;
                    }

                    edgeCount++;

                    if ((p[nvp + j] & 0x8000) != 0)
                    {
                        var dir = p[nvp + j] & 0xf;
                        if (dir != 0xf)
                        {
                            portalCount++;
                        }
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            // Find unique detail vertices.
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (param.detailMeshes != null)
            {
                // Has detail mesh, count unique detail vertex count and use input detail tri count.
                detailTriCount = param.detailTriCount;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    var p = param.polys[i];
                    var ndv = param.detailMeshes[i].Y;
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == MESH_NULL_IDX)
                        {
                            break;
                        }
                        nv++;
                    }
                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                // No input detail mesh, build detail mesh from nav polys.
                uniqueDetailVertCount = 0; // No extra detail verts.
                detailTriCount = 0;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    var p = param.polys[i];
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == MESH_NULL_IDX)
                        {
                            break;
                        }
                        nv++;
                    }
                    detailTriCount += nv - 2;
                }
            }

            MeshData data = new MeshData
            {
                // Store header
                header = new MeshHeader
                {
                    magic = DT_NAVMESH_MAGIC,
                    version = DT_NAVMESH_VERSION,
                    x = param.tileX,
                    y = param.tileY,
                    layer = param.tileLayer,
                    userId = param.userId,
                    polyCount = totPolyCount,
                    vertCount = totVertCount,
                    maxLinkCount = maxLinkCount,
                    bmin = param.bmin,
                    bmax = param.bmax,
                    detailMeshCount = param.polyCount,
                    detailVertCount = uniqueDetailVertCount,
                    detailTriCount = detailTriCount,
                    bvQuantFactor = 1.0f / param.cs,
                    offMeshBase = param.polyCount,
                    walkableHeight = param.walkableHeight,
                    walkableRadius = param.walkableRadius,
                    walkableClimb = param.walkableClimb,
                    offMeshConCount = storedOffMeshConCount,
                    bvNodeCount = param.buildBvTree ? param.polyCount * 2 : 0
                }
            };

            int offMeshVertsBase = param.vertCount;
            int offMeshPolyBase = param.polyCount;

            // Store vertices
            // Mesh vertices
            for (int i = 0; i < param.vertCount; ++i)
            {
                var iv = param.verts[i];
                var v = new Vector3
                {
                    X = param.bmin.X + iv.X * param.cs,
                    Y = param.bmin.Y + iv.Y * param.ch,
                    Z = param.bmin.Z + iv.Z * param.cs
                };
                data.navVerts.Add(v);
            }
            // Off-mesh link vertices.
            int n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2] == 0xff)
                {
                    var linkv = param.offMeshCon[i];
                    data.navVerts.Add(linkv.Start);
                    data.navVerts.Add(linkv.End);
                    n++;
                }
            }

            // Store polygons
            // Mesh polys
            int srcIndex = 0;
            for (int i = 0; i < param.polyCount; ++i)
            {
                var src = param.polys[srcIndex];

                Poly p = new Poly
                {
                    vertCount = 0,
                    flags = param.polyFlags[i],
                    Area = param.polyAreas[i],
                    Type = PolyTypes.DT_POLYTYPE_GROUND,
                };

                for (int j = 0; j < nvp; ++j)
                {
                    if (src[j] == MESH_NULL_IDX)
                    {
                        break;
                    }

                    p.verts[j] = src[j];

                    if ((src[nvp + j] & 0x8000) != 0)
                    {
                        // Border or portal edge.
                        var dir = src[nvp + j] & 0xf;
                        if (dir == 0xf) // Border
                        {
                            p.neis[j] = 0;
                        }
                        else if (dir == 0) // Portal x-
                        {
                            p.neis[j] = DT_EXT_LINK | 4;
                        }
                        else if (dir == 1) // Portal z+
                        {
                            p.neis[j] = DT_EXT_LINK | 2;
                        }
                        else if (dir == 2) // Portal x+
                        {
                            p.neis[j] = DT_EXT_LINK | 0;
                        }
                        else if (dir == 3) // Portal z-
                        {
                            p.neis[j] = DT_EXT_LINK | 6;
                        }
                    }
                    else
                    {
                        // Normal connection
                        p.neis[j] = src[nvp + j] + 1;
                    }

                    p.vertCount++;
                }

                data.navPolys.Add(p);

                srcIndex++;
            }

            // Off-mesh connection vertices.
            n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    Poly p = new Poly
                    {
                        flags = param.offMeshCon[i].Flags,
                        Area = param.offMeshCon[i].Area,
                        Type = PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION
                    };
                    p.verts[0] = (offMeshVertsBase + n * 2 + 0);
                    p.verts[1] = (offMeshVertsBase + n * 2 + 1);
                    p.vertCount = 2;

                    data.navPolys.Add(p);
                    n++;
                }
            }

            // Store detail meshes and vertices.
            // The nav polygon vertices are stored as the first vertices on each mesh.
            // We compress the mesh data by skipping them and using the navmesh coordinates.
            if (param.detailMeshes != null)
            {
                for (int i = 0; i < param.polyCount; ++i)
                {
                    int vb = param.detailMeshes[i][0];
                    int ndv = param.detailMeshes[i][1];
                    int nv = data.navPolys[i].vertCount;
                    PolyDetail dtl = new PolyDetail
                    {
                        vertBase = data.navDVerts.Count,
                        vertCount = (ndv - nv),
                        triBase = param.detailMeshes[i][2],
                        triCount = param.detailMeshes[i][3]
                    };
                    // Copy vertices except the first 'nv' verts which are equal to nav poly verts.
                    if (ndv - nv != 0)
                    {
                        var verts = param.detailVerts.Skip(vb + nv).Take(ndv - nv);
                        data.navDVerts.AddRange(verts);
                    }
                    data.navDMeshes.Add(dtl);
                }
                // Store triangles.
                data.navDTris.AddRange(param.detailTris);
            }
            else
            {
                // Create dummy detail mesh by triangulating polys.
                int tbase = 0;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    int nv = data.navPolys[i].vertCount;
                    PolyDetail dtl = new PolyDetail
                    {
                        vertBase = 0,
                        vertCount = 0,
                        triBase = tbase,
                        triCount = (nv - 2)
                    };
                    // Triangulate polygon (local indices).
                    for (int j = 2; j < nv; ++j)
                    {
                        var t = new Int4
                        {
                            X = 0,
                            Y = (j - 1),
                            Z = j,
                            // Bit for each edge that belongs to poly boundary.
                            W = (1 << 2)
                        };
                        if (j == 2) t.W |= (1 << 0);
                        if (j == nv - 1) t.W |= (1 << 4);
                        tbase++;

                        data.navDTris.Add(t);
                    }
                    data.navDMeshes.Add(dtl);
                }
            }

            // Store and create BVtree.
            if (param.buildBvTree)
            {
                CreateBVTree(param, ref data.navBvtree);
            }

            // Store Off-Mesh connections.
            n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2] == 0xff)
                {
                    // Copy connection end-points.
                    var endPts1 = param.offMeshCon[i].Start;
                    var endPts2 = param.offMeshCon[i].End;

                    var con = new OffMeshConnection
                    {
                        poly = offMeshPolyBase + n,
                        rad = param.offMeshCon[i].Radius,
                        flags = param.offMeshCon[i].Direction != 0 ? DT_OFFMESH_CON_BIDIR : 0,
                        side = offMeshConClass[i * 2 + 1],
                        start = endPts1,
                        end = endPts2,
                        userId = param.offMeshCon[i].Id,
                    };

                    data.offMeshCons.Add(con);
                    n++;
                }
            }

            offMeshConClass = null;

            outData = data;

            return true;
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
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }
        public static int AllocLink(MeshTile tile)
        {
            if (tile.linksFreeList == DT_NULL_LINK)
            {
                return DT_NULL_LINK;
            }
            int link = tile.linksFreeList;
            tile.linksFreeList = tile.links[link].next;
            return link;
        }
        public static void FreeLink(MeshTile tile, int link)
        {
            tile.links[link].next = tile.linksFreeList;
            tile.linksFreeList = link;
        }

        #endregion

        #region DETOURNODE

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        /// <remarks>
        /// From Thomas Wang, https://gist.github.com/badboy/6267743
        /// </remarks>
        public static int HashRef(int a)
        {
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }

        #endregion
    }
}
