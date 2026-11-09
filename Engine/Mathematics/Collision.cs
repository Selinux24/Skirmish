using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Mathematics
{
    /// <summary>
    /// Contains static methods to help in determining intersections, containment, etc.
    /// </summary>
    /// <remarks>
    /// This class is organized so that the least complex objects come first so that the least
    /// complex objects will have the most methods in most cases. Note that not all shapes exist
    /// at this time and not all shapes have a corresponding struct. Only the objects that have
    /// a corresponding struct should come first in naming and in parameter order. The order of
    /// complexity is as follows:
    /// 
    /// 1. Point
    /// 2. Ray
    /// 3. Segment
    /// 4. Plane
    /// 5. Triangle
    /// 6. Polygon
    /// 7. Box
    /// 8. Sphere
    /// 9. Ellipsoid
    /// 10. Cylinder
    /// 11. Cone
    /// 12. Capsule
    /// 13. Torus
    /// 14. Polyhedron
    /// 15. Frustum
    /// </remarks>
    public static class Collision
    {
        /// <summary>
        /// Determines the closest point between a point and a triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPointTriangle(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            Vector3 ab = vertex2 - vertex1;
            Vector3 ac = vertex3 - vertex1;
            Vector3 ap = point - vertex1;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result = vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            Vector3 bp = point - vertex2;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                result = vertex2; // Barycentric coordinates (0,1,0)
                return;
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
                return;
            }

            //Check if P in vertex region outside C
            Vector3 cp = point - vertex3;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                result = vertex3; //Barycentric coordinates (0,0,1)
                return;
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
                return;
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
                return;
            }

            //P inside face region. Compute Q through its Barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Determines the closest point between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPlanePoint(ref Plane plane, ref Vector3 point, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 126

            float dot = Vector3.Dot(plane.Normal, point);
            float t = dot - plane.D;

            result = point - (t * plane.Normal);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointBoxPoint(ref BoundingBox box, ref Vector3 point, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 130

            Vector3 temp = Vector3.Max(point, box.Minimum);
            result = Vector3.Min(temp, box.Maximum);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
        public static void ClosestPointSpherePoint(ref BoundingSphere sphere, ref Vector3 point, out Vector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the sphere's center to the point.
            result = Vector3.Subtract(point, sphere.Center);
            result.Normalize();

            //Multiply the unit direction by the sphere's radius to get a vector
            //the length of the sphere.
            result *= sphere.Radius;

            //Add the sphere's center to the direction to get a point on the sphere.
            result += sphere.Center;
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
        /// <remarks>
        /// If the two spheres are overlapping, but not directly on top of each other, the closest point
        /// is the 'closest' point of intersection. This can also be considered is the deepest point of
        /// intersection.
        /// </remarks>
        public static void ClosestPointSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2, out Vector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the first sphere's center to the second sphere's center.
            result = Vector3.Subtract(sphere2.Center, sphere1.Center);
            result.Normalize();

            //Multiply the unit direction by the first sphere's radius to get a vector
            //the length of the first sphere.
            result *= sphere1.Radius;

            //Add the first sphere's center to the direction to get a point on the first sphere.
            result += sphere1.Center;
        }

        /// <summary>
        /// Gets the closest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting point along the ray</returns>
        public static void ClosestPointInRay(ref Ray ray, ref Vector3 point, out Vector3 result)
        {
            var to = point - ray.Position;
            float rayLength = ray.Direction.Length();
            var dir = Vector3.Normalize(ray.Direction);

            var dist = Vector3.Dot(to, dir);
            if (dist < 0f) result = ray.Position;
            else if (dist > rayLength) result = ray.Position + ray.Direction;
            else result = ray.Position + (dir * dist);
        }
        /// <summary>
        /// Gets the closest point in a ray from a specified point
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Point</param>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the resulting minimum distance from point to ray</returns>
        public static void ClosestPointInRay(ref Ray ray, ref Vector3 point, out float distance, out Vector3 result)
        {
            ClosestPointInRay(ref ray, ref point, out result);
            distance = Vector3.Distance(result, point);
        }

        /// <summary>
        /// Gets the closest point in a segment from a specified point
        /// </summary>
        /// <param name="point1">First segment point</param>
        /// <param name="point2">Second segment point</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting minimum distance from point to segment</returns>
        public static void ClosestPointInSegment(ref Vector3 point1, ref Vector3 point2, ref Vector3 point, out Vector3 result)
        {
            ClosestPointInSegment(ref point1, ref point2, ref point, out _, out result);
        }
        /// <summary>
        /// Gets the closest point in a segment from a specified point
        /// </summary>
        /// <param name="point1">First segment point</param>
        /// <param name="point2">Second segment point</param>
        /// <param name="point">Point</param>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the resulting minimum distance from point to segment</returns>
        public static void ClosestPointInSegment(ref Vector3 point1, ref Vector3 point2, ref Vector3 point, out float distance, out Vector3 result)
        {
            var ab = point2 - point1;

            // Project point onto segment
            distance = Vector3.Dot(point - point1, ab);
            if (distance <= 0.0f)
            {
                // point projects outside the segment on the point1 side
                distance = -distance;
                result = point1;
                return;
            }

            float denom = Vector3.Dot(ab, ab);
            if (distance >= denom)
            {
                // point projects outside the segment on the point2 side
                result = point2;
                return;
            }

            // point projects inside the segment
            distance /= denom;
            result = point1 + distance * ab;
        }
        /// <summary>
        /// Gets the closest point in a segment from a specified point
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the resulting minimum distance from point to segment</returns>
        public static void ClosestPointInSegment(ref Segment segment, ref Vector3 point, out Vector3 result)
        {
            ClosestPointInSegment(ref segment, ref point, out _, out result);
        }
        /// <summary>
        /// Gets the closest point in a segment from a specified point
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="point">Point</param>
        /// <param name="distance">Distance</param>
        /// <returns>Returns the resulting minimum distance from point to segment</returns>
        public static void ClosestPointInSegment(ref Segment segment, ref Vector3 point, out float distance, out Vector3 result)
        {
            var p1 = segment.Point1;
            var p2 = segment.Point2;
            ClosestPointInSegment(ref p1, ref p2, ref point, out distance, out result);
        }

        /// <summary>
        /// Gets the point of the box closest to the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="box">Box</param>
        /// <returns>Returns the point of the box closest to the specified point</returns>
        public static void ClosestPointInBox(ref Vector3 point, ref BoundingBox box, out Vector3 result)
        {
            var center = box.Center;
            var extents = box.Extents;
            var transform = Matrix4x4.Identity;
            ClosestPointInBox(ref point, ref center, ref extents, ref transform, out result);
        }
        /// <summary>
        /// Gets the point of the box closest to the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="obb">Oriented bounding box</param>
        /// <returns>Returns the point of the box closest to the specified point</returns>
        public static void ClosestPointInBox(ref Vector3 point, ref OrientedBoundingBox obb, out Vector3 result)
        {
            var center = obb.Center;
            var extents = obb.Extents;
            var transform = obb.Transformation;
            ClosestPointInBox(ref point, ref center, ref extents, ref transform, out result);
        }
        /// <summary>
        /// Gets the point of the box closest to the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="center">Box center</param>
        /// <param name="extents">Box extents</param>
        /// <param name="transform">Box transform</param>
        /// <returns>Returns the point of the box closest to the specified point</returns>
        public static void ClosestPointInBox(ref Vector3 point, ref Vector3 center, ref Vector3 extents, ref Matrix4x4 transform, out Vector3 result)
        {
            Vector3 diff = point - center;

            float closestX = Vector3.Dot(diff, transform.Right);
            float closestY = Vector3.Dot(diff, transform.Up);
            float closestZ = Vector3.Dot(diff, transform.Backward);

            //X axis
            if (closestX < -extents.X)
            {
                closestX = -extents.X;
            }
            else if (closestX > extents.X)
            {
                closestX = extents.X;
            }

            //Y axis
            if (closestY < -extents.Y)
            {
                closestY = -extents.Y;
            }
            else if (closestY > extents.Y)
            {
                closestY = extents.Y;
            }

            //Z axis
            if (closestZ < -extents.Z)
            {
                closestZ = -extents.Z;
            }
            else if (closestZ > extents.Z)
            {
                closestZ = extents.Z;
            }

            result = center;
            result += closestX * transform.Right;
            result += closestY * transform.Up;
            result += closestZ * transform.Backward;
        }

        /// <summary>
        /// Gets the closest triangle point from the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the closest point in the triangle from point</returns>
        public static void ClosestPointInTriangle(ref Vector3 point, ref Triangle triangle, out Vector3 result)
        {
            Vector3 a = triangle.Point1;
            Vector3 b = triangle.Point2;
            Vector3 c = triangle.Point3;

            // Check if P in vertex region outside A
            Vector3 ab = Vector3.Subtract(b, a);
            Vector3 ac = Vector3.Subtract(c, a);
            Vector3 ap = Vector3.Subtract(point, a);
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                // barycentric coordinates (1,0,0)
                result = a;
                return;
            }

            // Check if P in vertex region outside B
            Vector3 bp = Vector3.Subtract(point, b);
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                // barycentric coordinates (0,1,0)
                result = b;
                return;
            }

            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                // barycentric coordinates (1-v,v,0)
                float v = d1 / (d1 - d3);
                result = new Vector3
                {
                    X = a.X + v * ab.X,
                    Y = a.Y + v * ab.Y,
                    Z = a.Z + v * ab.Z,
                };
                return;
            }

            // Check if P in vertex region outside C
            Vector3 cp = Vector3.Subtract(point, c);
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                // barycentric coordinates (0,0,1)
                result = c;
                return;
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                // barycentric coordinates (1-w,0,w)
                float w = d2 / (d2 - d6);
                result = new Vector3
                {
                    X = a.X + w * ac.X,
                    Y = a.Y + w * ac.Y,
                    Z = a.Z + w * ac.Z,
                };
                return;
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                // barycentric coordinates (0,1-w,w)
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = new Vector3
                {
                    X = b.X + w * (c.X - b.X),
                    Y = b.Y + w * (c.Y - b.Y),
                    Z = b.Z + w * (c.Z - b.Z),
                };
                return;
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float closestV = vb * denom;
            float closestW = vc * denom;
            result = new Vector3
            {
                X = a.X + ab.X * closestV + ac.X * closestW,
                Y = a.Y + ab.Y * closestV + ac.Y * closestW,
                Z = a.Z + ab.Z * closestV + ac.Z * closestW,
            };
        }
        /// <summary>
        /// Gets the closest triangle point from the specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the closest point in the triangle from point</returns>
        public static void ClosestPointInTriangle(ref Vector3 point, ref Triangle triangle, out float distance, out Vector3 result)
        {
            ClosestPointInTriangle(ref point, ref triangle, out result);

            Vector3.Distance(ref point, ref result, out distance);
        }

        /// <summary>
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="result">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(ref Vector3 point, Triangle[] mesh, out Triangle closest, out Vector3 result)
        {
            closest = new Triangle();
            result = point;

            float distance = float.MaxValue;
            for (int i = 0; i < mesh.Length; i++)
            {
                var t = mesh[i];

                ClosestPointInTriangle(ref point, ref t, out float dist, out var closestToTri);
                if (dist < distance)
                {
                    distance = dist;

                    closest = t;
                    result = closestToTri;
                }
            }
        }
        /// <summary>
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="result">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(ref Vector3 point, IEnumerable<Triangle> mesh, out Triangle closest, out Vector3 result)
        {
            ClosestPointInMesh(ref point, [.. mesh], out closest, out result);
        }
        /// <summary>
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="result">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(ref Vector3 point, Triangle[] mesh, out Triangle closest, out float distance, out Vector3 result)
        {
            ClosestPointInMesh(ref point, mesh, out closest, out result);

            Vector3.Distance(ref point, ref result, out distance);
        }
        /// <summary>
        /// Gets the closest point in a triangle mesh from the specified point p
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="closest">Returns the closest triangle from point p</param>
        /// <param name="result">Returns the closest point in the triangle from point p</param>
        public static void ClosestPointInMesh(ref Vector3 point, IEnumerable<Triangle> mesh, out Triangle closest, out float distance, out Vector3 result)
        {
            ClosestPointInMesh(ref point, [.. mesh], out closest, out distance, out result);
        }

        /// <summary>
        /// Determines the distance between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistancePlanePoint(ref Plane plane, ref Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 127

            float dot = Vector3.Dot(plane.Normal, point);
            return dot - plane.D;
        }
        /// <summary>
        /// Determines the distance between a <see cref="BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxPoint(ref BoundingBox box, ref Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 131

            float distance = 0f;

            if (point.X < box.Minimum.X)
                distance += (box.Minimum.X - point.X) * (box.Minimum.X - point.X);
            if (point.X > box.Maximum.X)
                distance += (point.X - box.Maximum.X) * (point.X - box.Maximum.X);

            if (point.Y < box.Minimum.Y)
                distance += (box.Minimum.Y - point.Y) * (box.Minimum.Y - point.Y);
            if (point.Y > box.Maximum.Y)
                distance += (point.Y - box.Maximum.Y) * (point.Y - box.Maximum.Y);

            if (point.Z < box.Minimum.Z)
                distance += (box.Minimum.Z - point.Z) * (box.Minimum.Z - point.Z);
            if (point.Z > box.Maximum.Z)
                distance += (point.Z - box.Maximum.Z) * (point.Z - box.Maximum.Z);

            return (float)Math.Sqrt(distance);
        }
        /// <summary>
        /// Determines the distance between a <see cref="BoundingBox"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            //Source:
            //Reference:

            float distance = 0f;

            //Distance for X.
            if (box1.Minimum.X > box2.Maximum.X)
            {
                float delta = box2.Maximum.X - box1.Minimum.X;
                distance += delta * delta;
            }
            else if (box2.Minimum.X > box1.Maximum.X)
            {
                float delta = box1.Maximum.X - box2.Minimum.X;
                distance += delta * delta;
            }

            //Distance for Y.
            if (box1.Minimum.Y > box2.Maximum.Y)
            {
                float delta = box2.Maximum.Y - box1.Minimum.Y;
                distance += delta * delta;
            }
            else if (box2.Minimum.Y > box1.Maximum.Y)
            {
                float delta = box1.Maximum.Y - box2.Minimum.Y;
                distance += delta * delta;
            }

            //Distance for Z.
            if (box1.Minimum.Z > box2.Maximum.Z)
            {
                float delta = box2.Maximum.Z - box1.Minimum.Z;
                distance += delta * delta;
            }
            else if (box2.Minimum.Z > box1.Maximum.Z)
            {
                float delta = box1.Maximum.Z - box2.Minimum.Z;
                distance += delta * delta;
            }

            return (float)Math.Sqrt(distance);
        }
        /// <summary>
        /// Determines the distance between a <see cref="BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSpherePoint(ref BoundingSphere sphere, ref Vector3 point)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3.Distance(sphere.Center, point);
            distance -= sphere.Radius;

            return Math.Max(distance, 0f);
        }
        /// <summary>
        /// Determines the distance between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3.Distance(sphere1.Center, sphere2.Center);
            distance -= sphere1.Radius + sphere2.Radius;

            return Math.Max(distance, 0f);
        }

        /// <summary>
        /// Gets whether two intersectables have intersection or not
        /// </summary>
        /// <param name="one">Object one</param>
        /// <param name="detectionModeOne">Detection mode for one</param>
        /// <param name="two">Object two</param>
        /// <param name="detectionModeTwo">Detection mode for two</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(IIntersectable one, IntersectDetectionMode detectionModeOne, IIntersectable two, IntersectDetectionMode detectionModeTwo)
        {
            var oneVolume = one?.GetIntersectionVolume(detectionModeOne);
            var twoVolume = two?.GetIntersectionVolume(detectionModeTwo);

            return Intersects(oneVolume, twoVolume);
        }
        /// <summary>
        /// Gets whether the intersectable and the volumen have intersection or not
        /// </summary>
        /// <param name="one">Object one</param>
        /// <param name="detectionModeOne">Detection mode for one</param>
        /// <param name="twoVolume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(IIntersectable one, IntersectDetectionMode detectionModeOne, ICullingVolume twoVolume)
        {
            var oneVolume = one?.GetIntersectionVolume(detectionModeOne);

            return Intersects(oneVolume, twoVolume);
        }
        /// <summary>
        /// Gets whether two volumes have intersection or not
        /// </summary>
        /// <param name="one">Volume one</param>
        /// <param name="two">Volume two</param>
        /// <returns>Returns true if have intersection</returns>
        public static bool Intersects(ICullingVolume one, ICullingVolume two)
        {
            if (one == null || two == null)
            {
                return false;
            }

            if (two is IntersectionVolumeSphere twoSph)
            {
                return one.Contains(twoSph) != ContainmentType.Disjoint;
            }

            if (two is IntersectionVolumeAxisAlignedBox twoBox)
            {
                return one.Contains(twoBox) != ContainmentType.Disjoint;
            }

            if (two is IntersectionVolumeFrustum twoFrustum)
            {
                return one.Contains(twoFrustum) != ContainmentType.Disjoint;
            }

            if (two is IntersectionVolumeMesh twoMesh)
            {
                return one.Contains((Triangle[])twoMesh) != ContainmentType.Disjoint;
            }

            return false;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a point.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPoint(ref Ray ray, ref Vector3 point)
        {
            //Source: RayIntersectsSphere
            //Reference: None

            Vector3 m = Vector3.Subtract(ray.Position, point);

            //Same thing as RayIntersectsSphere except that the radius of the sphere (point)
            //is the epsilon for zero.
            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - Helper.ZeroTolerance;

            if (c > 0f && b > 0f)
                return false;

            float discriminant = b * b - c;

            if (discriminant < 0f)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray1">The first ray to test.</param>
        /// <param name="ray2">The second ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        /// <remarks>
        /// This method performs a ray vs ray intersection test based on the following formula
        /// from Goldman.
        /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// Where o_1 is the position of the first ray, o_2 is the position of the second ray,
        /// d_1 is the normalized direction of the first ray, d_2 is the normalized direction
        /// of the second ray, det denotes the determinant of a matrix, x denotes the cross
        /// product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
        /// </remarks>
        public static bool RayIntersectsRay(ref Ray ray1, ref Ray ray2, out Vector3 point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780

            Vector3 cross = Vector3.Cross(ray1.Direction, ray2.Direction);
            float denominator = cross.Length();

            //Lines are parallel and on top of each other.
            if (Helper.IsZero(denominator) &&
                Helper.NearEqual(ray2.Position.X, ray1.Position.X) &&
                Helper.NearEqual(ray2.Position.Y, ray1.Position.Y) &&
                Helper.NearEqual(ray2.Position.Z, ray1.Position.Z))
            {
                point = Vector3.Zero;
                return true;
            }

            denominator *= denominator;

            //3x3 matrix for the first ray.
            float m11 = ray2.Position.X - ray1.Position.X;
            float m12 = ray2.Position.Y - ray1.Position.Y;
            float m13 = ray2.Position.Z - ray1.Position.Z;
            float m21 = ray2.Direction.X;
            float m22 = ray2.Direction.Y;
            float m23 = ray2.Direction.Z;
            float m31 = cross.X;
            float m32 = cross.Y;
            float m33 = cross.Z;

            //Determinant of first matrix.
            float dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //3x3 matrix for the second ray.
            m21 = ray1.Direction.X;
            m22 = ray1.Direction.Y;
            m23 = ray1.Direction.Z;

            //Determinant of the second matrix.
            float dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //t values of the point of intersection.
            float s = dets / denominator;
            float t = dett / denominator;

            //The points of intersection.
            Vector3 point1 = ray1.Position + (s * ray1.Direction);
            Vector3 point2 = ray2.Position + (t * ray2.Direction);

            //If the points are not equal, no intersection has occurred.
            if (!Helper.NearEqual(point2.X, point1.X) ||
                !Helper.NearEqual(point2.Y, point1.Y) ||
                !Helper.NearEqual(point2.Z, point1.Z))
            {
                point = Vector3.Zero;
                return false;
            }

            point = point1;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Segment"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="segment">Segment</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSegment(ref Ray ray, ref Segment segment, out Vector3 point)
        {
            return RayIntersectsSegment(ref ray, ref segment, out _, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Segment"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="segment">Segment</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSegment(ref Ray ray, ref Segment segment, out float distance, out Vector3 point)
        {
            Ray rRay1 = ray;
            Ray rRay2 = new(segment.Point1, segment.Direction);
            float length = segment.Length;

            bool touch = RayIntersectsRay(ref rRay1, ref rRay2, out Vector3 p);
            if (!touch)
            {
                point = Vector3.Zero;
                distance = float.MaxValue;

                return false;
            }

            float d = Vector3.Distance(ray.Position, p);
            if (d > length)
            {
                point = Vector3.Zero;
                distance = float.MaxValue;

                return false;
            }

            point = p;
            distance = d;

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Segment"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="segment">Segment</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSegment(ref PickingRay ray, ref Segment segment, out Vector3 point)
        {
            Ray r = ray;
            return RayIntersectsSegment(ref r, ref segment, out _, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Segment"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="segment">Segment</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSegment(ref PickingRay ray, ref Segment segment, out float distance, out Vector3 point)
        {
            Ray r = ray;
            return RayIntersectsSegment(ref r, ref segment, out distance, out point);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            float direction = Vector3.Dot(plane.Normal, ray.Direction);

            if (Helper.IsZero(direction))
            {
                distance = 0f;
                return false;
            }

            float position = Vector3.Dot(plane.Normal, ray.Position);
            distance = (-plane.D - position) / direction;

            if (distance < 0f)
            {
                distance = 0f;
                return false;
            }

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out float distance, out Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            if (RayIntersectsPlane(ref ray, ref plane, out distance))
            {
                point = ray.Position + (ray.Direction * distance);
                return true;
            }

            point = Vector3.Zero;
            return false;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the ray intersects either the front or back of the triangle.
        /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
        /// happened. If the intersection of the ray and the triangle is behind the origin of
        /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            //Source: Fast Minimum Storage Ray / Triangle Intersection
            //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            //Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            //Edge 1
            edge1.X = vertex2.X - vertex1.X;
            edge1.Y = vertex2.Y - vertex1.Y;
            edge1.Z = vertex2.Z - vertex1.Z;

            //Edge2
            edge2.X = vertex3.X - vertex1.X;
            edge2.Y = vertex3.Y - vertex1.Y;
            edge2.Z = vertex3.Z - vertex1.Z;

            //Cross product of ray direction and edge2 - first part of determinant.
            Vector3 directioncrossedge2;
            directioncrossedge2.X = (ray.Direction.Y * edge2.Z) - (ray.Direction.Z * edge2.Y);
            directioncrossedge2.Y = (ray.Direction.Z * edge2.X) - (ray.Direction.X * edge2.Z);
            directioncrossedge2.Z = (ray.Direction.X * edge2.Y) - (ray.Direction.Y * edge2.X);

            //Compute the determinant.
            float determinant;
            //Dot product of edge1 and the first part of determinant.
            determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            //If the ray is parallel to the triangle plane, there is no collision.
            //This also means that we are not culling, the ray may hit both the
            //back and the front of the triangle.
            if (Helper.IsZero(determinant))
            {
                distance = float.MaxValue;
                return false;
            }

            float inversedeterminant = 1.0f / determinant;

            //Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            distanceVector.X = ray.Position.X - vertex1.X;
            distanceVector.Y = ray.Position.Y - vertex1.Y;
            distanceVector.Z = ray.Position.Z - vertex1.Z;

            float triangleU;
            triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleU < 0f || triangleU > 1f)
            {
                distance = float.MaxValue;
                return false;
            }

            //Calculate the V parameter of the intersection point.
            Vector3 distancecrossedge1;
            distancecrossedge1.X = (distanceVector.Y * edge1.Z) - (distanceVector.Z * edge1.Y);
            distancecrossedge1.Y = (distanceVector.Z * edge1.X) - (distanceVector.X * edge1.Z);
            distancecrossedge1.Z = (distanceVector.X * edge1.Y) - (distanceVector.Y * edge1.X);

            float triangleV;
            triangleV = ((ray.Direction.X * distancecrossedge1.X) + (ray.Direction.Y * distancecrossedge1.Y)) + (ray.Direction.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleV < 0f || triangleU + triangleV > 1f)
            {
                distance = float.MaxValue;
                return false;
            }

            //Compute the distance along the ray to the triangle.
            float raydistance;
            raydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
            raydistance *= inversedeterminant;

            //Is the triangle behind the ray origin?
            if (raydistance < 0f)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = raydistance;
            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance, out Vector3 point)
        {
            if (!RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out float d))
            {
                point = Vector3.Zero;
                distance = float.MaxValue;
                return false;
            }

            point = ray.Position + ray.Direction * d;
            distance = Vector3.Distance(point, ray.Position);
            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the ray intersects either the front or back of the triangle.
        /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
        /// happened. If the intersection of the ray and the triangle is behind the origin of
        /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            return RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out distance, out _);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance, out Vector3 point)
        {
            point = Vector3.Zero;
            distance = float.MaxValue;

            Ray r = ray;
            if (RayIntersectsTriangle(ref r, ref vertex1, ref vertex2, ref vertex3, out float dist, out var p))
            {
                if (dist > ray.MaxDistance) return false;

                point = p;
                distance = dist;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Triangle tri, out Vector3 point)
        {
            return RayIntersectsTriangle(ref ray, ref tri, out _, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Triangle tri, out float distance, out Vector3 point)
        {
            var v1 = tri.Point1;
            var v2 = tri.Point2;
            var v3 = tri.Point3;
            return RayIntersectsTriangle(ref ray, ref v1, ref v2, ref v3, out distance, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Triangle tri, out Vector3 point)
        {
            return RayIntersectsTriangle(ref ray, ref tri, out _, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Triangle tri, out float distance, out Vector3 point)
        {
            point = Vector3.Zero;
            distance = float.MaxValue;

            if (ray.FacingOnly)
            {
                bool cull = Vector3.Dot(ray.Direction, tri.Normal) >= 0f;
                if (cull)
                {
                    point = Vector3.Zero;
                    distance = float.MaxValue;

                    return false;
                }
            }

            Ray r = ray;
            var v1 = tri.Point1;
            var v2 = tri.Point2;
            var v3 = tri.Point3;
            if (RayIntersectsTriangle(ref r, ref v1, ref v2, ref v3, out float dist, out var p))
            {
                if (dist > ray.MaxDistance) return false;

                point = p;
                distance = dist;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (Helper.IsZero(ray.Direction.X))
            {
                if (ray.Position.X < box.Minimum.X || ray.Position.X > box.Maximum.X)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (box.Minimum.X - ray.Position.X) * inverse;
                float t2 = (box.Maximum.X - ray.Position.X) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }

            if (Helper.IsZero(ray.Direction.Y))
            {
                if (ray.Position.Y < box.Minimum.Y || ray.Position.Y > box.Maximum.Y)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (box.Minimum.Y - ray.Position.Y) * inverse;
                float t2 = (box.Maximum.Y - ray.Position.Y) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }

            if (Helper.IsZero(ray.Direction.Z))
            {
                if (ray.Position.Z < box.Minimum.Z || ray.Position.Z > box.Maximum.Z)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (box.Minimum.Z - ray.Position.Z) * inverse;
                float t2 = (box.Maximum.Z - ray.Position.Z) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance, out Vector3 point)
        {
            if (!RayIntersectsBox(ref ray, ref box, out distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref PickingRay ray, ref BoundingBox box, out float distance)
        {
            Ray r = ray;
            return RayIntersectsBox(ref r, ref box, out distance, out _);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref PickingRay ray, ref BoundingBox box, out float distance, out Vector3 point)
        {
            Ray r = ray;
            return RayIntersectsBox(ref r, ref box, out distance, out point);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 177

            Vector3 m = Vector3.Subtract(ray.Position, sphere.Center);

            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - (sphere.Radius * sphere.Radius);

            if (c > 0f && b > 0f)
            {
                distance = 0f;
                return false;
            }

            float discriminant = b * b - c;

            if (discriminant < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = -b - MathF.Sqrt(discriminant);

            if (distance < 0f)
                distance = 0f;

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>. 
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out Vector3 point)
        {
            if (!RayIntersectsSphere(ref ray, ref sphere, out float distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The Ray to check for intersection with.</param>
        /// <param name="frustum">The BoundingFrustum to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public static bool RayIntersectsFrustum(ref Ray ray, ref BoundingFrustum frustum, out float? inDistance, out float? outDistance)
        {
            if (FrustumContainsPoint(ref frustum, ref ray.Position) != ContainmentType.Disjoint)
            {
                float nearstPlaneDistance = float.MaxValue;

                for (int i = 0; i < 6; i++)
                {
                    var plane = frustum.GetPlane(i);

                    if (RayIntersectsPlane(ref ray, ref plane, out float distance) && distance < nearstPlaneDistance)
                    {
                        nearstPlaneDistance = distance;
                    }
                }

                inDistance = nearstPlaneDistance;
                outDistance = null;
                return true;
            }
            else
            {
                //We will find the two points at which the ray enters and exists the frustum
                //These two points make a line which center inside the frustum if the ray intersects it
                //Or outside the frustum if the ray intersects frustum planes outside it.
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = frustum.GetPlane(i);

                    if (RayIntersectsPlane(ref ray, ref plane, out float distance))
                    {
                        minDist = Math.Min(minDist, distance);
                        maxDist = Math.Max(maxDist, distance);
                    }
                }

                Vector3 minPoint = ray.Position + ray.Direction * minDist;
                Vector3 maxPoint = ray.Position + ray.Direction * maxDist;
                Vector3 center = (minPoint + maxPoint) / 2f;
                if (FrustumContainsPoint(ref frustum, ref center) != ContainmentType.Disjoint)
                {
                    inDistance = minDist;
                    outDistance = maxDist;
                    return true;
                }
                else
                {
                    inDistance = null;
                    outDistance = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the ray intersects the specified circle in 3D
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="center">Circle center</param>
        /// <param name="normal">Circle normal</param>
        /// <param name="radius">Circle radius</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection</param>
        /// <param name="point">When the method completes, contains the point of intersection</param>
        public static bool RayIntersectsCircle3D(ref Ray ray, ref Vector3 center, ref Vector3 normal, float radius, out float distance, out Vector3 point)
        {
            distance = float.MaxValue;
            point = Vector3.Zero;

            var pFrom = new Plane(center, normal);
            if (!RayIntersectsPlane(ref ray, ref pFrom, out float dist, out Vector3 p))
            {
                return false;
            }

            if (dist > radius)
            {
                return false;
            }

            distance = dist;
            point = p;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Segment"/> and a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="segment">The segment to test</param>
        /// <param name="box">The box to test</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool SegmentIntersectsBox(ref Segment segment, ref BoundingBox box, out float distance)
        {
            var ray = new Ray(segment.Point1, segment.Direction);
            if (RayIntersectsBox(ref ray, ref box, out distance) && distance <= segment.Length)
            {
                return true;
            }

            distance = float.MaxValue;

            return false;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Segment"/> and a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="segment">The segment to test</param>
        /// <param name="box">The box to test</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected</returns>
        public static bool SegmentIntersectsBox(ref Segment segment, ref BoundingBox box, out float distance, out Vector3 point)
        {
            point = Vector3.Zero;
            distance = float.MaxValue;

            var ray = new Ray(segment.Point1, segment.Direction);
            if (!RayIntersectsBox(ref ray, ref box, out _, out var pos))
            {
                return false;
            }

            float d = Vector3.Distance(segment.Point1, pos);
            if (d > segment.Length)
            {
                return false;
            }

            point = pos;
            distance = d;

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Segment"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="segment">The segment to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SegmentIntersectsTriangle(ref Segment segment, ref Triangle tri, out float distance)
        {
            var ray = new PickingRay(segment);
            return RayIntersectsTriangle(ref ray, ref tri, out distance, out _);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Segment"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="segment">The segment to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SegmentIntersectsTriangle(ref Segment segment, ref Triangle tri, out float distance, out Vector3 point)
        {
            var ray = new PickingRay(segment);
            return RayIntersectsTriangle(ref ray, ref tri, out distance, out point);
        }

        /// <summary>
        /// Finds whether the point is contained within the triangle, and the distance between the point and the triangle.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="triangle">A triangle</param>
        /// <returns>A value indicating whether the point is contained within the triangle.</returns>
        public static bool TriangleIntersectsPoint(ref Triangle triangle, ref Vector3 point)
        {
            // Assume the point is in the center of coordinates

            // Subtract the triangle points to the test point
            var p1 = triangle.Point1 - point;
            var p2 = triangle.Point2 - point;
            var p3 = triangle.Point3 - point;

            // As the point is in the center, all the triangle normals must be facing to the same direction
            var u = Vector3.Cross(p2, p3);
            var v = Vector3.Cross(p3, p1);
            if (Vector3.Dot(u, v) < 0f)
            {
                return false;
            }

            var w = Vector3.Cross(p1, p2);
            if (Vector3.Dot(u, w) < 0f)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(ref Triangle triangle1, ref Triangle triangle2)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2);
        }
        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <param name="segment">Returns the intersection segment (if the triangles are not coplanar)</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(ref Triangle triangle1, ref Triangle triangle2, out Line3D? segment)
        {
            return IntersectionTriangle.Intersection(triangle1, triangle2, out segment);
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(ref Triangle triangle, Triangle[] mesh, out Triangle[] triangles)
        {
            List<Triangle> tris = [];

            for (int i = 0; i < mesh.Length; i++)
            {
                var t = mesh[i];
                if (TriangleIntersectsTriangle(ref triangle, ref t))
                {
                    tris.Add(t);
                }
            }

            triangles = [.. tris.Distinct()];

            return triangles.Length > 0;
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(ref Triangle triangle, IEnumerable<Triangle> mesh, out Triangle[] triangles)
        {
            return TriangleIntersectsMesh(ref triangle, [.. mesh], out triangles);
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(ref Triangle triangle, Triangle[] mesh, out Triangle[] triangles, out Line3D[] segments)
        {
            List<Triangle> tris = [];
            List<Line3D> segs = [];

            bool intersected = false;
            for (int i = 0; i < mesh.Length; i++)
            {
                var t = mesh[i];
                if (TriangleIntersectsTriangle(ref triangle, ref t, out var segment))
                {
                    tris.Add(t);
                    if (segment.HasValue) segs.Add(segment.Value);

                    intersected = true;
                }
            }

            triangles = [.. tris.Distinct()];
            segments = [.. segs.FindAll(seg => seg.Point1 != seg.Point2).Distinct()];

            return intersected;
        }
        /// <summary>
        /// Determines whether the triangle and the mesh intersects or not
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="triangles">Returns the intersected triangle list</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(ref Triangle triangle, IEnumerable<Triangle> mesh, out Triangle[] triangles, out Line3D[] segments)
        {
            return TriangleIntersectsMesh(ref triangle, [.. mesh], out triangles, out segments);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoint(ref Plane plane, ref Vector3 point)
        {
            float distance = Vector3.Dot(plane.Normal, point);
            distance += plane.D;

            if (distance > 0f)
                return PlaneIntersectionType.Front;

            if (distance < 0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a set of points.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="points">The points to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, Vector3[] points)
        {
            var result = PlaneIntersectsPoint(ref plane, ref points[0]);

            for (int i = 1; i < points.Length; i++)
            {
                if (PlaneIntersectsPoint(ref plane, ref points[i]) != result)
                {
                    return PlaneIntersectionType.Intersecting;
                }
            }

            return result;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a set of points.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="points">The points to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, IEnumerable<Vector3> points)
        {
            return PlaneIntersectsPoints(ref plane, [.. points]);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2)
        {
            Vector3 direction = Vector3.Cross(plane1.Normal, plane2.Normal);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3.Dot(direction, direction);

            if (Helper.IsZero(denominator))
                return false;

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="Ray"/>, or a zero ray if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// Although a ray is set to have an origin, the ray returned by this method is really
        /// a line in three dimensions which has no real origin. The ray is considered valid when
        /// both the positive direction is used and when the negative direction is used.
        /// </remarks>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2, out Ray line)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            Vector3 direction = Vector3.Cross(plane1.Normal, plane2.Normal);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3.Dot(direction, direction);

            //We assume the planes are normalized, therefore the denominator
            //only serves as a parallel and coincident check. Otherwise we need
            //to divide the point by the denominator.
            if (Helper.IsZero(denominator))
            {
                line = new Ray();
                return false;
            }

            Vector3 temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
            Vector3 point = Vector3.Cross(temp, direction);

            line.Position = point;
            line.Direction = direction;
            line.Direction.Normalize();

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a triangle.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsTriangle(ref Plane plane, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            PlaneIntersectionType test1 = PlaneIntersectsPoint(ref plane, ref vertex1);
            PlaneIntersectionType test2 = PlaneIntersectsPoint(ref plane, ref vertex2);
            PlaneIntersectionType test3 = PlaneIntersectsPoint(ref plane, ref vertex3);

            if (test1 == PlaneIntersectionType.Front && test2 == PlaneIntersectionType.Front && test3 == PlaneIntersectionType.Front)
                return PlaneIntersectionType.Front;

            if (test1 == PlaneIntersectionType.Back && test2 == PlaneIntersectionType.Back && test3 == PlaneIntersectionType.Back)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsBox(ref Plane plane, ref BoundingBox box)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 161

            Vector3 min;
            Vector3 max;

            max.X = (plane.Normal.X >= 0.0f) ? box.Minimum.X : box.Maximum.X;
            max.Y = (plane.Normal.Y >= 0.0f) ? box.Minimum.Y : box.Maximum.Y;
            max.Z = (plane.Normal.Z >= 0.0f) ? box.Minimum.Z : box.Maximum.Z;
            min.X = (plane.Normal.X >= 0.0f) ? box.Maximum.X : box.Minimum.X;
            min.Y = (plane.Normal.Y >= 0.0f) ? box.Maximum.Y : box.Minimum.Y;
            min.Z = (plane.Normal.Z >= 0.0f) ? box.Maximum.Z : box.Minimum.Z;

            float distance = Vector3.Dot(plane.Normal, max);

            if (distance + plane.D > 0.0f)
                return PlaneIntersectionType.Front;

            distance = Vector3.Dot(plane.Normal, min);

            if (distance + plane.D < 0.0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsSphere(ref Plane plane, ref BoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 160

            float distance = Vector3.Dot(plane.Normal, sphere.Center);
            distance += plane.D;

            if (distance > sphere.Radius)
                return PlaneIntersectionType.Front;

            if (distance < -sphere.Radius)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether a cylinder intersects with a plane
        /// </summary>
        /// <param name="cylinder">Axis aligned cylinder</param>
        /// <param name="plane">Plane</param>
        /// <returns>Returns true if the cylinder intersects the plane</returns>
        public static bool PlaneIntersectsCylinder(ref BoundingCylinder cylinder, ref Plane plane)
        {
            // Bounding cylinder is axis aligned. The direction of the cylinder is the up vector
            var cDir = Vector3.Up;

            // dir points towards the plane and -dir in the opposite direction
            var dir = Vector3.Cross(plane.Normal, cDir);
            if (Helper.IsZero(dir.Length()))
            {
                // Perfect base contact. Test distance from center
                var h = cylinder.Height * 0.5f;
                var c = cylinder.Center;
                float distance = Vector3.Dot(plane.Normal, c) + plane.D - h;

                return distance <= 0;
            }
            else
            {
                dir = Vector3.Normalize(Vector3.Cross(dir, cDir)) * cylinder.Radius;

                // Find the 4 points to test with the plane
                var h = cylinder.Height * 0.5f;
                var capPosition = cylinder.Center + (cDir * h);
                var basePosition = cylinder.Center - (cDir * h);

                var base1 = basePosition + dir;
                float distance = Vector3.Dot(plane.Normal, base1) + plane.D;
                if (distance <= Helper.ZeroTolerance)
                {
                    return true;
                }

                var cap1 = capPosition + dir;
                distance = Vector3.Dot(plane.Normal, cap1) + plane.D;
                if (distance <= Helper.ZeroTolerance)
                {
                    return true;
                }

                var base2 = basePosition - dir;
                distance = Vector3.Dot(plane.Normal, base2) + plane.D;
                if (distance <= Helper.ZeroTolerance)
                {
                    return true;
                }

                var cap2 = capPosition - dir;
                distance = Vector3.Dot(plane.Normal, cap2) + plane.D;
                if (distance <= Helper.ZeroTolerance)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Determines whether a capsule intersects with a plane
        /// </summary>
        /// <param name="capsule">Axis aligned capsule</param>
        /// <param name="plane">Plane</param>
        /// <returns>Returns true if the capsule intersects the plane</returns>
        public static bool PlaneIntersectsCapsule(ref BoundingCapsule capsule, ref Plane plane)
        {
            var p1 = capsule.BasePosition;
            float d = DistancePlanePoint(ref plane, ref p1);
            if (d <= capsule.Radius) return true;

            var p2 = capsule.BasePosition;
            d = DistancePlanePoint(ref plane, ref p2);
            if (d <= capsule.Radius) return true;

            return false;
        }

        /// <summary>
        /// Gets the intersection point between three planes
        /// </summary>
        /// <param name="p1">Plane 1</param>
        /// <param name="p2">Plane 2</param>
        /// <param name="p3">Plane 3</param>
        /// <param name="result">The intersection point.</param>
        public static void PlanesIntersectsPoint(ref Plane p1, ref Plane p2, ref Plane p3, out Vector3 result)
        {
            //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
            var p1p2 = Vector3.Cross(p1.Normal, p2.Normal);
            var p2p3 = Vector3.Cross(p2.Normal, p3.Normal);
            var p3p1 = Vector3.Cross(p3.Normal, p1.Normal);
            result = -
                p1.D * p2p3 / Vector3.Dot(p1.Normal, p2p3) -
                p2.D * p3p1 / Vector3.Dot(p2.Normal, p3p1) -
                p3.D * p1p2 / Vector3.Dot(p3.Normal, p1p2);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Minimum.X > box2.Maximum.X || box2.Minimum.X > box1.Maximum.X)
                return false;

            if (box1.Minimum.Y > box2.Maximum.Y || box2.Minimum.Y > box1.Maximum.Y)
                return false;

            if (box1.Minimum.Z > box2.Maximum.Z || box2.Minimum.Z > box1.Maximum.Z)
                return false;

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 166

            Vector3 vector = Vector3.Clamp(sphere.Center, box.Minimum, box.Maximum);
            float distance = Vector3.DistanceSquared(sphere.Center, vector);

            return distance <= sphere.Radius * sphere.Radius;
        }
        /// <summary>
        /// Determines whether a box intersects with a frustum
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the box intersects the frustum</returns>
        public static bool BoxIntersectsFrustum(ref BoundingBox box, ref BoundingFrustum frustum)
        {
            return BoxContainsFrustum(ref box, ref frustum) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a box intersects with a triangle
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the box intersects the triangle</returns>
        /// <remarks>Separating axis theorem implementation adapted from "https://github.com/typicalMoves/dava.engine/blob/main/Sources/Internal/Math/AABBox3.cpp"</remarks>
        public static bool BoxIntersectsTriangle(ref BoundingBox box, ref Triangle triangle)
        {
            // Translate the triangle to origin, and use only the box extents to refer to the box
            var boxCenter = box.Center;
            var boxExtents = box.Extents;
            var origTri = new Triangle(triangle.Point1 - boxCenter, triangle.Point2 - boxCenter, triangle.Point3 - boxCenter);

            // Test first 3 edges
            var edge1 = triangle.GetEdge1();
            float fex = MathF.Abs(edge1.X);
            float fey = MathF.Abs(edge1.Y);
            float fez = MathF.Abs(edge1.Z);
            if (!AxisTestX02(boxExtents, origTri, edge1.Z, edge1.Y, fez, fey)) return false;
            if (!AxisTestY02(boxExtents, origTri, edge1.Z, edge1.X, fez, fex)) return false;
            if (!AxisTestZ12(boxExtents, origTri, edge1.Y, edge1.X, fey, fex)) return false;

            // Test second 3 edges
            var edge2 = triangle.GetEdge2();
            fex = MathF.Abs(edge2.X);
            fey = MathF.Abs(edge2.Y);
            fez = MathF.Abs(edge2.Z);
            if (!AxisTestX02(boxExtents, origTri, edge2.Z, edge2.Y, fez, fey)) return false;
            if (!AxisTestY02(boxExtents, origTri, edge2.Z, edge2.X, fez, fex)) return false;
            if (!AxisTestZ01(boxExtents, origTri, edge2.Y, edge2.X, fey, fex)) return false;

            // Test third 3 edges
            var edge3 = triangle.GetEdge3();
            fex = MathF.Abs(edge3.X);
            fey = MathF.Abs(edge3.Y);
            fez = MathF.Abs(edge3.Z);
            if (!AxisTestX01(boxExtents, origTri, edge3.Z, edge3.Y, fez, fey)) return false;
            if (!AxisTestY01(boxExtents, origTri, edge3.Z, edge3.X, fez, fex)) return false;
            if (!AxisTestZ12(boxExtents, origTri, edge3.Y, edge3.X, fey, fex)) return false;

            // Test X direction
            Helper.MinMax(origTri.Point1.X, origTri.Point2.X, origTri.Point3.X, out float min, out float max);
            if (min > boxExtents.X || max < -boxExtents.X)
            {
                return false;
            }

            // Test Y direction
            Helper.MinMax(origTri.Point1.Y, origTri.Point2.Y, origTri.Point3.Y, out min, out max);
            if (min > boxExtents.Y || max < -boxExtents.Y)
            {
                return false;
            }

            // Test Z direction
            Helper.MinMax(origTri.Point1.Z, origTri.Point2.Z, origTri.Point3.Z, out min, out max);
            if (min > boxExtents.Z || max < -boxExtents.Z)
            {
                return false;
            }

            // Test the box extents vs the triangle plane
            BoundingBox aabb = new(-boxExtents, boxExtents);
            Plane p = triangle.GetPlane();
            return PlaneIntersectsBox(ref p, ref aabb) == PlaneIntersectionType.Intersecting;
        }
        /// <summary>
        /// Determines whether a box intersects with a mesh
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns true if the box intersects the mesh</returns>
        public static bool BoxIntersectsMesh(ref BoundingBox box, Triangle[] mesh)
        {
            for (int i = 0; i < mesh.Length; i++)
            {
                if (BoxContainsTriangle(ref box, ref mesh[i]) != ContainmentType.Disjoint)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Determines whether a box intersects with a mesh
        /// </summary>
        /// <param name="box">Bounding box</param>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns true if the box intersects the mesh</returns>
        public static bool BoxIntersectsMesh(ref BoundingBox box, IEnumerable<Triangle> mesh)
        {
            return BoxIntersectsMesh(ref box, [.. mesh]);
        }
        /// <summary>
        /// Determines whether a box intersects with a plane
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="plane">Plane</param>
        /// <returns>Returns true if the box intersects the plane</returns>
        public static bool BoxIntersectsPlane(ref BoundingBox box, ref Plane plane)
        {
            return PlaneIntersectsBox(ref plane, ref box) == PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingSphere"/> and a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 167

            ClosestPointPointTriangle(ref sphere.Center, ref vertex1, ref vertex2, ref vertex3, out Vector3 point);
            Vector3 v = point - sphere.Center;

            float dot = Vector3.Dot(v, v);

            return dot <= sphere.Radius * sphere.Radius;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;
            return SphereIntersectsTriangle(ref sphere, ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Triangle triangle, out PickingResult<Triangle> result)
        {
            var center = sphere.Center;
            ClosestPointInTriangle(ref center, ref triangle, out float distance, out var point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>
                {
                    Primitive = triangle,
                    Distance = distance,
                    Position = point,
                };

                return true;
            }

            result = new PickingResult<Triangle>
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">First sphere to test.</param>
        /// <param name="sphere2">Second sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float radiisum = sphere1.Radius + sphere2.Radius;
            return Vector3.DistanceSquared(sphere1.Center, sphere2.Center) <= radiisum * radiisum;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a box
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns true if the sphere intersects the box</returns>
        public static bool SphereIntersectsBox(ref BoundingSphere sphere, ref BoundingBox box)
        {
            return BoxIntersectsSphere(ref box, ref sphere);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a frustum
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the sphere intersects the frustum</returns>
        public static bool SphereIntersectsFrustum(ref BoundingSphere sphere, ref BoundingFrustum frustum)
        {
            return SphereContainsFrustum(ref sphere, ref frustum) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(ref BoundingSphere sphere, Triangle[] mesh, out PickingResult<Triangle> result)
        {
            var center = sphere.Center;
            ClosestPointInMesh(ref center, mesh, out var triangle, out float distance, out var point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>
                {
                    Primitive = triangle,
                    Distance = distance,
                    Position = point,
                };

                return true;
            }

            result = new PickingResult<Triangle>
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(ref BoundingSphere sphere, IEnumerable<Triangle> mesh, out PickingResult<Triangle> result)
        {
            return SphereIntersectsMesh(ref sphere, [.. mesh], out result);
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
        public static bool SphereIntersectsMeshAll(ref BoundingSphere sphere, Triangle[] mesh, out PickingResult<Triangle>[] results)
        {
            List<PickingResult<Triangle>> picks = [];

            for (int i = 0; i < mesh.Length; i++)
            {
                var center = sphere.Center;
                var t = mesh[i];
                ClosestPointInTriangle(ref center, ref t, out float dist, out var closestToTri);
                if (dist <= sphere.Radius)
                {
                    // Store intersection data
                    picks.Add(new PickingResult<Triangle> { Primitive = t, Position = closestToTri, Distance = dist });
                }
            }

            results = [.. picks];

            return picks.Count != 0;
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
        public static bool SphereIntersectsMeshAll(ref BoundingSphere sphere, IEnumerable<Triangle> mesh, out PickingResult<Triangle>[] results)
        {
            return SphereIntersectsMeshAll(ref sphere, [.. mesh], out results);
        }
        /// <summary>
        /// Determines whether a sphere intersects with a plane
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="plane">Plane</param>
        /// <returns>Returns true if the sphere intersects the plane</returns>
        public static bool SphereIntersectsPlane(ref BoundingSphere sphere, ref Plane plane)
        {
            return PlaneIntersectsSphere(ref plane, ref sphere) == PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether a frustum intersects with a frustum
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum two</param>
        /// <returns>Returns true if the frustum one intersects the frustum two</returns>
        public static bool FrustumIntersectsFrustum(ref BoundingFrustum frustum1, ref BoundingFrustum frustum2)
        {
            var res1to2 = FrustumContainsFrustum(ref frustum1, ref frustum2);
            if (res1to2 != ContainmentType.Disjoint)
            {
                return true;
            }

            return FrustumContainsFrustum(ref frustum2, ref frustum1) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Vector3 p1, ref Vector3 p2)
        {
            var ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!RayIntersectsFrustum(ref ray, ref frustum, out var inDistance, out _))
            {
                return false;
            }

            if (!inDistance.HasValue || Helper.NearEqual(MathF.Abs(inDistance.Value), float.MaxValue))
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);
            if (inDistance >= rayLength)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <param name="distance">Returns the intersection distance</param>
        /// <param name="point">Returns the intersection point</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Vector3 p1, ref Vector3 p2, out float distance, out Vector3 point)
        {
            distance = float.MaxValue;
            point = Vector3.Zero;

            var ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!RayIntersectsFrustum(ref ray, ref frustum, out var inDistance, out _))
            {
                return false;
            }

            if (!inDistance.HasValue || Helper.NearEqual(MathF.Abs(inDistance.Value), float.MaxValue))
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);
            if (inDistance >= rayLength)
            {
                return false;
            }

            distance = inDistance.Value;
            point = ray.Position + (Vector3.Normalize(ray.Direction) * inDistance.Value);

            return true;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p1">First segment point</param>
        /// <param name="p2">Second segment point</param>
        /// <param name="enteringPoint">Entering segment point</param>
        /// <param name="exitingPoint">Exiting segment point</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Vector3 p1, ref Vector3 p2, out Vector3? enteringPoint, out Vector3? exitingPoint)
        {
            enteringPoint = null;
            exitingPoint = null;

            var ray = new Ray(p1, Vector3.Normalize(p2 - p1));

            if (!RayIntersectsFrustum(ref ray, ref frustum, out var inDistance, out var outDistance))
            {
                return false;
            }

            float rayLength = Vector3.Distance(p1, p2);

            bool intoSegmentBounds = false;

            if (inDistance.HasValue && !Helper.NearEqual(MathF.Abs(inDistance.Value), float.MaxValue) && inDistance <= rayLength)
            {
                enteringPoint = ray.Position + (Vector3.Normalize(ray.Direction) * inDistance.Value);
                intoSegmentBounds = true;
            }

            if (outDistance.HasValue && !Helper.NearEqual(MathF.Abs(outDistance.Value), float.MaxValue) && outDistance <= rayLength)
            {
                exitingPoint = ray.Position + (Vector3.Normalize(ray.Direction) * outDistance.Value);
                intoSegmentBounds = true;
            }

            return intoSegmentBounds;
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="segment">Segment</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Segment segment)
        {
            var p1 = segment.Point1;
            var p2 = segment.Point2;
            return FrustumIntersectsSegment(ref frustum, ref p1, ref p2);
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="segment">Segment</param>
        /// <param name="distance">Returns the intersection distance</param>
        /// <param name="point">Returns the intersection point</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Segment segment, out float distance, out Vector3 point)
        {
            var p1 = segment.Point1;
            var p2 = segment.Point2;
            return FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out distance, out point);
        }
        /// <summary>
        /// Gets whether a frustum is intersected with the specified segment or not
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="segment">Segment</param>
        /// <param name="enteringPoint">Entering segment point</param>
        /// <param name="exitingPoint">Exiting segment point</param>
        /// <returns>Returns true if the frustum is intersected with the specified segment</returns>
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Segment segment, out Vector3? enteringPoint, out Vector3? exitingPoint)
        {
            var p1 = segment.Point1;
            var p2 = segment.Point2;
            return FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out enteringPoint, out exitingPoint);
        }

        /// <summary>
        /// Finds whether the point is contained within the triangle mesh, and the distance between the point and the closest triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="mesh">A mesh.</param>
        /// <returns>A value indicating whether the point is contained within the triangle mesh.</returns>
        public static bool MeshIntersectsPoint(Triangle[] mesh, ref Vector3 p)
        {
            for (int i = 0; i < mesh.Length; i++)
            {
                if (TriangleIntersectsPoint(ref mesh[i], ref p))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Finds whether the point is contained within the triangle mesh, and the distance between the point and the closest triangle.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="mesh">A mesh.</param>
        /// <returns>A value indicating whether the point is contained within the triangle mesh.</returns>
        public static bool MeshIntersectsPoint(IEnumerable<Triangle> mesh, ref Vector3 p)
        {
            return MeshIntersectsPoint([.. mesh], ref p);
        }
        /// <summary>
        /// Determines whether a mesh intersects with a mesh
        /// </summary>
        /// <param name="mesh1">Mesh one</param>
        /// <param name="mesh2">Mesh two</param>
        /// <param name="triangles">Returns the intersected triangles of the mesh two</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the mesh one intersects the mesh two</returns>
        public static bool MeshIntersectsMesh(Triangle[] mesh1, Triangle[] mesh2, out Triangle[] triangles, out Line3D[] segments)
        {
            List<Triangle> tris = [];
            List<Line3D> segs = [];

            for (int i = 0; i < mesh1.Length; i++)
            {
                if (TriangleIntersectsMesh(ref mesh1[i], mesh2, out var mTris, out var mSegs))
                {
                    tris.AddRange(mTris);
                    segs.AddRange(mSegs);
                }
            }

            triangles = [.. tris.Distinct()];
            segments = [.. segs.Distinct()];

            return triangles.Length > 0;
        }
        /// <summary>
        /// Determines whether a mesh intersects with a mesh
        /// </summary>
        /// <param name="mesh1">Mesh one</param>
        /// <param name="mesh2">Mesh two</param>
        /// <param name="triangles">Returns the intersected triangles of the mesh two</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the mesh one intersects the mesh two</returns>
        public static bool MeshIntersectsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2, out Triangle[] triangles, out Line3D[] segments)
        {
            return MeshIntersectsMesh([.. mesh1], [.. mesh2], out triangles, out segments);
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsPoint(ref BoundingBox box, ref Vector3 point)
        {
            if (box.Minimum.X <= point.X && box.Maximum.X >= point.X &&
                box.Minimum.Y <= point.Y && box.Maximum.Y >= point.Y &&
                box.Minimum.Z <= point.Z && box.Maximum.Z >= point.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
                return ContainmentType.Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && (box2.Maximum.X <= box1.Maximum.X &&
                box1.Minimum.Y <= box2.Minimum.Y && box2.Maximum.Y <= box1.Maximum.Y) &&
                box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            Vector3 vector = Vector3.Clamp(sphere.Center, box.Minimum, box.Maximum);
            float distance = Vector3.DistanceSquared(sphere.Center, vector);

            if (distance > sphere.Radius * sphere.Radius)
                return ContainmentType.Disjoint;

            if ((((box.Minimum.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Maximum.X - sphere.Radius)) && ((box.Maximum.X - box.Minimum.X > sphere.Radius) &&
                (box.Minimum.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Maximum.Y - sphere.Radius) && (box.Maximum.Y - box.Minimum.Y > sphere.Radius)) &&
                (((box.Minimum.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Maximum.Z - sphere.Radius)) && (box.Maximum.Z - box.Minimum.Z > sphere.Radius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a BoundingFrustum.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsFrustum(ref BoundingBox box, ref BoundingFrustum frustum)
        {
            if (FrustumBoxCoarse(ref frustum, ref box) == ContainmentType.Disjoint)
            {
                // Not intersection at all
                return ContainmentType.Disjoint;
            }

            // Check whether all the corners of the frustum are inside the box.
            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                if (BoxContainsPoint(ref box, ref corners[i]) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsTriangle(ref BoundingBox box, ref Triangle triangle)
        {
            // Test containment
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;
            var test1 = BoxContainsPoint(ref box, ref p1);
            var test2 = BoxContainsPoint(ref box, ref p2);
            var test3 = BoxContainsPoint(ref box, ref p3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
            {
                // All three points into the box
                return ContainmentType.Contains;
            }

            if (test1 == ContainmentType.Contains || test2 == ContainmentType.Contains || test3 == ContainmentType.Contains)
            {
                // One point at least, into the box
                return ContainmentType.Intersects;
            }

            // Test intersection
            if (BoxIntersectsTriangle(ref box, ref triangle))
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingBox contains a Triangle mesh.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsMesh(ref BoundingBox box, Triangle[] mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            for (int i = 0; i < mesh.Length; i++)
            {
                var c = BoxContainsTriangle(ref box, ref mesh[i]);
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
        /// Determines whether a BoundingBox contains a Triangle mesh.
        /// </summary>
        /// <param name="box">Axis aligned box</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType BoxContainsMesh(ref BoundingBox box, IEnumerable<Triangle> mesh)
        {
            return BoxContainsMesh(ref box, [.. mesh]);
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsPoint(ref BoundingSphere sphere, ref Vector3 point)
        {
            if (Vector3.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsTriangle(ref BoundingSphere sphere, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            //Source: Jorgy343
            //Reference: None

            ContainmentType test1 = SphereContainsPoint(ref sphere, ref vertex1);
            ContainmentType test2 = SphereContainsPoint(ref sphere, ref vertex2);
            ContainmentType test3 = SphereContainsPoint(ref sphere, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (SphereIntersectsTriangle(ref sphere, ref vertex1, ref vertex2, ref vertex3))
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsTriangle(ref BoundingSphere sphere, ref Triangle triangle)
        {
            var p1 = triangle.Point1;
            var p2 = triangle.Point2;
            var p3 = triangle.Point3;
            return SphereContainsTriangle(ref sphere, ref p1, ref p2, ref p3);
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsBox(ref BoundingSphere sphere, ref BoundingBox box)
        {
            Vector3 vector;

            if (!BoxIntersectsSphere(ref box, ref sphere))
                return ContainmentType.Disjoint;

            float radiussquared = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float distance = Vector3.Distance(sphere1.Center, sphere2.Center);

            if (sphere1.Radius + sphere2.Radius < distance)
                return ContainmentType.Disjoint;

            if (sphere1.Radius - sphere2.Radius < distance)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a BoundingFrustum.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsFrustum(ref BoundingSphere sphere, ref BoundingFrustum frustum)
        {
            if (FrustumSphereCoarse(ref frustum, ref sphere) == ContainmentType.Disjoint)
            {
                // Not intersection at all
                return ContainmentType.Disjoint;
            }

            // Check whether all the corners of the frustum are inside the sphere.
            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                if (SphereContainsPoint(ref sphere, ref corners[i]) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingSphere contains a Triangle mesh.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsMesh(ref BoundingSphere sphere, Triangle[] mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            for (int i = 0; i < mesh.Length; i++)
            {
                var c = SphereContainsTriangle(ref sphere, ref mesh[i]);
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
        /// Determines whether a BoundingSphere contains a Triangle mesh.
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType SphereContainsMesh(ref BoundingSphere sphere, IEnumerable<Triangle> mesh)
        {
            return SphereContainsMesh(ref sphere, [.. mesh]);
        }

        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public static ContainmentType FrustumContainsPoint(ref BoundingFrustum frustum, ref Vector3 point)
        {
            var result = PlaneIntersectionType.Front;
            for (int i = 0; i < 6; i++)
            {
                var p = frustum.GetPlane(i);
                var planeResult = PlaneIntersectsPoint(ref p, ref point);
                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }

            return result switch
            {
                PlaneIntersectionType.Intersecting => ContainmentType.Intersects,
                _ => ContainmentType.Contains,
            };
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingSphere.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsSphere(ref BoundingFrustum frustum, ref BoundingSphere sphere)
        {
            return FrustumSphereCoarse(ref frustum, ref sphere);
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingBox.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsBox(ref BoundingFrustum frustum, ref BoundingBox box)
        {
            if (FrustumBoxCoarse(ref frustum, ref box) == ContainmentType.Disjoint)
            {
                // Not intersection at all
                return ContainmentType.Disjoint;
            }

            // Check whether all the corners of the box are inside the frustum.
            var corners = box.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                if (FrustumContainsPoint(ref frustum, ref corners[i]) == ContainmentType.Disjoint)
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a BoundingFrustum.
        /// </summary>
        /// <param name="frustum1">Frustum one</param>
        /// <param name="frustum2">Frustum two</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsFrustum(ref BoundingFrustum frustum1, ref BoundingFrustum frustum2)
        {
            if (frustum1 == frustum2)
            {
                return ContainmentType.Contains;
            }

            //Test frustum2 corners against frustum1 (cheaper test first)
            var corners2 = frustum2.GetCorners();
            var f1Contains2 = FrustumContainsPoints(ref frustum1, corners2);
            if (f1Contains2 != ContainmentType.Disjoint)
            {
                //This test only validates points, not edges.
                //But the intersection/contains result is valid, frustum2 is into the frustum1, or intersection exists between them
                return f1Contains2;
            }

            //Test frustum1 corners against frustum2
            var corners1 = frustum1.GetCorners();
            var f2Contains1 = FrustumContainsPoints(ref frustum2, corners1);
            if (f2Contains1 != ContainmentType.Disjoint)
            {
                //If frustum1 is into the frustum2, the result is intersection, not contains.
                return ContainmentType.Intersects;
            }

            //Test near to far segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners2[i + 0];
                var p2 = corners2[i + 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test near plane segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners2[i + 0];
                var p2 = corners2[(i + 1) % 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test far plane segments
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners2[i + 4];
                var p2 = corners2[((i + 1) % 4) + 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2))
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a Triangle.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsTriangle(ref BoundingFrustum frustum, ref Triangle triangle)
        {
            var vertices = triangle.GetVertices().ToArray();

            List<Vector3> inVerts = [];
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                if (FrustumContainsPoint(ref frustum, ref v) != ContainmentType.Disjoint)
                {
                    inVerts.Add(v);
                }
            }

            if (inVerts.Count == vertices.Length)
            {
                // All vertices were inside the trunk
                return ContainmentType.Contains;
            }

            if (inVerts.Count != 0)
            {
                // At least one vertex into the frustum
                return ContainmentType.Intersects;
            }

            // Test triangle segments
            var edges = triangle.GetEdgeSegments();
            for (int i = 0; i < edges.Length; i++)
            {
                if (FrustumIntersectsSegment(ref frustum, ref edges[i]))
                {
                    return ContainmentType.Intersects;
                }
            }

            return ContainmentType.Disjoint;
        }
        /// <summary>
        /// Determines whether a BoundingFrustum contains a Triangle mesh.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsMesh(ref BoundingFrustum frustum, Triangle[] mesh)
        {
            ContainmentType res = ContainmentType.Disjoint;

            for (int i = 0; i < mesh.Length; i++)
            {
                var c = FrustumContainsTriangle(ref frustum, ref mesh[i]);
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
        /// Determines whether a BoundingFrustum contains a Triangle mesh.
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType FrustumContainsMesh(ref BoundingFrustum frustum, IEnumerable<Triangle> mesh)
        {
            return FrustumContainsMesh(ref frustum, [.. mesh]);
        }
        /// <summary>
        /// Gets whether the frustum contains the point list
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="points">Point collection</param>
        public static ContainmentType FrustumContainsPoints(ref BoundingFrustum frustum, Vector3[] points)
        {
            bool allCornersContained = true;
            bool intersects = false;
            foreach (var vertex in points)
            {
                var v = vertex;
                var res = FrustumContainsPoint(ref frustum, ref v);
                if (res != ContainmentType.Disjoint)
                {
                    intersects = true;
                }
                else
                {
                    allCornersContained = false;
                }
            }

            if (!intersects)
            {
                return ContainmentType.Disjoint;
            }

            if (allCornersContained)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }
        /// <summary>
        /// Gets whether the frustum contains the point list
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="points">Point collection</param>
        public static ContainmentType FrustumContainsPoints(ref BoundingFrustum frustum, IEnumerable<Vector3> points)
        {
            return FrustumContainsPoints(ref frustum, [.. points]);
        }

        /// <summary>
        /// Gets whether the capsule contains the point
        /// </summary>
        /// <param name="capsule">Capsule</param>
        /// <param name="point">Point</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType CapsuleContainsPoint(ref BoundingCapsule capsule, ref Vector3 point)
        {
            // Find capsule points
            var p1 = capsule.BasePosition;
            var p2 = capsule.CapPosition;

            // Find closest point of point in axis
            ClosestPointInSegment(ref p1, ref p2, ref point, out float distance, out _);
            if (distance > capsule.Radius)
            {
                // Outside capsule radius
                return ContainmentType.Disjoint;
            }

            if (Helper.NearEqual(distance, capsule.Radius))
            {
                // In the capsule
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Gets whether the cylinder contains the point
        /// </summary>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="point">Point</param>
        /// <returns></returns>
        public static ContainmentType CylinderContainsPoint(ref BoundingCylinder cylinder, ref Vector3 point)
        {
            // Find cylinder central axis
            var p1 = cylinder.BasePosition;
            var p2 = cylinder.CapPosition;

            // Find closest point of point in axis
            var ray = Ray.FromPoints(p1, p2);
            ClosestPointInRay(ref ray, ref point, out float distance, out var closest);
            if (distance > cylinder.Radius)
            {
                // Outside cylinder radius
                return ContainmentType.Disjoint;
            }

            // Find distance from closest point in axis to the center
            float hh = cylinder.Height * 0.5f;
            float distOnAxis = Vector3.Distance(cylinder.Center, closest);
            if (distOnAxis > hh)
            {
                // Outside cap and base distances
                return ContainmentType.Disjoint;
            }

            if (Helper.NearEqual(distance, cylinder.Radius))
            {
                // The point is into the cylinder radius, and between cap and base distances.
                return ContainmentType.Intersects;
            }

            if (Helper.NearEqual(distOnAxis, hh))
            {
                // The point is in the cap or base.
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingSphere.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsSphere(Triangle[] mesh, ref BoundingSphere sphere)
        {
            var mbox = BoundingBox.FromPoints([.. mesh.SelectMany(t => t.GetVertices())]);

            return BoxContainsSphere(ref mbox, ref sphere);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingSphere.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsSphere(IEnumerable<Triangle> mesh, ref BoundingSphere sphere)
        {
            return MeshContainsSphere([.. mesh], ref sphere);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingBox.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsBox(Triangle[] mesh, ref BoundingBox box)
        {
            var mbox = BoundingBox.FromPoints([.. mesh.SelectMany(t => t.GetVertices())]);

            return BoxContainsBox(ref mbox, ref box);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingBox.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsBox(IEnumerable<Triangle> mesh, ref BoundingBox box)
        {
            return MeshContainsBox([.. mesh], ref box);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingFrustum.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsFrustum(Triangle[] mesh, ref BoundingFrustum frustum)
        {
            var mbox = BoundingBox.FromPoints([.. mesh.SelectMany(t => t.GetVertices())]);

            return BoxContainsFrustum(ref mbox, ref frustum);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingFrustum.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsFrustum(IEnumerable<Triangle> mesh, ref BoundingFrustum frustum)
        {
            return MeshContainsFrustum([.. mesh], ref frustum);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a Triangle mesh.
        /// </summary>
        /// <param name="mesh1">Triangle mesh one</param>
        /// <param name="mesh2">Triangle mesh two</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsMesh(Triangle[] mesh1, Triangle[] mesh2)
        {
            // A mesh is not a volume. Calculate the bounding volumes of the tow meshes
            var bbox1 = BoundingBox.FromPoints([.. mesh1.SelectMany(t => t.GetVertices())]);
            var bbox2 = BoundingBox.FromPoints([.. mesh2.SelectMany(t => t.GetVertices())]);

            if (BoxContainsBox(ref bbox1, ref bbox2) == ContainmentType.Disjoint)
            {
                // If volumes disjoint, no intersection is possible
                return ContainmentType.Disjoint;
            }

            if (BoxContainsBox(ref bbox2, ref bbox1) == ContainmentType.Contains)
            {
                // Box2 contains box1. Then box1 intersects box2
                return ContainmentType.Intersects;
            }

            // Test each triangle of the mesh
            for (int i = 0; i < mesh1.Length; i++)
            {
                if (TriangleIntersectsMesh(ref mesh1[i], mesh2, out _))
                {
                    return ContainmentType.Intersects;
                }
            }

            // If no intersection found, must be a containment result
            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a Triangle mesh.
        /// </summary>
        /// <param name="mesh1">Triangle mesh one</param>
        /// <param name="mesh2">Triangle mesh two</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsMesh(IEnumerable<Triangle> mesh1, IEnumerable<Triangle> mesh2)
        {
            return MeshContainsMesh([.. mesh1], [.. mesh2]);
        }

        private static bool AxisTestX01(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p1 = a * tri.Point2.Y - b * tri.Point2.Z;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestX02(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p2 = a * tri.Point3.Y - b * tri.Point3.Z;
            Helper.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY01(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p1 = -a * tri.Point2.X + b * tri.Point2.Z;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY02(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p2 = -a * tri.Point3.X + b * tri.Point3.Z;
            Helper.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ01(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.X - b * tri.Point1.Y;
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            Helper.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Y;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ12(Vector3 extents, Triangle tri, float a, float b, float fa, float fb)
        {
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            float p2 = a * tri.Point3.X - b * tri.Point3.Y;
            Helper.MinMax(p1, p2, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Y;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }

        private static ContainmentType FrustumBoxCoarse(ref BoundingFrustum frustum, ref BoundingBox box)
        {
            var result = ContainmentType.Contains;

            for (int i = 0; i < 6; i++)
            {
                var plane = frustum.GetPlane(i);
                GetBoxToPlanePVertexNVertex(ref box, ref plane.Normal, out var p, out var n);
                if (PlaneIntersectsPoint(ref plane, ref p) == PlaneIntersectionType.Back)
                {
                    return ContainmentType.Disjoint;
                }

                if (PlaneIntersectsPoint(ref plane, ref n) == PlaneIntersectionType.Back)
                {
                    result = ContainmentType.Intersects;
                }
            }

            return result;
        }
        private static ContainmentType FrustumSphereCoarse(ref BoundingFrustum frustum, ref BoundingSphere sphere)
        {
            var result = PlaneIntersectionType.Front;

            for (int i = 0; i < 6; i++)
            {
                var p = frustum.GetPlane(i);
                var planeResult = PlaneIntersectsSphere(ref p, ref sphere);
                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }

            if (result == PlaneIntersectionType.Intersecting)
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }
        private static void GetBoxToPlanePVertexNVertex(ref BoundingBox box, ref Vector3 planeNormal, out Vector3 p, out Vector3 n)
        {
            p = box.Minimum;
            if (planeNormal.X >= 0) p.X = box.Maximum.X;
            if (planeNormal.Y >= 0) p.Y = box.Maximum.Y;
            if (planeNormal.Z >= 0) p.Z = box.Maximum.Z;

            n = box.Maximum;
            if (planeNormal.X >= 0) n.X = box.Minimum.X;
            if (planeNormal.Y >= 0) n.Y = box.Minimum.Y;
            if (planeNormal.Z >= 0) n.Z = box.Minimum.Z;
        }
    }
}
