using System;
using SharpDX;

namespace Engine.Common
{
    public static class GeometryUtil
    {
        public static uint[] GenerateIndices(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            int nodes = trianglesPerNode / 2;
            uint side = (uint)Math.Sqrt(nodes);
            uint sideLoss = side / 2;

            bool topSide =
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideTop;

            bool bottomSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.SideBottom;

            bool leftSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.SideLeft;

            bool rightSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideRight;

            uint totalTriangles = (uint)trianglesPerNode;
            if (topSide) totalTriangles -= sideLoss;
            if (bottomSide) totalTriangles -= sideLoss;
            if (leftSide) totalTriangles -= sideLoss;
            if (rightSide) totalTriangles -= sideLoss;

            uint[] indices = new uint[totalTriangles * 3];

            int index = 0;

            for (uint y = 1; y < side; y += 2)
            {
                for (uint x = 1; x < side; x += 2)
                {
                    uint indexPRow = ((y - 1) * (side + 1)) + x;
                    uint indexCRow = ((y + 0) * (side + 1)) + x;
                    uint indexNRow = ((y + 1) * (side + 1)) + x;

                    //Top side
                    if (y == 1 && topSide)
                    {
                        //Top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow + 1;
                    }
                    else
                    {
                        //Top left
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow;
                        //Top right
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow;
                        indices[index++] = indexPRow + 1;
                    }

                    //Bottom side
                    if (y == side - 1 && bottomSide)
                    {
                        //Bottom only
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow - 1;
                    }
                    else
                    {
                        //Bottom left
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow;
                        indices[index++] = indexNRow - 1;
                        //Bottom right
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow;
                    }

                    //Left side
                    if (x == 1 && leftSide)
                    {
                        //Left only
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow - 1;
                        indices[index++] = indexPRow - 1;
                    }
                    else
                    {
                        //Left top
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow - 1;
                        indices[index++] = indexPRow - 1;
                        //Left bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow - 1;
                        indices[index++] = indexCRow - 1;
                    }

                    //Right side
                    if (x == side - 1 && rightSide)
                    {
                        //Right only
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                    else
                    {
                        //Right top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexCRow + 1;
                        //Right bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                }
            }

            return indices;
        }

        public static bool Intersects(Vector3 v11, Vector3 v12, Vector3 v21, Vector3 v22)
        {
            Vector2 p11 = new Vector2(v11.X, v11.Z);
            Vector2 p12 = new Vector2(v12.X, v12.Z);
            Vector2 p21 = new Vector2(v21.X, v21.Z);
            Vector2 p22 = new Vector2(v22.X, v22.Z);

            if ((p11.X == p21.X) && (p11.Y == p21.Y)) return false;
            if ((p11.X == p22.X) && (p11.Y == p22.Y)) return false;
            if ((p12.X == p21.X) && (p12.Y == p21.Y)) return false;
            if ((p12.X == p22.X) && (p12.Y == p22.Y)) return false;

            Vector2 v1ort = new Vector2(p12.Y - p11.Y, p11.X - p12.X);
            Vector2 v2ort = new Vector2(p22.Y - p21.Y, p21.X - p22.X);

            Vector2 v;
            v = p21 - p11;
            float dot21 = v.X * v1ort.X + v.Y * v1ort.Y;
            v = p22 - p11;
            float dot22 = v.X * v1ort.X + v.Y * v1ort.Y;

            if (dot21 * dot22 > 0) return false;

            v = p11 - p21;
            float dot11 = v.X * v2ort.X + v.Y * v2ort.Y;
            v = p12 - p21;
            float dot12 = v.X * v2ort.X + v.Y * v2ort.Y;

            if (dot11 * dot12 > 0) return false;

            return true;
        }
        public static bool IsInside(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p)
        {
            if (IsConvex(p1, p, p2)) return false;
            if (IsConvex(p2, p, p3)) return false;
            if (IsConvex(p3, p, p1)) return false;
            return true;
        }
        public static bool InCone(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p)
        {
            if (IsConvex(p1, p2, p3))
            {
                if (!IsConvex(p1, p2, p)) return false;
                if (!IsConvex(p2, p3, p)) return false;
                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p)) return true;
                if (IsConvex(p2, p3, p)) return true;
                return false;
            }
        }
        public static bool IsReflex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return OP(p1, p2, p3) < 0;
        }
        public static bool IsConvex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return OP(p1, p2, p3) > 0;
        }
        private static float OP(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return ((p3.Z - p1.Z) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Z - p1.Z));
        }

        /// <summary>
        /// Find the 3D distance between a point (x, y, z) and a segment PQ
        /// </summary>
        /// <param name="pt">The coordinate of the point.</param>
        /// <param name="p">The coordinate of point P in the segment PQ.</param>
        /// <param name="q">The coordinate of point Q in the segment PQ.</param>
        /// <returns>The distance between the point and the segment.</returns>
        internal static float PointToSegmentSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
        {
            //distance from P to Q
            Vector3 pq = q - p;

            //disance from P to the lone point
            float dx = pt.X - p.X;
            float dy = pt.Y - p.Y;
            float dz = pt.Z - p.Z;

            float segmentMagnitudeSquared = pq.LengthSquared();
            float t = pq.X * dx + pq.Y * dy + pq.Z * dz;

            if (segmentMagnitudeSquared > 0)
                t /= segmentMagnitudeSquared;

            //keep t between 0 and 1
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

            dx = p.X + t * pq.X - pt.X;
            dy = p.Y + t * pq.Y - pt.Y;
            dz = p.Z + t * pq.Z - pt.Z;

            return dx * dx + dy * dy + dz * dz;
        }
        /// <summary>
        /// Find the 2d distance between a point (x, z) and a segment PQ, where P is (px, pz) and Q is (qx, qz).
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="z">The Z coordinate of the point.</param>
        /// <param name="px">The X coordinate of point P in the segment PQ.</param>
        /// <param name="pz">The Z coordinate of point P in the segment PQ.</param>
        /// <param name="qx">The X coordinate of point Q in the segment PQ.</param>
        /// <param name="qz">The Z coordinate of point Q in the segment PQ.</param>
        /// <returns>The distance between the point and the segment.</returns>
        internal static float PointToSegment2DSquared(int x, int z, int px, int pz, int qx, int qz)
        {
            float segmentDeltaX = qx - px;
            float segmentDeltaZ = qz - pz;
            float dx = x - px;
            float dz = z - pz;
            float segmentMagnitudeSquared = segmentDeltaX * segmentDeltaX + segmentDeltaZ * segmentDeltaZ;
            float t = segmentDeltaX * dx + segmentDeltaZ * dz;

            //normalize?
            if (segmentMagnitudeSquared > 0)
                t /= segmentMagnitudeSquared;

            //0 < t < 1
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

            dx = px + t * segmentDeltaX - x;
            dz = pz + t * segmentDeltaZ - z;

            return dx * dx + dz * dz;
        }
        /// <summary>
        /// Find the 2d distance between a point and a segment PQ
        /// </summary>
        /// <param name="pt">The coordinate of the point.</param>
        /// <param name="p">The coordinate of point P in the segment PQ.</param>
        /// <param name="q">The coordinate of point Q in the segment PQ.</param>
        /// <returns>The distance between the point and the segment.</returns>
        internal static float PointToSegment2DSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
        {
            float t = 0;
            return PointToSegment2DSquared(ref pt, ref p, ref q, out t);
        }
        /// <summary>
        /// Find the 2d distance between a point and a segment PQ
        /// </summary>
        /// <param name="pt">The coordinate of the point.</param>
        /// <param name="p">The coordinate of point P in the segment PQ.</param>
        /// <param name="q">The coordinate of point Q in the segment PQ.</param>
        /// <param name="t">Parameterization ratio t</param>
        /// <returns>The distance between the point and the segment.</returns>
        internal static float PointToSegment2DSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q, out float t)
        {
            //distance from P to Q in the xz plane
            float segmentDeltaX = q.X - p.X;
            float segmentDeltaZ = q.Z - p.Z;

            //distance from P to lone point in xz plane
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;

            float segmentMagnitudeSquared = segmentDeltaX * segmentDeltaX + segmentDeltaZ * segmentDeltaZ;
            t = segmentDeltaX * dx + segmentDeltaZ * dz;

            if (segmentMagnitudeSquared > 0)
                t /= segmentMagnitudeSquared;

            //keep t between 0 and 1
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

            dx = p.X + t * segmentDeltaX - pt.X;
            dz = p.Z + t * segmentDeltaZ - pt.Z;

            return dx * dx + dz * dz;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="verts"></param>
        /// <param name="vertCount"></param>
        /// <returns></returns>
        internal static float PointToPolygonSquared(Vector3 point, Vector3[] verts, int vertCount)
        {
            float dmin = float.MaxValue;
            bool c = false;

            for (int i = 0, j = vertCount - 1; i < vertCount; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];

                if (((vi.Z > point.Z) != (vj.Z > point.Z)) && (point.X < (vj.X - vi.X) * (point.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                    c = !c;

                dmin = Math.Min(dmin, PointToSegment2DSquared(ref point, ref vj, ref vi));
            }

            return c ? -dmin : dmin;
        }
        /// <summary>
        /// Finds the squared distance between a point and the nearest edge of a polygon.
        /// </summary>
        /// <param name="pt">A point.</param>
        /// <param name="verts">A set of vertices that define a polygon.</param>
        /// <param name="nverts">The number of vertices to use from <c>verts</c>.</param>
        /// <returns>The squared distance between a point and the nearest edge of a polygon.</returns>
        internal static float PointToPolygonEdgeSquared(Vector3 pt, Vector3[] verts, int nverts)
        {
            float dmin = float.MaxValue;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                dmin = Math.Min(dmin, PointToSegment2DSquared(ref pt, ref verts[j], ref verts[i]));
            }

            return PointInPoly(pt, verts, nverts) ? -dmin : dmin;
        }
        /// <summary>
        /// Finds the distance between a point and the nearest edge of a polygon.
        /// </summary>
        /// <param name="pt">A point.</param>
        /// <param name="verts">A set of vertices that define a polygon.</param>
        /// <param name="nverts">The number of vertices to use from <c>verts</c>.</param>
        /// <param name="edgeDist">A buffer for edge distances to be stored in.</param>
        /// <param name="edgeT">A buffer for parametrization ratios to be stored in.</param>
        /// <returns>A value indicating whether the point is contained in the polygon.</returns>
        internal static bool PointToPolygonEdgeSquared(Vector3 pt, Vector3[] verts, int nverts, float[] edgeDist, float[] edgeT)
        {
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                edgeDist[j] = PointToSegment2DSquared(ref pt, ref verts[j], ref verts[i], out edgeT[j]);
            }

            return PointInPoly(pt, verts, nverts);
        }
        /// <summary>
        /// Finds the distance between a point and triangle ABC.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <returns>The distnace between the point and the triangle.</returns>
        internal static float PointToTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            //If the point lies inside the triangle, return the interpolated y-coordinate
            float h;
            if (PointToTriangle(p, a, b, c, out h))
            {
                return Math.Abs(h - p.Y);
            }

            return float.MaxValue;
        }
        /// <summary>
        /// Finds the distance between a point and triangle ABC.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="height">The height between the point and the triangle.</param>
        /// <returns>A value indicating whether the point is contained within the triangle.</returns>
        internal static bool PointToTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float height)
        {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = p - a;

            Vector2 v20 = new Vector2(v0.X, v0.Z);
            Vector2 v21 = new Vector2(v1.X, v1.Z);
            Vector2 v22 = new Vector2(v2.X, v2.Z);

            float dot00, dot01, dot02, dot11, dot12;

            Vector2.Dot(ref v20, ref v20, out dot00);
            Vector2.Dot(ref v20, ref v21, out dot01);
            Vector2.Dot(ref v20, ref v22, out dot02);
            Vector2.Dot(ref v21, ref v21, out dot11);
            Vector2.Dot(ref v21, ref v22, out dot12);

            //compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            const float EPS = 1E-4f;

            //if point lies inside triangle, return interpolated y-coordinate
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                height = a.Y + v0.Y * u + v1.Y * v;
                return true;
            }

            height = float.MaxValue;
            return false;
        }
        /// <summary>
        /// Determines whether a point is inside a polygon.
        /// </summary>
        /// <param name="pt">A point.</param>
        /// <param name="verts">A set of vertices that define a polygon.</param>
        /// <param name="nverts">The number of vertices to use from <c>verts</c>.</param>
        /// <returns>A value indicating whether the point is contained within the polygon.</returns>
        internal static bool PointInPoly(Vector3 pt, Vector3[] verts, int nverts)
        {
            bool c = false;

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
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
        /// Calculates the component-wise minimum of two vectors.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector.</param>
        /// <param name="result">The component-wise minimum of the two vectors.</param>
        internal static void ComponentMin(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
#if OPENTK || STANDALONE
			Vector3.ComponentMin(ref left, ref right, out result);
#elif UNITY3D
			result = Vector3.Min(left, right);
#else
            Vector3.Min(ref left, ref right, out result);
#endif
        }
        /// <summary>
        /// Calculates the component-wise maximum of two vectors.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector.</param>
        /// <param name="result">The component-wise maximum of the two vectors.</param>
        internal static void ComponentMax(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
#if OPENTK || STANDALONE
			Vector3.ComponentMax(ref left, ref right, out result);
#elif UNITY3D
			result = Vector3.Min(left, right);
#else
            Vector3.Max(ref left, ref right, out result);
#endif
        }
        /// <summary>
        /// Calculates the distance between two points on the XZ plane.
        /// </summary>
        /// <param name="a">A point.</param>
        /// <param name="b">Another point.</param>
        /// <returns>The distance between the two points.</returns>
        internal static float Distance2D(Vector3 a, Vector3 b)
        {
            float result;
            Distance2D(ref a, ref b, out result);
            return result;
        }
        /// <summary>
        /// Calculates the distance between two points on the XZ plane.
        /// </summary>
        /// <param name="a">A point.</param>
        /// <param name="b">Another point.</param>
        /// <param name="dist">The distance between the two points.</param>
        internal static void Distance2D(ref Vector3 a, ref Vector3 b, out float dist)
        {
            float dx = b.X - a.X;
            float dz = b.Z - a.Z;
            dist = (float)Math.Sqrt(dx * dx + dz * dz);
        }
        /// <summary>
        /// Calculates the dot product of two vectors projected onto the XZ plane.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector</param>
        /// <param name="result">The dot product of the two vectors.</param>
        internal static void Dot2D(ref Vector3 left, ref Vector3 right, out float result)
        {
            result = left.X * right.X + left.Z * right.Z;
        }
        /// <summary>
        /// Calculates the dot product of two vectors projected onto the XZ plane.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector</param>
        /// <returns>The dot product</returns>
        internal static float Dot2D(ref Vector3 left, ref Vector3 right)
        {
            return left.X * right.X + left.Z * right.Z;
        }
        /// <summary>
        /// Calculates the cross product of two vectors (formed from three points)
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="p3">The third point</param>
        /// <returns>The 2d cross product</returns>
        internal static float Cross2D(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float result;
            Cross2D(ref p1, ref p2, ref p3, out result);
            return result;
        }
        /// <summary>
        /// Calculates the cross product of two vectors (formed from three points)
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="p3">The third point</param>
        /// <param name="result">The 2d cross product</param>
        internal static void Cross2D(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, out float result)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;

            result = u1 * v2 - v1 * u2;
        }
        /// <summary>
        /// Calculates the perpendicular dot product of two vectors projected onto the XZ plane.
        /// </summary>
        /// <param name="a">A vector.</param>
        /// <param name="b">Another vector.</param>
        /// <param name="result">The perpendicular dot product on the XZ plane.</param>
        internal static void PerpDotXZ(ref Vector3 a, ref Vector3 b, out float result)
        {
            result = a.X * b.Z - a.Z * b.X;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="angle"></param>
        internal static void CalculateSlopeAngle(ref Vector3 vec, out float angle)
        {
            Vector3 up = Vector3.UnitY;
            float dot;
            Vector3.Dot(ref vec, ref up, out dot);
            angle = (float)Math.Acos(dot);
        }

        /// <summary>
        /// Determine whether a ray (origin, dir) is intersecting a segment AB.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="dir">The direction of the ray.</param>
        /// <param name="a">The endpoint A of segment AB.</param>
        /// <param name="b">The endpoint B of segment AB.</param>
        /// <param name="t">The parameter t</param>
        /// <returns>A value indicating whether the ray is intersecting with the segment.</returns>
        public static bool RaySegment(Vector3 origin, Vector3 dir, Vector3 a, Vector3 b, out float t)
        {
            //default if not intersectng
            t = 0;

            Vector3 v = b - a;
            Vector3 w = origin - a;

            float d;

            GeometryUtil.PerpDotXZ(ref dir, ref v, out d);
            d *= -1;
            if (Math.Abs(d) < 1e-6f)
                return false;

            d = 1.0f / d;
            GeometryUtil.PerpDotXZ(ref v, ref w, out t);
            t *= -d;
            if (t < 0 || t > 1)
                return false;

            float s;
            GeometryUtil.PerpDotXZ(ref dir, ref w, out s);
            s *= -d;
            if (s < 0 || s > 1)
                return false;

            return true;
        }
        /// <summary>
        /// Determines whether two 2D segments AB and CD are intersecting.
        /// </summary>
        /// <param name="a">The endpoint A of segment AB.</param>
        /// <param name="b">The endpoint B of segment AB.</param>
        /// <param name="c">The endpoint C of segment CD.</param>
        /// <param name="d">The endpoint D of segment CD.</param>
        /// <returns>A value indicating whether the two segments are intersecting.</returns>
        internal static bool SegmentSegment2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d)
        {
            float a1, a2, a3;

            GeometryUtil.Cross2D(ref a, ref b, ref d, out a1);
            GeometryUtil.Cross2D(ref a, ref b, ref c, out a2);

            if (a1 * a2 < 0.0f)
            {
                GeometryUtil.Cross2D(ref c, ref d, ref a, out a3);
                float a4 = a3 + a2 - a1;

                if (a3 * a4 < 0.0f)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Determines whether two 2D segments AB and CD are intersecting.
        /// </summary>
        /// <param name="a">The endpoint A of segment AB.</param>
        /// <param name="b">The endpoint B of segment AB.</param>
        /// <param name="c">The endpoint C of segment CD.</param>
        /// <param name="d">The endpoint D of segment CD.</param>
        /// <param name="s">The normalized dot product between CD and AC on the XZ plane.</param>
        /// <param name="t">The normalized dot product between AB and AC on the XZ plane.</param>
        /// <returns>A value indicating whether the two segments are intersecting.</returns>
        internal static bool SegmentSegment2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, out float s, out float t)
        {
            Vector3 u = b - a;
            Vector3 v = d - c;
            Vector3 w = a - c;

            float magnitude;
            GeometryUtil.PerpDotXZ(ref u, ref v, out magnitude);

            if (Math.Abs(magnitude) < 1e-6f)
            {
                //TODO is NaN the best value to set here?
                s = float.NaN;
                t = float.NaN;
                return false;
            }

            GeometryUtil.PerpDotXZ(ref v, ref w, out s);
            GeometryUtil.PerpDotXZ(ref u, ref w, out t);
            s /= magnitude;
            t /= magnitude;

            return true;
        }
        /// <summary>
        /// Determines whether two polygons A and B are intersecting
        /// </summary>
        /// <param name="polya">Polygon A's vertices</param>
        /// <param name="npolya">Number of vertices for polygon A</param>
        /// <param name="polyb">Polygon B's vertices</param>
        /// <param name="npolyb">Number of vertices for polygon B</param>
        /// <returns>True if intersecting, false if not</returns>
        internal static bool PolyPoly2D(Vector3[] polya, int npolya, Vector3[] polyb, int npolyb)
        {
            const float EPS = 1E-4f;

            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                Vector3 va = polya[j];
                Vector3 vb = polya[i];
                Vector3 n = new Vector3(va.X - vb.X, 0.0f, va.Z - vb.Z);
                float amin, amax, bmin, bmax;
                ProjectPoly(n, polya, npolya, out amin, out amax);
                ProjectPoly(n, polyb, npolyb, out bmin, out bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, EPS))
                {
                    //found separating axis
                    return false;
                }
            }

            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                Vector3 va = polyb[j];
                Vector3 vb = polyb[i];
                Vector3 n = new Vector3(va.X - vb.X, 0.0f, va.Z - vb.Z);
                float amin, amax, bmin, bmax;
                ProjectPoly(n, polya, npolya, out amin, out amax);
                ProjectPoly(n, polyb, npolyb, out bmin, out bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, EPS))
                {
                    //found separating axis
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Determines whether the segment interesects with the polygon.
        /// </summary>
        /// <param name="p0">Segment's first endpoint</param>
        /// <param name="p1">Segment's second endpoint</param>
        /// <param name="verts">Polygon's vertices</param>
        /// <param name="nverts">The number of vertices in the polygon</param>
        /// <param name="tmin">Parameter t minimum</param>
        /// <param name="tmax">Parameter t maximum</param>
        /// <param name="segMin">Minimum vertex index</param>
        /// <param name="segMax">Maximum vertex index</param>
        /// <returns>True if intersect, false if not</returns>
        internal static bool SegmentPoly2D(Vector3 p0, Vector3 p1, Vector3[] verts, int nverts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            const float Epsilon = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            Vector3 dir = p1 - p0;

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                Vector3 edge = verts[i] - verts[j];
                Vector3 diff = p0 - verts[j];
                float n = edge.Z * diff.X - edge.X * diff.Z;
                float d = dir.Z * edge.X - dir.X * edge.Z;
                if (Math.Abs(d) < Epsilon)
                {
                    //S is nearly parallel to this edge
                    if (n < 0)
                        return false;
                    else
                        continue;
                }

                float t = n / d;
                if (d < 0)
                {
                    //segment S is entering across this edge
                    if (t > tmin)
                    {
                        tmin = t;
                        segMin = j;

                        //S enters after leaving the polygon
                        if (tmin > tmax)
                            return false;
                    }
                }
                else
                {
                    //segment S is leaving across this edge
                    if (t < tmax)
                    {
                        tmax = t;
                        segMax = j;

                        //S leaves before entering the polygon
                        if (tmax < tmin)
                            return false;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="poly"></param>
        /// <param name="npoly"></param>
        /// <param name="rmin"></param>
        /// <param name="rmax"></param>
        internal static void ProjectPoly(Vector3 axis, Vector3[] poly, int npoly, out float rmin, out float rmax)
        {
            GeometryUtil.Dot2D(ref axis, ref poly[0], out rmin);
            GeometryUtil.Dot2D(ref axis, ref poly[0], out rmax);
            for (int i = 1; i < npoly; i++)
            {
                float d;
                GeometryUtil.Dot2D(ref axis, ref poly[i], out d);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="amin"></param>
        /// <param name="amax"></param>
        /// <param name="bmin"></param>
        /// <param name="bmax"></param>
        /// <param name="eps"></param>
        /// <returns></returns>
        internal static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }
    }

    public enum IndexBufferShapeEnum : int
    {
        None = -1,
        Full = 0,
        SideTop = 1,
        SideBottom = 2,
        SideLeft = 3,
        SideRight = 4,
        CornerTopLeft = 5,
        CornerBottomLeft = 6,
        CornerTopRight = 7,
        CornerBottomRight = 8,
    }
}
