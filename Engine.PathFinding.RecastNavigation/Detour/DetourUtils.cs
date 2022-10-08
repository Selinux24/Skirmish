﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Recast;

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

        #region DETOURNAVMESHBUILDER

        public static void CalcExtends(BVItem[] items, int imin, int imax, out Int3 bmin, out Int3 bmax)
        {
            bmin.X = items[imin].BMin.X;
            bmin.Y = items[imin].BMin.Y;
            bmin.Z = items[imin].BMin.Z;

            bmax.X = items[imin].BMax.X;
            bmax.Y = items[imin].BMax.Y;
            bmax.Z = items[imin].BMax.Z;

            for (int i = imin + 1; i < imax; ++i)
            {
                BVItem it = items[i];
                if (it.BMin.X < bmin.X) bmin.X = it.BMin.X;
                if (it.BMin.Y < bmin.Y) bmin.Y = it.BMin.Y;
                if (it.BMin.Z < bmin.Z) bmin.Z = it.BMin.Z;

                if (it.BMax.X > bmax.X) bmax.X = it.BMax.X;
                if (it.BMax.Y > bmax.Y) bmax.Y = it.BMax.Y;
                if (it.BMax.Z > bmax.Z) bmax.Z = it.BMax.Z;
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
                node.BMin = items[imin].BMin;
                node.BMax = items[imin].BMax;
                node.I = items[imin].I;
            }
            else
            {
                // Split
                CalcExtends(items, imin, imax, out var bmin, out var bmax);
                node.BMin = bmin;
                node.BMax = bmax;

                int axis = LongestAxis(
                    node.BMax.X - node.BMin.X,
                    node.BMax.Y - node.BMin.Y,
                    node.BMax.Z - node.BMin.Z);

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
                node.I = -iescape;
            }
        }
        public static int CreateBVTree(NavMeshCreateParams param, out List<BVNode> nodes)
        {
            nodes = new List<BVNode>();

            // Build tree
            float quantFactor = 1 / param.CellSize;
            BVItem[] items = new BVItem[param.PolyCount];
            for (int i = 0; i < param.PolyCount; i++)
            {
                var it = items[i];
                it.I = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.DetailMeshes != null)
                {
                    CalcDetailBounds(ref it, param.DetailMeshes[i], param.DetailVerts, param.BMin, quantFactor);
                }
                else
                {
                    CalcPolygonBounds(ref it, param.Polys[i], param.Nvp, param.Verts, param.CellSize, param.CellHeight);
                }
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.PolyCount, 0, param.PolyCount, ref curNode, ref nodes);

            return curNode;
        }
        private static void CalcDetailBounds(ref BVItem it, PolyMeshDetailIndices dm, Vector3[] detailVerts, Vector3 bMin, float quantFactor)
        {
            int vb = dm.VertBase;
            int ndv = dm.VertCount;
            GetMinMaxBounds(detailVerts, vb, ndv, out var bmin, out var bmax);

            // BV-tree uses cs for all dimensions
            it.BMin = new Int3
            {
                X = MathUtil.Clamp((int)((bmin.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmin.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmin.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };

            it.BMax = new Int3
            {
                X = MathUtil.Clamp((int)((bmax.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmax.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmax.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };
        }
        private static void GetMinMaxBounds(Vector3[] vectors, int vb, int ndv, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = vectors[vb];
            bmax = vectors[vb];
            for (int j = 1; j < ndv; j++)
            {
                bmin = Vector3.Min(bmin, vectors[vb + j]);
                bmax = Vector3.Max(bmax, vectors[vb + j]);
            }
        }
        private static void CalcPolygonBounds(ref BVItem it, IndexedPolygon p, int nvp, Int3[] verts, float ch, float cs)
        {
            var itBMin = verts[p[0]];
            var itBMax = verts[p[0]];

            for (int j = 1; j < nvp; ++j)
            {
                if (p[j] == MESH_NULL_IDX) break;
                var x = verts[p[j]].X;
                var y = verts[p[j]].Y;
                var z = verts[p[j]].Z;

                if (x < it.BMin.X) itBMin.X = x;
                if (y < it.BMin.Y) itBMin.Y = y;
                if (z < it.BMin.Z) itBMin.Z = z;

                if (x > it.BMax.X) itBMax.X = x;
                if (y > it.BMax.Y) itBMax.Y = y;
                if (z > it.BMax.Z) itBMax.Z = z;
            }
            // Remap y
            itBMin.Y = (int)Math.Floor(it.BMin.Y * ch / cs);
            itBMax.Y = (int)Math.Ceiling(it.BMax.Y * ch / cs);

            it.BMin = itBMin;
            it.BMax = itBMax;
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
        public static MeshData CreateNavMeshData(NavMeshCreateParams param)
        {
            if (param.Nvp > DT_VERTS_PER_POLYGON ||
                param.VertCount == 0 || param.Verts == null ||
                param.PolyCount == 0 || param.Polys == null)
            {
                return null;
            }

            // Classify off-mesh connection points. 
            // We store only the connections whose start point is inside the tile.
            ClassifyOffMeshConnections(param, out var offMeshConClass, out int storedOffMeshConCount, out int offMeshConLinkCount);

            // Off-mesh connectionss are stored as polygons, adjust values.
            int totPolyCount = param.PolyCount + storedOffMeshConCount;
            int totVertCount = param.VertCount + storedOffMeshConCount * 2;

            // Find portal edges which are at tile borders.
            FindPortalEdges(param, out int edgeCount, out int portalCount);

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            // Find unique detail vertices.
            FindUniqueDetailVertices(param, out int uniqueDetailVertCount, out int detailTriCount);

            MeshData data = new MeshData
            {
                // Store header
                Header = new MeshHeader
                {
                    Magic = DT_NAVMESH_MAGIC,
                    Version = DT_NAVMESH_VERSION,
                    X = param.TileX,
                    Y = param.TileY,
                    Layer = param.TileLayer,
                    UserId = param.UserId,
                    PolyCount = totPolyCount,
                    VertCount = totVertCount,
                    MaxLinkCount = maxLinkCount,
                    Bounds = new BoundingBox(param.BMin, param.BMax),
                    DetailMeshCount = param.PolyCount,
                    DetailVertCount = uniqueDetailVertCount,
                    DetailTriCount = detailTriCount,
                    BvQuantFactor = 1.0f / param.CellSize,
                    OffMeshBase = param.PolyCount,
                    WalkableHeight = param.WalkableHeight,
                    WalkableRadius = param.WalkableRadius,
                    WalkableClimb = param.WalkableClimb,
                    OffMeshConCount = storedOffMeshConCount,
                    BvNodeCount = param.BuildBvTree ? param.PolyCount * 2 : 0
                }
            };

            int offMeshVertsBase = param.VertCount;
            int offMeshPolyBase = param.PolyCount;

            // Store vertices

            // Mesh vertices
            data.StoreMeshVertices(param);

            // Off-mesh link vertices.
            data.StoreOffMeshLinksVertices(param, offMeshConClass);

            // Store polygons

            // Mesh polys
            data.StoreMeshPolygons(param);

            // Off-mesh connection vertices.
            data.StoreOffMeshConnectionVertices(param, offMeshConClass, offMeshVertsBase);

            // Store detail meshes and vertices.
            // The nav polygon vertices are stored as the first vertices on each mesh.
            // We compress the mesh data by skipping them and using the navmesh coordinates.
            data.StoreDetailMeshes(param);

            // Store and create BVtree.
            if (param.BuildBvTree)
            {
                CreateBVTree(param, out var nodes);

                data.NavBvtree.AddRange(nodes);
            }

            // Store Off-Mesh connections.
            data.StoreOffMeshConnections(param, offMeshConClass, offMeshPolyBase);

            return data;
        }
        private static void ClassifyOffMeshConnections(NavMeshCreateParams param, out Vector2Int[] offMeshConClass, out int storedOffMeshConCount, out int offMeshConLinkCount)
        {
            // Classify off-mesh connection points. We store only the connections
            // whose start point is inside the tile.
            offMeshConClass = null;
            storedOffMeshConCount = 0;
            offMeshConLinkCount = 0;

            if (param.OffMeshConCount <= 0)
            {
                return;
            }

            offMeshConClass = new Vector2Int[param.OffMeshConCount];

            // Find tight heigh bounds, used for culling out off-mesh start locations.
            var bbox = param.FindBounds();
            Vector3 bmin = bbox.Minimum;
            Vector3 bmax = bbox.Maximum;

            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                var p0 = param.OffMeshCon[i].Start;
                var p1 = param.OffMeshCon[i].End;
                int x = ClassifyOffMeshPoint(p0, bmin, bmax);
                int y = ClassifyOffMeshPoint(p1, bmin, bmax);

                // Zero out off-mesh start positions which are not even potentially touching the mesh and
                // count how many links should be allocated for off-mesh connections.
                if (x == 0xff)
                {
                    if (p0.Y < bmin.Y || p0.Y > bmax.Y)
                    {
                        x = 0;
                    }

                    offMeshConLinkCount++;
                    storedOffMeshConCount++;
                }

                if (y == 0xff)
                {
                    offMeshConLinkCount++;
                }

                offMeshConClass[i].X = x;
                offMeshConClass[i].Y = y;
            }
        }
        private static void FindPortalEdges(NavMeshCreateParams param, out int edgeCount, out int portalCount)
        {
            edgeCount = 0;
            portalCount = 0;

            int nvp = param.Nvp;

            for (int i = 0; i < param.PolyCount; ++i)
            {
                var p = param.Polys[i];

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

        }
        private static void FindUniqueDetailVertices(NavMeshCreateParams param, out int uniqueDetailVertCount, out int detailTriCount)
        {
            // Find unique detail vertices.
            uniqueDetailVertCount = 0;

            if (param.DetailMeshes != null)
            {
                // Has detail mesh, count unique detail vertex count and use input detail tri count.
                detailTriCount = param.DetailTriCount;
                for (int i = 0; i < param.PolyCount; ++i)
                {
                    var p = param.Polys[i];
                    var ndv = param.DetailMeshes[i].VertCount;
                    int nv = p.FindFirstFreeIndex(param.Nvp);
                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                // No input detail mesh, build detail mesh from nav polys.
                uniqueDetailVertCount = 0; // No extra detail verts.
                detailTriCount = 0;
                for (int i = 0; i < param.PolyCount; ++i)
                {
                    var p = param.Polys[i];
                    int nv = p.FindFirstFreeIndex(param.Nvp);
                    detailTriCount += nv - 2;
                }
            }
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
        public static int AllocLink(MeshTile tile)
        {
            if (tile.LinksFreeList == DT_NULL_LINK)
            {
                return DT_NULL_LINK;
            }
            int link = tile.LinksFreeList;
            tile.LinksFreeList = tile.Links[link].Next;
            return link;
        }
        public static void FreeLink(MeshTile tile, int link)
        {
            tile.Links[link].Next = tile.LinksFreeList;
            tile.LinksFreeList = link;
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
