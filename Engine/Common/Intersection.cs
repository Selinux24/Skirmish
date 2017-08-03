using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Intersections
    /// </summary>
    public static class Intersection
    {
        /// <summary>
        /// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="instance">Instance</param>
        /// <param name="frustum">A <see cref="BoundingFrustum"/> for testing.</param>
        /// <returns>Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingFrustum"/>.</returns>
        public static ContainmentType FrustumContainsFrustum(this BoundingFrustum instance, BoundingFrustum frustum)
        {
            if (instance == frustum)
            {
                return ContainmentType.Contains;
            }

            var intersects = false;
            for (var i = 0; i < 6; ++i)
            {
                var plane = instance.GetPlane(i);
                PlaneIntersectionType planeIntersectionType;
                frustum.Intersects(ref plane, out planeIntersectionType);
                if (planeIntersectionType == PlaneIntersectionType.Back)
                {
                    return ContainmentType.Disjoint;
                }
                else if (planeIntersectionType == PlaneIntersectionType.Intersecting)
                {
                    intersects = true;
                    break;
                }
            }

            return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingBox.
        /// </summary>
        /// <param name="box1">The first box to test</param>
        /// <param name="box2">The second box to test</param>
        /// <returns>The type of containment the two objects have</returns>
        public static ContainmentType BoxContainsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            return Collision.BoxContainsBox(ref box1, ref box2);
        }
        /// <summary>
        /// Determines whether there is an intersection between a Ray and a BoundingBox
        /// </summary>
        /// <param name="ray">The ray to test</param>
        /// <param name="box">The box to test</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance)
        {
            return Collision.RayIntersectsBox(ref ray, ref box, out distance);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            float d;
            if (!Collision.RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out d))
            {
                distance = float.MaxValue;
                return false;
            }

            distance = d;
            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 point, out float distance)
        {
            float d;
            if (!Collision.RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out d))
            {
                point = Vector3.Zero;
                distance = float.MaxValue;
                return false;
            }

            point = ray.Position + (ray.Direction * d);
            distance = d;
            return true;
        }
        /// <summary>
		/// Determine whether a ray is intersecting a segment AB.
		/// </summary>
        /// <param name="ray">Ray</param>
		/// <param name="a">The endpoint A of segment AB.</param>
		/// <param name="b">The endpoint B of segment AB.</param>
		/// <param name="t">The parameter t</param>
		/// <returns>A value indicating whether the ray is intersecting with the segment.</returns>
		public static bool RayIntersectsSegment(ref Ray ray, ref Vector3 a, ref Vector3 b, out float t)
        {
            //default if not intersectng
            t = 0;

            Vector3 v = b - a;
            Vector3 w = ray.Position - a;

            float d = Helper.PerpendicularDotXZ(ref ray.Direction, ref v) * -1;
            if (Math.Abs(d) < 1e-6f)
            {
                return false;
            }

            d = 1.0f / d;
            t = Helper.PerpendicularDotXZ(ref v, ref w) * -d;
            if (t < 0 || t > 1)
            {
                return false;
            }

            float s = Helper.PerpendicularDotXZ(ref ray.Direction, ref w) * -d;
            if (s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Find the 3D distance between a point (x, y, z) and a segment PQ
        /// </summary>
        /// <param name="pt">The coordinate of the point.</param>
        /// <param name="p">The coordinate of point P in the segment PQ.</param>
        /// <param name="q">The coordinate of point Q in the segment PQ.</param>
        /// <returns>The distance between the point and the segment.</returns>
        public static float PointToSegmentSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
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
        public static float PointToSegment2DSquared(int x, int z, int px, int pz, int qx, int qz)
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
        public static float PointToSegment2DSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
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
        public static float PointToSegment2DSquared(ref Vector3 pt, ref Vector3 p, ref Vector3 q, out float t)
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
        public static float PointToPolygonSquared(Vector3 point, Vector3[] verts, int vertCount)
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
        public static float PointToPolygonEdgeSquared(Vector3 pt, Vector3[] verts, int nverts)
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
        public static bool PointToPolygonEdgeSquared(Vector3 pt, Vector3[] verts, int nverts, float[] edgeDist, float[] edgeT)
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
        public static float PointToTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
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
        public static bool PointToTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float height)
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
        public static bool PointInPoly(Vector3 pt, Vector3[] verts, int nverts)
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
        /// Determines whether two 2D segments AB and CD are intersecting.
        /// </summary>
        /// <param name="a">The endpoint A of segment AB.</param>
        /// <param name="b">The endpoint B of segment AB.</param>
        /// <param name="c">The endpoint C of segment CD.</param>
        /// <param name="d">The endpoint D of segment CD.</param>
        /// <returns>A value indicating whether the two segments are intersecting.</returns>
        public static bool SegmentSegment2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d)
        {
            float a1, a2, a3;

            Helper.Cross2D(ref a, ref b, ref d, out a1);
            Helper.Cross2D(ref a, ref b, ref c, out a2);

            if (a1 * a2 < 0.0f)
            {
                Helper.Cross2D(ref c, ref d, ref a, out a3);
                float a4 = a3 + a2 - a1;

                if (a3 * a4 < 0.0f)
                {
                    return true;
                }
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
        public static bool SegmentSegment2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, out float s, out float t)
        {
            Vector3 u = b - a;
            Vector3 v = d - c;
            Vector3 w = a - c;

            float magnitude = Helper.PerpendicularDotXZ(ref u, ref v);

            if (Math.Abs(magnitude) < 1e-6f)
            {
                s = float.NaN;
                t = float.NaN;

                return false;
            }

            s = Helper.PerpendicularDotXZ(ref v, ref w) / magnitude;
            t = Helper.PerpendicularDotXZ(ref u, ref w) / magnitude;

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
        public static bool SegmentPolygon2D(Vector3 p0, Vector3 p1, Vector3[] verts, int nverts, out float tmin, out float tmax, out int segMin, out int segMax)
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
        /// Determines whether two polygons A and B are intersecting
        /// </summary>
        /// <param name="polya">Polygon A's vertices</param>
        /// <param name="npolya">Number of vertices for polygon A</param>
        /// <param name="polyb">Polygon B's vertices</param>
        /// <param name="npolyb">Number of vertices for polygon B</param>
        /// <returns>True if intersecting, false if not</returns>
        public static bool PolygonIntersectsPolygon2D(Vector3[] polya, int npolya, Vector3[] polyb, int npolyb)
        {
            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                Vector3 va = polya[j];
                Vector3 vb = polya[i];
                Vector3 n = new Vector3(va.X - vb.X, 0.0f, va.Z - vb.Z);
                float amin, amax, bmin, bmax;
                Helper.ProjectPolygon(n, polya, npolya, out amin, out amax);
                Helper.ProjectPolygon(n, polyb, npolyb, out bmin, out bmax);
                if (!Helper.OverlapRange(amin, amax, bmin, bmax, float.Epsilon))
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
                Helper.ProjectPolygon(n, polya, npolya, out amin, out amax);
                Helper.ProjectPolygon(n, polyb, npolyb, out bmin, out bmax);
                if (!Helper.OverlapRange(amin, amax, bmin, bmax, float.Epsilon))
                {
                    //found separating axis
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs intersection test with ray and ray intersectable item list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="items">Ray intersectable item list</param>
        /// <param name="facingOnly">Select only items facing to ray origin</param>
        /// <param name="position">Result picked position</param>
        /// <param name="item">Result picked ray intersectable item</param>
        /// <param name="distance">Result distance to picked position</param>
        /// <returns>Returns first intersection if exists</returns>
        public static bool IntersectFirst<T>(ref Ray ray, T[] items, bool facingOnly, out Vector3 position, out T item, out float distance) where T : IRayIntersectable
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            for (int i = 0; i < items.Length; i++)
            {
                var cItem = items[i];

                Vector3 pos;
                float d;
                if (cItem.Intersects(ref ray, facingOnly, out pos, out d))
                {
                    position = pos;
                    item = cItem;
                    distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Performs intersection test with ray and ray intersectable item list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="items">Triangle list</param>
        /// <param name="facingOnly">Select only items facing to ray origin</param>
        /// <param name="position">Result picked position</param>
        /// <param name="item">Result picked ray intersectable item</param>
        /// <param name="distance">Result distance to picked position</param>
        /// <returns>Returns nearest intersection if exists</returns>
        public static bool IntersectNearest<T>(ref Ray ray, T[] items, bool facingOnly, out Vector3 position, out T item, out float distance) where T : IRayIntersectable
        {
            position = Vector3.Zero;
            item = default(T);
            distance = float.MaxValue;

            Vector3[] pickedPositions;
            T[] pickedTriangles;
            float[] pickedDistances;
            if (IntersectAll(ref ray, items, facingOnly, out pickedPositions, out pickedTriangles, out pickedDistances))
            {
                float distanceMin = float.MaxValue;

                for (int i = 0; i < pickedPositions.Length; i++)
                {
                    float dist = pickedDistances[i];
                    if (dist < distanceMin)
                    {
                        distanceMin = dist;
                        position = pickedPositions[i];
                        item = pickedTriangles[i];
                        distance = pickedDistances[i];
                    }
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// Performs intersection test with ray and ray intersectable item list
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="items">Triangle list</param>
        /// <param name="facingOnly">Select only items facing to ray origin</param>
        /// <param name="pickedPositions">Picked position list</param>
        /// <param name="pickedItems">Picked ray intersectable item list</param>
        /// <param name="pickedDistances">Distances to picked positions</param>
        /// <returns>Returns all intersections if exists</returns>
        public static bool IntersectAll<T>(ref Ray ray, T[] items, bool facingOnly, out Vector3[] pickedPositions, out T[] pickedItems, out float[] pickedDistances) where T : IRayIntersectable
        {
            SortedDictionary<float, Vector3> pickedPositionList = new SortedDictionary<float, Vector3>();
            SortedDictionary<float, T> pickedTriangleList = new SortedDictionary<float, T>();
            SortedDictionary<float, float> pickedDistancesList = new SortedDictionary<float, float>();

            foreach (T t in items)
            {
                Vector3 pos;
                float d;
                if (t.Intersects(ref ray, facingOnly, out pos, out d))
                {
                    //Avoid duplicate picked positions
                    if (!pickedPositionList.ContainsValue(pos))
                    {
                        float k = d;
                        while (pickedPositionList.ContainsKey(k))
                        {
                            //Avoid duplicate distance keys
                            k += 0.001f;
                        }

                        pickedPositionList.Add(k, pos);
                        pickedTriangleList.Add(k, t);
                        pickedDistancesList.Add(k, d);
                    }
                }
            }

            if (pickedPositionList.Values.Count > 0)
            {
                pickedPositions = new Vector3[pickedPositionList.Values.Count];
                pickedItems = new T[pickedTriangleList.Values.Count];
                pickedDistances = new float[pickedDistancesList.Values.Count];

                pickedPositionList.Values.CopyTo(pickedPositions, 0);
                pickedTriangleList.Values.CopyTo(pickedItems, 0);
                pickedDistancesList.Values.CopyTo(pickedDistances, 0);

                return true;
            }
            else
            {
                pickedPositions = null;
                pickedItems = null;
                pickedDistances = null;

                return false;
            }
        }
    }
}
