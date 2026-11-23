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
                return one.Contains(twoMesh.GetTriangles()) != ContainmentType.Disjoint;
            }

            return false;
        }

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
            Vector3.Subtract(ref vertex2, ref vertex1, out Vector3 ab);
            Vector3.Subtract(ref vertex3, ref vertex1, out Vector3 ac);
            Vector3.Subtract(ref point, ref vertex1, out Vector3 ap);

            Vector3.Dot(ref ab, ref ap, out float d1);
            Vector3.Dot(ref ac, ref ap, out float d2);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result = vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            Vector3.Subtract(ref point, ref vertex2, out Vector3 bp);
            Vector3.Dot(ref ab, ref bp, out float d3);
            Vector3.Dot(ref ac, ref bp, out float d4);
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
            Vector3.Subtract(ref point, ref vertex3, out Vector3 cp);
            Vector3.Dot(ref ab, ref cp, out float d5);
            Vector3.Dot(ref ac, ref cp, out float d6);
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

            Vector3.Dot(ref plane.Normal, ref point, out float dot);
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

            Vector3.Max(ref point, ref box.Minimum, out Vector3 max);
            Vector3.Min(ref max, ref box.Maximum, out result);
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
            Vector3.Subtract(ref point, ref sphere.Center, out result);
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
            Vector3.Subtract(ref sphere2.Center, ref sphere1.Center, out result);
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
            Vector3.Subtract(ref point, ref ray.Position, out var to);
            Vector3.Normalize(ref ray.Direction, out var dir);

            Vector3.Dot(ref to, ref dir, out var dist);
            if (dist < 0f)
            {
                result = ray.Position;
                return;
            }

            float rayLength = ray.Direction.Length();
            if (dist > rayLength)
            {
                result = ray.Position + ray.Direction;
                return;
            }

            result = ray.Position + (dir * dist);
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

            Vector3.Distance(ref result, ref point, out distance);
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
            Vector3.Subtract(ref point2, ref point1, out var ab);

            // Project point onto segment
            distance = Vector3.Dot(point - point1, ab);
            if (distance <= 0f)
            {
                // point projects outside the segment on the point1 side
                distance = -distance;
                result = point1;
                return;
            }

            Vector3.Dot(ref ab, ref ab, out float denom);
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
            ClosestPointInSegment(ref segment.Point1, ref segment.Point2, ref point, out distance, out result);
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
            Vector3.Subtract(ref point, ref center, out Vector3 diff);

            var vR = transform.Right;
            var vU = transform.Up;
            var vB = transform.Backward;
            Vector3.Dot(ref diff, ref vR, out float closestX);
            Vector3.Dot(ref diff, ref vU, out float closestY);
            Vector3.Dot(ref diff, ref vB, out float closestZ);

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
            ref Vector3 a = ref triangle.Point1;
            ref Vector3 b = ref triangle.Point2;
            ref Vector3 c = ref triangle.Point3;

            // Check if P in vertex region outside A
            Vector3.Subtract(ref b, ref a, out Vector3 ab);
            Vector3.Subtract(ref c, ref a, out Vector3 ac);
            Vector3.Subtract(ref point, ref a, out Vector3 ap);
            Vector3.Dot(ref ab, ref ap, out float d1);
            Vector3.Dot(ref ac, ref ap, out float d2);
            if (d1 <= 0f && d2 <= 00f)
            {
                // barycentric coordinates (1,0,0)
                result = a;
                return;
            }

            // Check if P in vertex region outside B
            Vector3.Subtract(ref point, ref b, out Vector3 bp);
            Vector3.Dot(ref ab, ref bp, out float d3);
            Vector3.Dot(ref ac, ref bp, out float d4);
            if (d3 >= 0f && d4 <= d3)
            {
                // barycentric coordinates (0,1,0)
                result = b;
                return;
            }

            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
            {
                // barycentric coordinates (1-v,v,0)
                float v = d1 / (d1 - d3);
                result = new()
                {
                    X = a.X + v * ab.X,
                    Y = a.Y + v * ab.Y,
                    Z = a.Z + v * ab.Z,
                };
                return;
            }

            // Check if P in vertex region outside C
            Vector3.Subtract(ref point, ref c, out Vector3 cp);
            Vector3.Dot(ref ab, ref cp, out float d5);
            Vector3.Dot(ref ac, ref cp, out float d6);
            if (d6 >= 0f && d5 <= d6)
            {
                // barycentric coordinates (0,0,1)
                result = c;
                return;
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
            {
                // barycentric coordinates (1-w,0,w)
                float w = d2 / (d2 - d6);
                result = new()
                {
                    X = a.X + w * ac.X,
                    Y = a.Y + w * ac.Y,
                    Z = a.Z + w * ac.Z,
                };
                return;
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
            {
                // barycentric coordinates (0,1-w,w)
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = new()
                {
                    X = b.X + w * (c.X - b.X),
                    Y = b.Y + w * (c.Y - b.Y),
                    Z = b.Z + w * (c.Z - b.Z),
                };
                return;
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1f / (va + vb + vc);
            float closestV = vb * denom;
            float closestW = vc * denom;
            result = new()
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
            closest = new();
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
        public static void ClosestPointInMesh(ref Vector3 point, Triangle[] mesh, out Triangle closest, out float distance, out Vector3 result)
        {
            ClosestPointInMesh(ref point, mesh, out closest, out result);

            Vector3.Distance(ref point, ref result, out distance);
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

            if (point.X < box.Minimum.X) distance += (box.Minimum.X - point.X) * (box.Minimum.X - point.X);
            if (point.X > box.Maximum.X) distance += (point.X - box.Maximum.X) * (point.X - box.Maximum.X);

            if (point.Y < box.Minimum.Y) distance += (box.Minimum.Y - point.Y) * (box.Minimum.Y - point.Y);
            if (point.Y > box.Maximum.Y) distance += (point.Y - box.Maximum.Y) * (point.Y - box.Maximum.Y);

            if (point.Z < box.Minimum.Z) distance += (box.Minimum.Z - point.Z) * (box.Minimum.Z - point.Z);
            if (point.Z > box.Maximum.Z) distance += (point.Z - box.Maximum.Z) * (point.Z - box.Maximum.Z);

            return MathF.Sqrt(distance);
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

            return MathF.Sqrt(distance);
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

            Vector3.Distance(ref sphere.Center, ref point, out float distance);
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

            Vector3.Distance(ref sphere1.Center, ref sphere2.Center, out float distance);
            distance -= sphere1.Radius + sphere2.Radius;

            return Math.Max(distance, 0f);
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

            Vector3.Subtract(ref ray.Position, ref point, out Vector3 m);

            //Same thing as RayIntersectsSphere except that the radius of the sphere (point)
            //is the epsilon for zero.
            Vector3.Dot(ref m, ref ray.Direction, out float b);
            Vector3.Dot(ref m, ref m, out float c);
            c -= MathUtils.ZeroTolerance;

            if (c > 0f && b > 0f) return false;

            float discriminant = b * b - c;

            if (discriminant < 0f) return false;

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
        public static bool RayIntersectsRay(ref Ray ray1, ref Ray ray2, out Vector3 point)
        {
            return RayIntersectsRay(
                ref ray1.Position, ref ray1.Direction,
                ref ray2.Position, ref ray2.Direction,
                out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="rPos1">First ray position</param>
        /// <param name="rDir1">First ray direction</param>
        /// <param name="rPos2">Second ray position</param>
        /// <param name="rDir2">Second ray direction</param>
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
        public static bool RayIntersectsRay(ref Vector3 rPos1, ref Vector3 rDir1, ref Vector3 rPos2, ref Vector3 rDir2, out Vector3 point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780

            Vector3.Cross(ref rDir1, ref rDir2, out Vector3 cross);
            float denominator = cross.Length();

            //Lines are parallel and on top of each other.
            if (MathUtils.IsZero(denominator) &&
                MathUtils.NearEqual(rPos2.X, rPos1.X) &&
                MathUtils.NearEqual(rPos2.Y, rPos1.Y) &&
                MathUtils.NearEqual(rPos2.Z, rPos1.Z))
            {
                point = Vector3.Zero;
                return true;
            }

            denominator *= denominator;

            //3x3 matrix for the first ray.
            float m11 = rPos2.X - rPos1.X;
            float m12 = rPos2.Y - rPos1.Y;
            float m13 = rPos2.Z - rPos1.Z;
            float m21 = rDir2.X;
            float m22 = rDir2.Y;
            float m23 = rDir2.Z;
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
            m21 = rDir1.X;
            m22 = rDir1.Y;
            m23 = rDir1.Z;

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
            var point1 = rPos1 + (s * rDir1);
            var point2 = rPos2 + (t * rDir2);

            //If the points are not equal, no intersection has occurred.
            if (!MathUtils.NearEqual(point2.X, point1.X) ||
                !MathUtils.NearEqual(point2.Y, point1.Y) ||
                !MathUtils.NearEqual(point2.Z, point1.Z))
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
            var sDir = segment.Direction;
            var sLength = segment.Length;
            return RayIntersectsSegment(
                ref ray.Position, ref ray.Direction,
                ref segment.Point1, ref sDir, ref sLength,
                out distance, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Segment"/>.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="sPos">Segment position</param>
        /// <param name="sDir">Segment direction</param>
        /// <param name="sLength">Segment length</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSegment(ref Vector3 rPos, ref Vector3 rDir, ref Vector3 sPos, ref Vector3 sDir, ref float sLength, out float distance, out Vector3 point)
        {
            bool touch = RayIntersectsRay(ref rPos, ref rDir, ref sPos, ref sDir, out Vector3 p);
            if (!touch)
            {
                point = Vector3.Zero;
                distance = float.MaxValue;

                return false;
            }

            Vector3.Distance(ref rPos, ref p, out float d);
            if (d > sLength)
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
            var sDir = segment.Direction;
            var sLength = segment.Length;
            return RayIntersectsSegment(
                ref ray.Position, ref ray.Direction,
                ref segment.Point1, ref sDir, ref sLength,
                out _, out point);
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
            var sDir = segment.Direction;
            var sLength = segment.Length;
            return RayIntersectsSegment(
                ref ray.Position, ref ray.Direction,
                ref segment.Point1, ref sDir, ref sLength,
                out distance, out point);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref Vector3 rPos, ref Vector3 rDir, ref Plane plane, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            Vector3.Dot(ref plane.Normal, ref rDir, out float direction);

            if (MathUtils.IsZero(direction))
            {
                distance = 0f;
                return false;
            }

            Vector3.Dot(ref plane.Normal, ref rPos, out float position);
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
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="plane">The plane to test</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsPlane(ref Vector3 rPos, ref Vector3 rDir, ref Plane plane, out float distance, out Vector3 point)
        {
            if (!RayIntersectsPlane(ref rPos, ref rDir, ref plane, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = rPos + (rDir * d);
            return true;
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
            return RayIntersectsPlane(ref ray.Position, ref ray.Direction, ref plane, out distance);
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
            return RayIntersectsPlane(ref ray.Position, ref ray.Direction, ref plane, out distance, out point);
        }

        /// <summary>
        /// Determine whether the specified direction vector is oriented against the triangle's normal.
        /// </summary>
        /// <param name="v">Direction vector</param>
        /// <param name="t1">Triangle point 1</param>
        /// <param name="t2">Triangle point 2</param>
        /// <param name="t3">Triangle point 3</param>
        /// <returns>Returns true if the specified direction vector is oriented against the triangle's normal.</returns>
        static bool VectorFacingTriangle(ref Vector3 v, ref Vector3 t1, ref Vector3 t2, ref Vector3 t3)
        {
            MathUtils.CalculateTriPointNormalLH(ref t1, ref t2, ref t3, out var normal);
            Vector3.Dot(ref v, ref normal, out float l);
            return l < 0f;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
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
        public static bool RayIntersectsTriangle(ref Vector3 rPos, ref Vector3 rDir, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            //Source: Fast Minimum Storage Ray / Triangle Intersection
            //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            //Compute vectors along two edges of the triangle.
            Vector3.Normalize(ref rDir, out var dir);

            //Edge 1
            Vector3.Subtract(ref vertex2, ref vertex1, out Vector3 edge1);

            //Edge2
            Vector3.Subtract(ref vertex3, ref vertex1, out Vector3 edge2);

            //Cross product of ray direction and edge2 - first part of determinant.
            Vector3.Cross(ref dir, ref edge2, out Vector3 directioncrossedge2);

            //Compute the determinant.
            //Dot product of edge1 and the first part of determinant.
            float determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            //If the ray is parallel to the triangle plane, there is no collision.
            //This also means that we are not culling, the ray may hit both the
            //back and the front of the triangle.
            if (MathUtils.IsZero(determinant))
            {
                distance = float.MaxValue;
                return false;
            }

            float inversedeterminant = 1.0f / determinant;

            //Calculate the U parameter of the intersection point.
            Vector3.Subtract(ref rPos, ref vertex1, out Vector3 distanceVector);

            float triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleU < 0f || triangleU > 1f)
            {
                distance = float.MaxValue;
                return false;
            }

            //Calculate the V parameter of the intersection point.
            Vector3.Cross(ref distanceVector, ref edge1, out Vector3 distancecrossedge1);

            float triangleV = (dir.X * distancecrossedge1.X) + (dir.Y * distancecrossedge1.Y) + (dir.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleV < 0f || triangleU + triangleV > 1f)
            {
                distance = float.MaxValue;
                return false;
            }

            //Compute the distance along the ray to the triangle.
            float raydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
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
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the ray intersects either the front or back of the triangle.
        /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
        /// happened. If the intersection of the ray and the triangle is behind the origin of
        /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref Vector3 rPos, ref Vector3 rDir, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance, out Vector3 point)
        {
            if (!RayIntersectsTriangle(ref rPos, ref rDir, ref vertex1, ref vertex2, ref vertex3, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = rPos + (Vector3.Normalize(rDir) * d);
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
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            return RayIntersectsTriangle(
                ref ray.Position, ref ray.Direction,
                ref vertex1, ref vertex2, ref vertex3,
                out distance);
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
            return RayIntersectsTriangle(
                ref ray.Position, ref ray.Direction,
                ref vertex1, ref vertex2, ref vertex3,
                out distance, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Triangle tri, out float distance)
        {
            return RayIntersectsTriangle(
                ref ray.Position, ref ray.Direction,
                ref tri.Point1, ref tri.Point2, ref tri.Point3,
                out distance);
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
            return RayIntersectsTriangle(
                ref ray.Position, ref ray.Direction,
                ref tri.Point1, ref tri.Point2, ref tri.Point3,
                out distance, out point);
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
            var rDir = ray.Direction;
            if (ray.FacingOnly && !VectorFacingTriangle(ref rDir, ref vertex1, ref vertex2, ref vertex3))
            {
                distance = float.MaxValue;
                return false;
            }

            if (!RayIntersectsTriangle(ref ray.Position, ref rDir, ref vertex1, ref vertex2, ref vertex3, out float d))
            {
                distance = float.MaxValue;
                return false;
            }

            if (d > ray.Length)
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
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance, out Vector3 point)
        {
            if (!RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = ray.Position + ray.Direction * d;
            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="PickingRay"/> and a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="tri">Triangle</param>
        /// <param name="distance">Distance to point</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref PickingRay ray, ref Triangle tri, out float distance)
        {
            return RayIntersectsTriangle(ref ray, ref tri.Point1, ref tri.Point2, ref tri.Point3, out distance);
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
            return RayIntersectsTriangle(ref ray, ref tri.Point1, ref tri.Point2, ref tri.Point3, out distance, out point);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Vector3 rPos, ref Vector3 rDir, ref BoundingBox box, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (MathUtils.IsZero(rDir.X))
            {
                if (rPos.X < box.Minimum.X || rPos.X > box.Maximum.X)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1f / rDir.X;
                float t1 = (box.Minimum.X - rPos.X) * inverse;
                float t2 = (box.Maximum.X - rPos.X) * inverse;

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

            if (MathUtils.IsZero(rDir.Y))
            {
                if (rPos.Y < box.Minimum.Y || rPos.Y > box.Maximum.Y)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1f / rDir.Y;
                float t1 = (box.Minimum.Y - rPos.Y) * inverse;
                float t2 = (box.Maximum.Y - rPos.Y) * inverse;

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

            if (MathUtils.IsZero(rDir.Z))
            {
                if (rPos.Z < box.Minimum.Z || rPos.Z > box.Maximum.Z)
                {
                    distance = float.MaxValue;
                    return false;
                }
            }
            else
            {
                float inverse = 1f / rDir.Z;
                float t1 = (box.Minimum.Z - rPos.Z) * inverse;
                float t2 = (box.Maximum.Z - rPos.Z) * inverse;

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
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Vector3 rPos, ref Vector3 rDir, ref BoundingBox box, out float distance, out Vector3 point)
        {
            if (!RayIntersectsBox(ref rPos, ref rDir, ref box, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = rPos + (rDir * d);
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
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance)
        {
            return RayIntersectsBox(ref ray.Position, ref ray.Direction, ref box, out distance);
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
            return RayIntersectsBox(ref ray.Position, ref ray.Direction, ref box, out distance, out point);
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
            var rDir = ray.Direction;
            if (!RayIntersectsBox(ref ray.Position, ref rDir, ref box, out float d))
            {
                distance = float.MaxValue;
                return false;
            }

            if (d > ray.Length)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = d;
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
        public static bool RayIntersectsBox(ref PickingRay ray, ref BoundingBox box, out float distance, out Vector3 point)
        {
            if (!RayIntersectsBox(ref ray, ref box, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = ray.Position + (ray.Direction * d);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Vector3 rPos, ref Vector3 rDir, ref BoundingSphere sphere, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 177

            Vector3.Subtract(ref rPos, ref sphere.Center, out Vector3 m);

            Vector3.Dot(ref m, ref rDir, out float b);
            Vector3.Dot(ref m, ref m, out float c);
            c -= sphere.Radius * sphere.Radius;

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
            {
                distance = 0f;
            }

            return true;
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>. 
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Vector3 rPos, ref Vector3 rDir, ref BoundingSphere sphere, out float distance, out Vector3 point)
        {
            if (!RayIntersectsSphere(ref rPos, ref rDir, ref sphere, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = rPos + (Vector3.Normalize(rDir) * d);
            return true;
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
            return RayIntersectsSphere(ref ray.Position, ref ray.Direction, ref sphere, out distance);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>. 
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out float distance, out Vector3 point)
        {
            return RayIntersectsSphere(ref ray.Position, ref ray.Direction, ref sphere, out distance, out point);
        }
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref PickingRay ray, ref BoundingSphere sphere, out float distance)
        {
            if (!RayIntersectsSphere(ref ray.Position, ref ray.Direction, ref sphere, out float d))
            {
                distance = float.MaxValue;
                return false;
            }

            if (d > ray.Length)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = d;
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
        public static bool RayIntersectsSphere(ref PickingRay ray, ref BoundingSphere sphere, out float distance, out Vector3 point)
        {
            if (!RayIntersectsSphere(ref ray, ref sphere, out distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (Vector3.Normalize(ray.Direction) * distance);
            return true;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="rPos">Ray position</param>
        /// <param name="rDir">Ray direction</param>
        /// <param name="frustum">The BoundingFrustum to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public static bool RayIntersectsFrustum(ref Vector3 rPos, ref Vector3 rDir, ref BoundingFrustum frustum, out float? inDistance, out float? outDistance)
        {
            if (FrustumContainsPoint(ref frustum, ref rPos) != ContainmentType.Disjoint)
            {
                float nearstPlaneDistance = float.MaxValue;

                for (int i = 0; i < 6; i++)
                {
                    var plane = frustum.GetPlane(i);

                    if (RayIntersectsPlane(ref rPos, ref rDir, ref plane, out float distance) && distance < nearstPlaneDistance)
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

                    if (RayIntersectsPlane(ref rPos, ref rDir, ref plane, out float distance))
                    {
                        minDist = Math.Min(minDist, distance);
                        maxDist = Math.Max(maxDist, distance);
                    }
                }

                Vector3 minPoint = rPos + rDir * minDist;
                Vector3 maxPoint = rPos + rDir * maxDist;
                Vector3 center = (minPoint + maxPoint) * 0.5f;
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
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The Ray to check for intersection with.</param>
        /// <param name="frustum">The BoundingFrustum to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public static bool RayIntersectsFrustum(ref Ray ray, ref BoundingFrustum frustum, out float? inDistance, out float? outDistance)
        {
            return RayIntersectsFrustum(ref ray.Position, ref ray.Direction, ref frustum, out inDistance, out outDistance);
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
            if (!RayIntersectsPlane(ref ray, ref pFrom, out float d, out Vector3 p))
            {
                return false;
            }

            if (d > radius)
            {
                return false;
            }

            distance = d;
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
            var rDir = segment.Direction;
            if (!RayIntersectsBox(ref segment.Point1, ref rDir, ref box, out float d))
            {
                distance = float.MaxValue;
                return true;
            }

            if (d > segment.Length)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = d;
            return true;
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
            if (!SegmentIntersectsBox(ref segment, ref box, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = segment.Point1 + (segment.Direction * d);
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
            var rDir = segment.Direction;
            if (!RayIntersectsTriangle(ref segment.Point1, ref rDir, ref tri.Point1, ref tri.Point2, ref tri.Point3, out float d))
            {
                distance = float.MaxValue;
                return true;
            }

            if (d > segment.Length)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = d;
            return true;
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
            if (!SegmentIntersectsTriangle(ref segment, ref tri, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            distance = d;
            point = segment.Point1 + segment.Direction * d;
            return true;
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
            Vector3.Subtract(ref triangle.Point1, ref point, out var p1);
            Vector3.Subtract(ref triangle.Point2, ref point, out var p2);
            Vector3.Subtract(ref triangle.Point3, ref point, out var p3);

            // As the point is in the center, all the triangle normals must be facing to the same direction
            Vector3.Cross(ref p2, ref p3, out var u);
            Vector3.Cross(ref p3, ref p1, out var v);
            Vector3.Dot(ref u, ref v, out float uv);
            if (uv < 0f) return false;

            Vector3.Cross(ref p1, ref p2, out var w);
            Vector3.Dot(ref u, ref w, out float uw);
            if (uw < 0f) return false;

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
            return IntersectionTriangle.Intersection(ref triangle1, ref triangle2);
        }
        /// <summary>
        /// Determines whether the triangles intersects or not
        /// </summary>
        /// <param name="triangle1">Triangle one</param>
        /// <param name="triangle2">Triangle two</param>
        /// <param name="segment">Returns the intersection segment (if the triangles are not coplanar)</param>
        /// <returns>Returns true if the triangles intersects</returns>
        public static bool TriangleIntersectsTriangle(ref Triangle triangle1, ref Triangle triangle2, out Segment? segment)
        {
            return IntersectionTriangle.Intersection(ref triangle1, ref triangle2, out segment);
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
                if (TriangleIntersectsTriangle(ref triangle, ref mesh[i]))
                {
                    tris.Add(mesh[i]);
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
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the triangle intersects the mesh</returns>
        public static bool TriangleIntersectsMesh(ref Triangle triangle, Triangle[] mesh, out Triangle[] triangles, out Segment[] segments)
        {
            List<Triangle> tris = [];
            List<Segment> segs = [];

            bool intersected = false;
            for (int i = 0; i < mesh.Length; i++)
            {
                if (TriangleIntersectsTriangle(ref triangle, ref mesh[i], out var segment))
                {
                    tris.Add(mesh[i]);
                    if (segment.HasValue) segs.Add(segment.Value);

                    intersected = true;
                }
            }

            triangles = [.. tris.Distinct()];
            segments = [.. segs.FindAll(seg => seg.Point1 != seg.Point2).Distinct()];

            return intersected;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoint(ref Plane plane, ref Vector3 point)
        {
            Vector3.Dot(ref plane.Normal, ref point, out float distance);
            distance += plane.D;

            if (distance > 0f) return PlaneIntersectionType.Front;
            if (distance < 0f) return PlaneIntersectionType.Back;
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
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2)
        {
            Vector3.Cross(ref plane1.Normal, ref plane2.Normal, out Vector3 direction);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            Vector3.Dot(ref direction, ref direction, out float denominator);

            return !MathUtils.IsZero(denominator);
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

            Vector3.Cross(ref plane1.Normal, ref plane2.Normal, out Vector3 direction);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            Vector3.Dot(ref direction, ref direction, out float denominator);

            //We assume the planes are normalized, therefore the denominator
            //only serves as a parallel and coincident check. Otherwise we need
            //to divide the point by the denominator.
            if (MathUtils.IsZero(denominator))
            {
                line = new();
                return false;
            }

            Vector3 temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
            Vector3.Cross(ref temp, ref direction, out Vector3 point);

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
            max.X = (plane.Normal.X >= 0f) ? box.Minimum.X : box.Maximum.X;
            max.Y = (plane.Normal.Y >= 0f) ? box.Minimum.Y : box.Maximum.Y;
            max.Z = (plane.Normal.Z >= 0f) ? box.Minimum.Z : box.Maximum.Z;
            min.X = (plane.Normal.X >= 0f) ? box.Maximum.X : box.Minimum.X;
            min.Y = (plane.Normal.Y >= 0f) ? box.Maximum.Y : box.Minimum.Y;
            min.Z = (plane.Normal.Z >= 0f) ? box.Maximum.Z : box.Minimum.Z;

            Vector3.Dot(ref plane.Normal, ref max, out float distance);
            if (distance + plane.D > 0f) return PlaneIntersectionType.Front;

            Vector3.Dot(ref plane.Normal, ref min, out distance);
            if (distance + plane.D < 0f) return PlaneIntersectionType.Back;

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

            Vector3.Dot(ref plane.Normal, ref sphere.Center, out float distance);
            distance += plane.D;

            if (distance > sphere.Radius) return PlaneIntersectionType.Front;
            if (distance < -sphere.Radius) return PlaneIntersectionType.Back;
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
            Vector3.Cross(ref plane.Normal, ref cDir, out var dir);
            if (MathUtils.IsZero(dir.Length()))
            {
                // Perfect base contact. Test distance from center
                var h = cylinder.Height * 0.5f;
                Vector3.Dot(ref plane.Normal, ref cylinder.Center, out float distance);
                distance += plane.D - h;

                return distance <= 0f;
            }
            else
            {
                Vector3.Cross(ref dir, ref cDir, out var dc);
                dc.Normalize();
                dir = dc * cylinder.Radius;

                // Find the 4 points to test with the plane
                float h = cylinder.Height * 0.5f;
                cDir *= h;
                Vector3.Add(ref cylinder.Center, ref cDir, out Vector3 capPosition);
                Vector3.Subtract(ref cylinder.Center, ref cDir, out Vector3 basePosition);

                Vector3.Add(ref basePosition, ref dir, out Vector3 base1);
                Vector3.Dot(ref plane.Normal, ref base1, out float distance);
                if (distance + plane.D <= MathUtils.ZeroTolerance)
                {
                    return true;
                }

                Vector3.Add(ref capPosition, ref dir, out Vector3 cap1);
                Vector3.Dot(ref plane.Normal, ref cap1, out distance);
                if (distance + plane.D <= MathUtils.ZeroTolerance)
                {
                    return true;
                }

                Vector3.Subtract(ref basePosition, ref dir, out Vector3 base2);
                Vector3.Dot(ref plane.Normal, ref base2, out distance);
                if (distance + plane.D <= MathUtils.ZeroTolerance)
                {
                    return true;
                }

                Vector3.Subtract(ref capPosition, ref dir, out Vector3 cap2);
                Vector3.Dot(ref plane.Normal, ref cap2, out distance);
                if (distance + plane.D <= MathUtils.ZeroTolerance)
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
            Vector3.Cross(ref p1.Normal, ref p2.Normal, out var p1p2);
            Vector3.Cross(ref p2.Normal, ref p3.Normal, out var p2p3);
            Vector3.Cross(ref p3.Normal, ref p1.Normal, out var p3p1);

            Vector3.Dot(ref p1.Normal, ref p2p3, out var d1);
            Vector3.Dot(ref p2.Normal, ref p3p1, out var d2);
            Vector3.Dot(ref p3.Normal, ref p1p2, out var d3);

            result =
                -p1.D * p2p3 / d1
                - p2.D * p3p1 / d2
                - p3.D * p1p2 / d3;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Minimum.X > box2.Maximum.X || box2.Minimum.X > box1.Maximum.X) return false;

            if (box1.Minimum.Y > box2.Maximum.Y || box2.Minimum.Y > box1.Maximum.Y) return false;

            if (box1.Minimum.Z > box2.Maximum.Z || box2.Minimum.Z > box1.Maximum.Z) return false;

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

            Vector3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out Vector3 vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out float distance);

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
            if (!AxisTestX02(ref boxExtents, ref origTri, edge1.Z, edge1.Y, fez, fey)) return false;
            if (!AxisTestY02(ref boxExtents, ref origTri, edge1.Z, edge1.X, fez, fex)) return false;
            if (!AxisTestZ12(ref boxExtents, ref origTri, edge1.Y, edge1.X, fey, fex)) return false;

            // Test second 3 edges
            var edge2 = triangle.GetEdge2();
            fex = MathF.Abs(edge2.X);
            fey = MathF.Abs(edge2.Y);
            fez = MathF.Abs(edge2.Z);
            if (!AxisTestX02(ref boxExtents, ref origTri, edge2.Z, edge2.Y, fez, fey)) return false;
            if (!AxisTestY02(ref boxExtents, ref origTri, edge2.Z, edge2.X, fez, fex)) return false;
            if (!AxisTestZ01(ref boxExtents, ref origTri, edge2.Y, edge2.X, fey, fex)) return false;

            // Test third 3 edges
            var edge3 = triangle.GetEdge3();
            fex = MathF.Abs(edge3.X);
            fey = MathF.Abs(edge3.Y);
            fez = MathF.Abs(edge3.Z);
            if (!AxisTestX01(ref boxExtents, ref origTri, edge3.Z, edge3.Y, fez, fey)) return false;
            if (!AxisTestY01(ref boxExtents, ref origTri, edge3.Z, edge3.X, fez, fex)) return false;
            if (!AxisTestZ12(ref boxExtents, ref origTri, edge3.Y, edge3.X, fey, fex)) return false;

            // Test X direction
            MathUtils.MinMax(origTri.Point1.X, origTri.Point2.X, origTri.Point3.X, out float min, out float max);
            if (min > boxExtents.X || max < -boxExtents.X)
            {
                return false;
            }

            // Test Y direction
            MathUtils.MinMax(origTri.Point1.Y, origTri.Point2.Y, origTri.Point3.Y, out min, out max);
            if (min > boxExtents.Y || max < -boxExtents.Y)
            {
                return false;
            }

            // Test Z direction
            MathUtils.MinMax(origTri.Point1.Z, origTri.Point2.Z, origTri.Point3.Z, out min, out max);
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
            Vector3.Subtract(ref point, ref sphere.Center, out Vector3 v);
            float l2 = v.LengthSquared();
            float r2 = sphere.Radius * sphere.Radius;

            return l2 <= r2;
        }
        /// <summary>
        /// Determines whether a sphere intersects with a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <returns>Returns true if the sphere intersects the triangle</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Triangle triangle)
        {
            return SphereIntersectsTriangle(ref sphere, ref triangle.Point1, ref triangle.Point2, ref triangle.Point3);
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
            ClosestPointInTriangle(ref sphere.Center, ref triangle, out float distance, out var point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>(point, distance, triangle);
                return true;
            }

            result = PickingResult<Triangle>.Empty;
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
            Vector3.DistanceSquared(ref sphere1.Center, ref sphere2.Center, out float r2);
            return r2 <= radiisum * radiisum;
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
        /// Determines whether a sphere intersects with a triangle mesh
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="result">Returns the piking result</param>
        /// <returns>Returns true if the sphere intersects the triangle mesh</returns>
        public static bool SphereIntersectsMesh(ref BoundingSphere sphere, Triangle[] mesh, out PickingResult<Triangle> result)
        {
            ClosestPointInMesh(ref sphere.Center, mesh, out var triangle, out float distance, out var point);

            if (distance <= sphere.Radius)
            {
                result = new PickingResult<Triangle>(point, distance, triangle);
                return true;
            }

            result = PickingResult<Triangle>.Empty;
            return false;
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
                ClosestPointInTriangle(ref sphere.Center, ref mesh[i], out float dist, out var closestToTri);
                if (dist <= sphere.Radius)
                {
                    // Store intersection data
                    picks.Add(new PickingResult<Triangle>(closestToTri, dist, mesh[i]));
                }
            }

            results = [.. picks];
            return picks.Count != 0;
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
        public static bool FrustumIntersectsSegment(ref BoundingFrustum frustum, ref Vector3 p1, ref Vector3 p2, out float distance)
        {
            Vector3.Subtract(ref p2, ref p1, out var rDir);
            rDir.Normalize();

            if (!RayIntersectsFrustum(ref p1, ref rDir, ref frustum, out var inDistance, out _))
            {
                distance = float.MaxValue;
                return false;
            }

            if (!inDistance.HasValue || MathUtils.NearEqual(MathF.Abs(inDistance.Value), float.MaxValue))
            {
                distance = float.MaxValue;
                return false;
            }

            Vector3.Distance(ref p1, ref p2, out float rayLength);
            if (inDistance >= rayLength)
            {
                distance = float.MaxValue;
                return false;
            }

            distance = inDistance.Value;
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
            if (!FrustumIntersectsSegment(ref frustum, ref p1, ref p2, out float d))
            {
                distance = float.MaxValue;
                point = Vector3.Zero;
                return false;
            }

            Vector3.Subtract(ref p2, ref p1, out var rDir);

            distance = d;
            point = p1 + (Vector3.Normalize(rDir) * d);
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
            Vector3.Subtract(ref p2, ref p1, out var rDir);
            rDir.Normalize();

            if (!RayIntersectsFrustum(ref p1, ref rDir, ref frustum, out var inDistance, out var outDistance))
            {
                enteringPoint = null;
                exitingPoint = null;
                return false;
            }

            Vector3.Distance(ref p1, ref p2, out float rayLength);

            enteringPoint = null;
            exitingPoint = null;
            bool intoSegmentBounds = false;

            if (inDistance.HasValue && !MathUtils.NearEqual(MathF.Abs(inDistance.Value), float.MaxValue) && inDistance <= rayLength)
            {
                enteringPoint = p1 + (rDir * inDistance.Value);
                intoSegmentBounds = true;
            }

            if (outDistance.HasValue && !MathUtils.NearEqual(MathF.Abs(outDistance.Value), float.MaxValue) && outDistance <= rayLength)
            {
                exitingPoint = p1 + (rDir * outDistance.Value);
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
            return FrustumIntersectsSegment(ref frustum, ref segment.Point1, ref segment.Point2, out _);
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
            return FrustumIntersectsSegment(ref frustum, ref segment.Point1, ref segment.Point2, out distance, out point);
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
            return FrustumIntersectsSegment(ref frustum, ref segment.Point1, ref segment.Point2, out enteringPoint, out exitingPoint);
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
        /// Determines whether a mesh intersects with a mesh
        /// </summary>
        /// <param name="mesh1">Mesh one</param>
        /// <param name="mesh2">Mesh two</param>
        /// <param name="triangles">Returns the intersected triangles of the mesh two</param>
        /// <param name="segments">Returns the intersection segment list</param>
        /// <returns>Returns true if the mesh one intersects the mesh two</returns>
        public static bool MeshIntersectsMesh(Triangle[] mesh1, Triangle[] mesh2, out Triangle[] triangles, out Segment[] segments)
        {
            List<Triangle> tris = [];
            List<Segment> segs = [];

            for (int i = 0; i < mesh1.Length; i++)
            {
                if (TriangleIntersectsMesh(ref mesh1[i], mesh2, out var mTris, out var mSegs))
                {
                    tris.AddRange(mTris);
                    segs.AddRange(mSegs);
                }
            }

            triangles = [.. tris.Distinct()];
            segments = [.. segs.FindAll(seg => seg.Point1 != seg.Point2).Distinct()];

            return triangles.Length > 0;
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
            Vector3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out Vector3 vector);
            Vector3.DistanceSquared(ref sphere.Center, ref vector, out float distance);

            if (distance > sphere.Radius * sphere.Radius) return ContainmentType.Disjoint;

            if ((box.Minimum.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Maximum.X - sphere.Radius) && (box.Maximum.X - box.Minimum.X > sphere.Radius) &&
                (box.Minimum.Y + sphere.Radius <= sphere.Center.Y) && (sphere.Center.Y <= box.Maximum.Y - sphere.Radius) && (box.Maximum.Y - box.Minimum.Y > sphere.Radius) &&
                (box.Minimum.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Maximum.Z - sphere.Radius) && (box.Maximum.Z - box.Minimum.Z > sphere.Radius))
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
            var test1 = BoxContainsPoint(ref box, ref triangle.Point1);
            var test2 = BoxContainsPoint(ref box, ref triangle.Point2);
            var test3 = BoxContainsPoint(ref box, ref triangle.Point3);

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
        /// Determines whether a <see cref="BoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsPoint(ref BoundingSphere sphere, ref Vector3 point)
        {
            Vector3.DistanceSquared(ref point, ref sphere.Center, out float d2);

            if (d2 <= sphere.Radius * sphere.Radius) return ContainmentType.Contains;

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
            return SphereContainsTriangle(ref sphere, ref triangle.Point1, ref triangle.Point2, ref triangle.Point3);
        }
        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsBox(ref BoundingSphere sphere, ref BoundingBox box)
        {

            if (!BoxIntersectsSphere(ref box, ref sphere)) return ContainmentType.Disjoint;

            float radiussquared = sphere.Radius * sphere.Radius;

            Vector3 vector;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared) return ContainmentType.Intersects;

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
            Vector3.Distance(ref sphere1.Center, ref sphere2.Center, out float distance);

            if (sphere1.Radius + sphere2.Radius < distance) return ContainmentType.Disjoint;

            if (sphere1.Radius - sphere2.Radius < distance) return ContainmentType.Intersects;

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
                ref var p1 = ref corners2[i + 0];
                ref var p2 = ref corners2[i + 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2, out _))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test near plane segments
            for (int i = 0; i < 4; i++)
            {
                ref var p1 = ref corners2[i + 0];
                ref var p2 = ref corners2[(i + 1) % 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2, out _))
                {
                    return ContainmentType.Intersects;
                }
            }

            //Test far plane segments
            for (int i = 0; i < 4; i++)
            {
                ref var p1 = ref corners2[i + 4];
                ref var p2 = ref corners2[((i + 1) % 4) + 4];

                if (FrustumIntersectsSegment(ref frustum1, ref p1, ref p2, out _))
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
            var vertices = triangle.GetVertices();

            List<Vector3> inVerts = [];
            for (int i = 0; i < vertices.Length; i++)
            {
                if (FrustumContainsPoint(ref frustum, ref vertices[i]) != ContainmentType.Disjoint)
                {
                    inVerts.Add(vertices[i]);
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
        /// Gets whether the frustum contains the point list
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="points">Point collection</param>
        public static ContainmentType FrustumContainsPoints(ref BoundingFrustum frustum, Vector3[] points)
        {
            bool allCornersContained = true;
            bool intersects = false;
            for (int i = 0; i < points.Length; i++)
            {
                var res = FrustumContainsPoint(ref frustum, ref points[i]);
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

            if (MathUtils.NearEqual(distance, capsule.Radius))
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

            if (MathUtils.NearEqual(distance, cylinder.Radius))
            {
                // The point is into the cylinder radius, and between cap and base distances.
                return ContainmentType.Intersects;
            }

            if (MathUtils.NearEqual(distOnAxis, hh))
            {
                // The point is in the cap or base.
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }


        static Vector3[] GetVerticesFromMesh(Triangle[] mesh)
        {
            return [.. mesh.SelectMany(t => t.GetVertices()).Distinct()];
        }

        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingSphere.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="sphere">Sphere</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsSphere(Triangle[] mesh, ref BoundingSphere sphere)
        {
            var mbox = BoundingBox.FromPoints(GetVerticesFromMesh(mesh));

            return BoxContainsSphere(ref mbox, ref sphere);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingBox.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="box">Axis aligned box</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsBox(Triangle[] mesh, ref BoundingBox box)
        {
            var mbox = BoundingBox.FromPoints(GetVerticesFromMesh(mesh));

            return BoxContainsBox(ref mbox, ref box);
        }
        /// <summary>
        /// Determines whether a Triangle mesh contains a BoundingFrustum.
        /// </summary>
        /// <param name="mesh">Triangle mesh</param>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the type of containment the two objects have between them</returns>
        public static ContainmentType MeshContainsFrustum(Triangle[] mesh, ref BoundingFrustum frustum)
        {
            var mbox = BoundingBox.FromPoints(GetVerticesFromMesh(mesh));

            return BoxContainsFrustum(ref mbox, ref frustum);
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
            var bbox1 = BoundingBox.FromPoints(GetVerticesFromMesh(mesh1));
            var bbox2 = BoundingBox.FromPoints(GetVerticesFromMesh(mesh2));

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

        private static bool AxisTestX01(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p1 = a * tri.Point2.Y - b * tri.Point2.Z;
            MathUtils.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestX02(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.Y - b * tri.Point1.Z;
            float p2 = a * tri.Point3.Y - b * tri.Point3.Z;
            MathUtils.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.Y + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY01(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p1 = -a * tri.Point2.X + b * tri.Point2.Z;
            MathUtils.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestY02(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = -a * tri.Point1.X + b * tri.Point1.Z;
            float p2 = -a * tri.Point3.X + b * tri.Point3.Z;
            MathUtils.MinMax(p0, p2, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Z;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ01(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p0 = a * tri.Point1.X - b * tri.Point1.Y;
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            MathUtils.MinMax(p0, p1, out float min, out float max);

            float rad = fa * extents.X + fb * extents.Y;
            if (min > rad || max < -rad)
            {
                return false;
            }

            return true;
        }
        private static bool AxisTestZ12(ref Vector3 extents, ref Triangle tri, float a, float b, float fa, float fb)
        {
            float p1 = a * tri.Point2.X - b * tri.Point2.Y;
            float p2 = a * tri.Point3.X - b * tri.Point3.Y;
            MathUtils.MinMax(p1, p2, out float min, out float max);

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
