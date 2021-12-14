using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Intersections
    /// </summary>
    public static class Intersection
    {
        /// <summary>
        /// Determines whether a sphere intersects with a sphere
        /// </summary>
        /// <param name="sphere1">Sphere one</param>
        /// <param name="sphere2">Sphere two</param>
        /// <returns>Returns true if the sphere one intersects the sphere two</returns>
        public static bool SphereIntersectsSphere(BoundingSphere sphere1, BoundingSphere sphere2)
        {
            return sphere1.Intersects(sphere2);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a box
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns true if the sphere intersects the box</returns>
        public static bool SphereIntersectsBox(BoundingSphere sphere, BoundingBox box)
        {
            return sphere.Intersects(box);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a frustum
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the sphere intersects the frustum</returns>
        public static bool SphereIntersectsFrustum(BoundingSphere sphere, BoundingFrustum frustum)
        {
            return frustum.Intersects(ref sphere);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(BoundingSphere sphere, Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;

            return sphere.Intersects(ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <param name="point">Returns the closest point in the triangle to the sphere center</param>
        /// <param name="distance">Returns the distance from the closest point to the sphere center</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(BoundingSphere sphere, Triangle triangle, out Vector3 point, out float distance)
        {
            ClosestPointInTriangle(sphere.Center, triangle, out point);

            distance = Vector3.Distance(sphere.Center, point);

            return distance <= sphere.Radius;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="triangle">Returns the closest triangle to the sphere center</param>
        /// <param name="point">Returns the closest point in the triangle mesh to the sphere center</param>
        /// <param name="distance">Returns the distance from the closest point to the sphere center</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(BoundingSphere sphere, IEnumerable<Triangle> mesh, out Triangle triangle, out Vector3 point, out float distance)
        {
            ClosestPointInMesh(sphere.Center, mesh, out triangle, out point);

            distance = Vector3.Distance(sphere.Center, point);

            return distance <= sphere.Radius;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="points">Returns the intersection point list</param>
        /// <param name="distances">Returns the distance list from the sphere center to the intersection points</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(BoundingSphere sphere, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles, out IEnumerable<Vector3> points, out IEnumerable<float> distances)
        {
            List<Triangle> tris = new List<Triangle>();
            List<Vector3> pts = new List<Vector3>();
            List<float> dst = new List<float>();

            foreach (var t in mesh)
            {
                ClosestPointInTriangle(sphere.Center, t.Point1, t.Point2, t.Point3, out Vector3 closestToTri);

                float dist = Vector3.DistanceSquared(sphere.Center, closestToTri);
                if (dist <= sphere.Radius)
                {
                    // Store intersection data
                    tris.Add(t);
                    pts.Add(closestToTri);
                    dst.Add(dist);
                }
            }

            triangles = tris.ToArray();
            points = pts.ToArray();
            distances = dst.ToArray();

            return tris.Count > 0;
        }

        /// <summary>
        /// Determines whether a box intersects with a sphere
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns true if the box intersects the sphere</returns>
        public static bool BoxIntersectsSphere(BoundingBox box, BoundingSphere sphere)
        {
            return box.Intersects(sphere);
        }
        /// <summary>
        /// Determines whether a box intersects with a box
        /// </summary>
        /// <param name="box1">Axis aligned box one</param>
        /// <param name="box2">Axis aligned box two</param>
        /// <returns>Returns true if the box one intersects the box two</returns>
        public static bool BoxIntersectsBox(BoundingBox box1, BoundingBox box2)
        {
            return box1.Intersects(box2);
        }
        /// <summary>
        /// Determines whether a box intersects with a frustum
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the box intersects the frustum</returns>
        public static bool BoxIntersectsFrustum(BoundingBox box, BoundingFrustum frustum)
        {
            return frustum.Intersects(ref box);
        }
        /// <summary>
        /// Determines whether a box intersects with a triangle
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the box intersects the triangle</returns>
        public static bool BoxIntersectsTriangle(BoundingBox box, Triangle triangle)
        {
            // Own implementation. Not found in SharpDX
            return BoxContainsTriangle(box, triangle) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a box intersects with a mesh
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <returns>Returns true if the box intersects the mesh</returns>
        public static bool BoxIntersectsMesh(BoundingBox box, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles)
        {
            triangles = mesh
                .Where(t => BoxIntersectsTriangle(box, t))
                .ToArray();

            return triangles.Any();
        }

        /// <summary>
        /// Determines whether a frustum intersects with a frustum
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum two</param>
        /// <returns>Returns true if the frustum one intersects the frustum two</returns>
        public static bool FrustumIntersectsFrustum(BoundingFrustum frustum1, BoundingFrustum frustum2)
        {
            // Own implementation. Has errors in SharpDX
            return FrustumContainsFrustum(frustum1, frustum2) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a mesh intersects with a mesh
        /// </summary>
        /// <param name="mesh1">Mesh one</param>
        /// <param name="mesh2">Mesh two</param>
        /// <param name="triangles">Returns the intersected triangles of the mesh two</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the mesh one intersects the mesh two</returns>
        public static bool MeshIntersectsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2, out IEnumerable<Triangle> triangles, out IEnumerable<Line3D> segments)
        {
            List<Triangle> tris = new List<Triangle>();
            List<Line3D> segs = new List<Line3D>();

            foreach (var t in mesh1)
            {
                if (TriangleIntersectsMesh(t, mesh2, out var mTris, out var mSegs))
                {
                    tris.AddRange(mTris);
                    segs.AddRange(mSegs);
                }
            }

            triangles = tris.ToArray();
            segments = segs.ToArray();

            return tris.Count > 0;
        }

        /// <summary>
        /// Determines whether there is an intersection between a Ray and a BoundingBox
        /// </summary>
        /// <param name="ray">The ray to test</param>
        /// <param name="box">The box to test</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool RayIntersectsBox(Ray ray, BoundingBox box, out float distance)
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
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 point, out float distance)
        {
            point = Vector3.Zero;
            distance = float.MaxValue;

            if (Collision.RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out float d))
            {
                point = ray.Position + (ray.Direction * d);
                distance = d;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(Triangle triangle1, Triangle triangle2)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2);
        }
        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <param name="coplanar">Returns true if the triangles are coplanar</param>
        /// <param name="segment">Returns the intersection segment (if the triangles are not coplanar)</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(Triangle triangle1, Triangle triangle2, out bool coplanar, out Line3D segment)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2, out coplanar, out segment);
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(Triangle triangle, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles)
        {
            triangles = mesh
                .Where(t => TriangleIntersectsTriangle(triangle, t))
                .ToArray();

            return triangles.Any();
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(Triangle triangle, IEnumerable<Triangle> mesh, out IEnumerable<Triangle> triangles, out IEnumerable<Line3D> segments)
        {
            List<Triangle> tris = new List<Triangle>();
            List<Line3D> segs = new List<Line3D>();

            foreach (var t in mesh)
            {
                if (TriangleIntersectsTriangle(triangle, t, out _, out Line3D segment))
                {
                    tris.Add(t);
                    segs.Add(segment);
                }
            }

            triangles = tris.ToArray();
            segments = segs.ToArray();

            return tris.Count > 0;
        }

        /// <summary>
        /// Determines whether the triangles ar coplanar
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <returns>Returns true if the triangles are coplanar</returns>
        public static bool TriangleCoplanar(Triangle triangle1, Triangle triangle2)
        {
            return IntersectionTriangle.Coplanar(triangle1, triangle2);
        }

        /// <summary>
        /// Finds whether the point is contantained within the triangle, and the distance between the point and the triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="t">A triangle</param>
        /// <param name="distance">The distance between the point and the triangle.</param>
        /// <returns>A value indicating whether the point is contained within the triangle.</returns>
        public static bool PointInTriangle(Vector3 p, Triangle t, out float distance)
        {
            return PointInTriangle(p, t.Point1, t.Point2, t.Point3, out distance);
        }
        /// <summary>
        /// Finds whether the point is contantained within the triangle, and the distance between the point and the triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="distance">The distance between the point and the triangle.</param>
        /// <returns>A value indicating whether the point is contained within the triangle.</returns>
        public static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float distance)
        {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = p - a;

            Vector2 v20 = new Vector2(v0.X, v0.Z);
            Vector2 v21 = new Vector2(v1.X, v1.Z);
            Vector2 v22 = new Vector2(v2.X, v2.Z);

            Vector2.Dot(ref v20, ref v20, out float dot00);
            Vector2.Dot(ref v20, ref v21, out float dot01);
            Vector2.Dot(ref v20, ref v22, out float dot02);
            Vector2.Dot(ref v21, ref v21, out float dot11);
            Vector2.Dot(ref v21, ref v22, out float dot12);

            //compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            const float EPS = 1E-4f;

            //if point lies inside triangle, return interpolated y-coordinate
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                var h = a.Y + v0.Y * u + v1.Y * v;
                distance = Math.Abs(h - p.Y);
                return true;
            }

            distance = float.MaxValue;
            return false;
        }
        /// <summary>
        /// Finds whether the point is contantained within the triangle mesh, and the distance between the point and the closest triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="distance">The distance between the point and the closest triangle.</param>
        /// <param name="distance"></param>
        /// <returns>A value indicating whether the point is contained within the triangle mesh.</returns>
        public static bool PointInMesh(Vector3 p, IEnumerable<Triangle> mesh, out float distance)
        {
            distance = float.MaxValue;

            foreach (var tri in mesh)
            {
                if (PointInTriangle(p, tri, out float d))
                {
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
        /// <param name="items">Ray intersectable item list</param>
        /// <param name="facingOnly">Select only items facing to ray origin</param>
        /// <param name="position">Result picked position</param>
        /// <param name="item">Result picked ray intersectable item</param>
        /// <param name="distance">Result distance to picked position</param>
        /// <returns>Returns first intersection if exists</returns>
        public static bool IntersectFirst<T>(Ray ray, IEnumerable<T> items, bool facingOnly, out Vector3 position, out T item, out float distance) where T : IRayIntersectable
        {
            position = Vector3.Zero;
            item = default;
            distance = float.MaxValue;

            foreach (var cItem in items)
            {
                if (cItem.Intersects(ray, facingOnly, out Vector3 pos, out float d))
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
        public static bool IntersectNearest<T>(Ray ray, IEnumerable<T> items, bool facingOnly, out Vector3 position, out T item, out float distance) where T : IRayIntersectable
        {
            position = Vector3.Zero;
            item = default;
            distance = float.MaxValue;

            if (IntersectAll(ray, items, facingOnly, out var pickedPositions, out var pickedTriangles, out var pickedDistances))
            {
                float distanceMin = float.MaxValue;

                for (int i = 0; i < pickedPositions.Count(); i++)
                {
                    float dist = pickedDistances.ElementAt(i);
                    if (dist < distanceMin)
                    {
                        distanceMin = dist;
                        position = pickedPositions.ElementAt(i);
                        item = pickedTriangles.ElementAt(i);
                        distance = pickedDistances.ElementAt(i);
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
        public static bool IntersectAll<T>(Ray ray, IEnumerable<T> items, bool facingOnly, out IEnumerable<Vector3> pickedPositions, out IEnumerable<T> pickedItems, out IEnumerable<float> pickedDistances) where T : IRayIntersectable
        {
            SortedDictionary<float, Vector3> pickedPositionList = new SortedDictionary<float, Vector3>();
            SortedDictionary<float, T> pickedTriangleList = new SortedDictionary<float, T>();
            SortedDictionary<float, float> pickedDistancesList = new SortedDictionary<float, float>();

            foreach (T t in items)
            {
                //Avoid duplicate picked positions
                var intersects = t.Intersects(ray, facingOnly, out Vector3 pos, out float d);
                if (intersects && !pickedPositionList.ContainsValue(pos))
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

            if (pickedPositionList.Values.Count > 0)
            {
                var tmpPickedPositions = new Vector3[pickedPositionList.Values.Count];
                var tmpPickedItems = new T[pickedTriangleList.Values.Count];
                var tmpPickedDistances = new float[pickedDistancesList.Values.Count];

                pickedPositionList.Values.CopyTo(tmpPickedPositions, 0);
                pickedTriangleList.Values.CopyTo(tmpPickedItems, 0);
                pickedDistancesList.Values.CopyTo(tmpPickedDistances, 0);

                pickedPositions = tmpPickedPositions;
                pickedItems = tmpPickedItems;
                pickedDistances = tmpPickedDistances;

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

        /// <summary>
        /// Gets the nearest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting point along the ray</returns>
        public static Vector3 NearestPointOnLine(Ray ray, Vector3 point)
        {
            Vector3 origin = ray.Position;
            Vector3 dir = Vector3.Normalize(ray.Direction);

            //Vector from origin to point
            var v = origin - point;

            //Project v over direction vector and get the distance magnitude over dir
            var d = Vector3.Dot(v, dir);

            //Move from origin towards the resulting distance
            return origin + (dir * d);
        }
        /// <summary>
        /// Gets the distance from the nearest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting minimum distance from point to ray</returns>
        public static float DistanceFromPointToLine(Ray ray, Vector3 point)
        {
            var linePoint = NearestPointOnLine(ray, point);

            return Vector3.Distance(linePoint, point);
        }

        /// <summary>
        /// Gets the closest triangle point from the specified point p
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="t">Triangle</param>
        /// <param name="closest">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInTriangle(Vector3 p, Triangle t, out Vector3 closest)
        {
            ClosestPointInTriangle(p, t.Point1, t.Point2, t.Point3, out closest);
        }
        /// <summary>
        /// Gets the closest triangle point from the specified point p
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="a">Triangle vertex A</param>
        /// <param name="b">Triangle vertex B</param>
        /// <param name="c">Triangle vertex C</param>
        /// <param name="closest">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out Vector3 closest)
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
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="contactPoint">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(Vector3 p, IEnumerable<Triangle> mesh, out Triangle closest, out Vector3 contactPoint)
        {
            closest = new Triangle();
            contactPoint = p;

            float distance = float.MaxValue;
            foreach (var t in mesh)
            {
                ClosestPointInTriangle(p, t.Point1, t.Point2, t.Point3, out Vector3 closestToTri);

                float sqrDist = Vector3.DistanceSquared(p, closestToTri);

                if (sqrDist < distance)
                {
                    distance = sqrDist;

                    closest = t;
                    contactPoint = closestToTri;
                }
            }
        }

        /// <summary>
        /// Gets whether the specified edge overlaps with the axis
        /// </summary>
        /// <param name="extents">Box extents</param>
        /// <param name="tri">Triangle</param>
        /// <param name="edge">Edge</param>
        /// <param name="axis">Axis</param>
        private static bool TriangleOverlapOnBoxAxis(Vector3 extents, Triangle tri, Vector3 edge, Axis axis)
        {
            if (axis == Axis.X)
            {
                // a.X ^ b.X = (1,0,0) ^ edge
                // axis = Vector3(0, -edge.Z, edge.Y)
                float dPoint1 = tri.Point1.Z * edge.Y - tri.Point1.Y * edge.Z;
                float dPoint2 = tri.Point2.Z * edge.Y - tri.Point2.Y * edge.Z;
                float dPoint3 = tri.Point3.Z * edge.Y - tri.Point3.Y * edge.Z;
                float dhalf = Math.Abs(extents.Y * edge.Z) + Math.Abs(extents.Z * edge.Y);
                if (Math.Min(dPoint1, Math.Min(dPoint2, dPoint3)) >= dhalf ||
                    Math.Max(dPoint1, Math.Max(dPoint2, dPoint3)) <= -dhalf)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (axis == Axis.Y)
            {
                // a.Y ^ b.X = (0,1,0) ^ edge
                // axis = Vector3(edge.Z, 0, -edge.X)
                float dPoint1 = tri.Point1.X * edge.Z - tri.Point1.Z * edge.X;
                float dPoint2 = tri.Point2.X * edge.Z - tri.Point2.Z * edge.X;
                float dPoint3 = tri.Point3.X * edge.Z - tri.Point3.Z * edge.X;
                float dhalf = Math.Abs(extents.X * edge.Z) + Math.Abs(extents.Z * edge.X);
                if (Math.Min(dPoint1, Math.Min(dPoint2, dPoint3)) >= dhalf ||
                    Math.Max(dPoint1, Math.Max(dPoint2, dPoint3)) <= -dhalf)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (axis == Axis.Z)
            {
                // a.Y ^ b.X = (0,0,1) ^ edge
                // axis = Vector3(-edge.Y, edge.X, 0)
                float dPoint1 = tri.Point1.Y * edge.X - tri.Point1.X * edge.Y;
                float dPoint2 = tri.Point2.Y * edge.X - tri.Point2.X * edge.Y;
                float dPoint3 = tri.Point3.Y * edge.X - tri.Point3.X * edge.Y;
                float dhalf = Math.Abs(extents.Y * edge.X) + Math.Abs(extents.X * edge.Y);
                if (Math.Min(dPoint1, Math.Min(dPoint2, dPoint3)) >= dhalf ||
                    Math.Max(dPoint1, Math.Max(dPoint2, dPoint3)) <= -dhalf)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingSphere.
        /// </summary>
        /// <param name="sphere1">The first sphere to test</param>
        /// <param name="sphere2">The second sphere to test</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsSphere(BoundingSphere sphere1, BoundingSphere sphere2)
        {
            return Collision.SphereContainsSphere(ref sphere1, ref sphere2);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingBox.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsBox(BoundingSphere sphere, BoundingBox box)
        {
            return Collision.SphereContainsBox(ref sphere, ref box);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingFrustum.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsFrustum(BoundingSphere sphere, BoundingFrustum frustum)
        {
            return frustum.Contains(ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsTriangle(BoundingSphere sphere, Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;

            return Collision.SphereContainsTriangle(ref sphere, ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle mesh.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsMesh(BoundingSphere sphere, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                ContainmentType c = SphereContainsTriangle(sphere, t);

                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingSphere.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsSphere(BoundingBox box, BoundingSphere sphere)
        {
            return Collision.BoxContainsSphere(ref box, ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingBox.
        /// </summary>
        /// <param name="box1">The first box to test</param>
        /// <param name="box2">The second box to test</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsBox(BoundingBox box1, BoundingBox box2)
        {
            return Collision.BoxContainsBox(ref box1, ref box2);
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingFrustum.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsFrustum(BoundingBox box, BoundingFrustum frustum)
        {
            return frustum.Contains(ref box);
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsTriangle(BoundingBox box, Triangle triangle)
        {
            Vector3 boxExtents = box.GetExtents();

            BoundingBox triBounds = BoundingBox.FromPoints(triangle.GetVertices().ToArray());
            Vector3 triExtents = triBounds.GetExtents();
            Vector3 triCenter = triBounds.GetCenter();

            float triBoundCenterX = Math.Abs(triCenter.X);
            float triBoundCenterY = Math.Abs(triCenter.Y);
            float triBoundCenterZ = Math.Abs(triCenter.Z);

            if (triExtents.X + boxExtents.X <= triBoundCenterX ||
                triExtents.Y + boxExtents.Y <= triBoundCenterY ||
                triExtents.Z + boxExtents.Z <= triBoundCenterZ)
            {
                // The triangle is outside of the bounding box 
                return ContainmentType.Disjoint;
            }

            if (triExtents.X + triBoundCenterX <= boxExtents.X &&
                triExtents.Y + triBoundCenterY <= boxExtents.Y &&
                triExtents.Z + triBoundCenterZ <= boxExtents.Z)
            {
                // The triangle is inside the bounding box
                return ContainmentType.Contains;
            }

            // Test the triangle normal

            Vector3 edge1 = triangle.GetEdge1();
            Vector3 edge2 = triangle.GetEdge2();
            Vector3 crossEdge = Vector3.Cross(edge1, edge2);
            float triDist = Vector3.Dot(triangle.Point1, crossEdge);

            if (Math.Abs(crossEdge.X * boxExtents.X) +
                Math.Abs(crossEdge.Y * boxExtents.Y) +
                Math.Abs(crossEdge.Z * boxExtents.Z) <= Math.Abs(triDist))
            {
                return ContainmentType.Disjoint;
            }

            // Test the nine edge cross-products

            Vector3 edge3 = triangle.GetEdge3();

            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge1, Axis.X)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge2, Axis.X)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge3, Axis.X)) { return ContainmentType.Disjoint; }

            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge1, Axis.Y)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge2, Axis.Y)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge3, Axis.Y)) { return ContainmentType.Disjoint; }

            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge1, Axis.Z)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge2, Axis.Z)) { return ContainmentType.Disjoint; }
            if (TriangleOverlapOnBoxAxis(boxExtents, triangle, edge3, Axis.Z)) { return ContainmentType.Disjoint; }

            return ContainmentType.Intersects;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle mesh.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsMesh(BoundingBox box, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                ContainmentType c = BoxContainsTriangle(box, t);

                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingSphere.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsSphere(BoundingFrustum frustum, BoundingSphere sphere)
        {
            return frustum.Contains(ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingBox.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsBox(BoundingFrustum frustum, BoundingBox box)
        {
            return frustum.Contains(ref box);
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingFrustum.
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum trwo</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsFrustum(BoundingFrustum frustum1, BoundingFrustum frustum2)
        {
            if (frustum1 == frustum2)
            {
                return ContainmentType.Contains;
            }

            var intersects = false;
            for (var i = 0; i < 6; ++i)
            {
                var plane = frustum1.GetPlane(i);
                frustum2.Intersects(ref plane, out PlaneIntersectionType planeIntersectionType);
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
        /// Determines whether a BoundingFrustum contains a Triangle.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsTriangle(BoundingFrustum frustum, Triangle triangle)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a Triangle mesh.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsMesh(BoundingFrustum frustum, IEnumerable<Triangle> mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            foreach (var t in mesh)
            {
                ContainmentType c = FrustumContainsTriangle(frustum, t);

                if (c == res)
                {
                    continue;
                }

                if (res == ContainmentType.Disjoint)
                {
                    // From Disjoint to something
                    res = c;
                }
                else if (res == ContainmentType.Contains && c == ContainmentType.Intersects)
                {
                    // From contains to intersects
                    res = c;

                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Determines whether a Triangle mesh contains a Triangle mesh.
        /// </summary>
        /// <param name="mesh1">Triangle mesh one</param>
        /// <param name="mesh2">Triangle mesh two</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2)
        {
            foreach (var t in mesh1)
            {
                if (TriangleIntersectsMesh(t, mesh2, out _))
                {
                    return ContainmentType.Intersects;
                }
            }

            var bbox1 = BoundingBox.FromPoints(mesh1.SelectMany(t => t.GetVertices()).ToArray());
            var bbox2 = BoundingBox.FromPoints(mesh2.SelectMany(t => t.GetVertices()).ToArray());

            if (bbox1.Contains(ref bbox2) == ContainmentType.Disjoint)
            {
                return ContainmentType.Disjoint;
            }

            return ContainmentType.Contains;
        }
    }
}
