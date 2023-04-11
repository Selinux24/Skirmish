using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    using Engine.Common;
    using Engine.Physics.Colliders;
    using Engine.Physics.GJK;
    using System;

    /// <summary>
    /// Contact detector
    /// </summary>
    public static class ContactDetector
    {
        /// <summary>
        /// Detect collision between two primitives
        /// </summary>
        /// <param name="primitive1">First primitive</param>
        /// <param name="primitive2">Second primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BetweenObjects(ICollider primitive1, ICollider primitive2, ContactResolver data)
        {
            if (!data.HasFreeContacts())
            {
                return false;
            }

            if (primitive1 == null || primitive2 == null)
            {
                return false;
            }

            if (primitive2 is HalfSpaceCollider)
            {
                // Half spaces must be always the first component
                (primitive1, primitive2) = (primitive2, primitive1);
            }

            //if (primitive1 is SphereCollider sphere1)
            //{
            //    // Special case with spheres
            //    return SphereAndPrimitive(sphere1, primitive2, data);
            //}

            if (primitive1 is HalfSpaceCollider halfSpace1)
            {
                // Special case with half-spaces
                return HalfSpaceAndPrimitive(halfSpace1, primitive2, data);
            }

            if (Solver.GJK(primitive1, primitive2, true, out var position, out var normal, out var penetration))
            {
                data.AddContact(primitive1.RigidBody, primitive2.RigidBody, position, normal, penetration);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Detects the collision between primitive and half space
        /// </summary>
        /// <param name="halfSpace">Half space</param>
        /// <param name="primitive">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool HalfSpaceAndPrimitive(HalfSpaceCollider halfSpace, ICollider primitive, ContactResolver data)
        {
            if (primitive is HalfSpaceCollider)
            {
                // No collision detection between half-spaces 
                return false;
            }

            if (primitive is SphereCollider sphere)
            {
                return HalfSpaceAndSphere(halfSpace, sphere, data);
            }

            if (primitive is CapsuleCollider capsule)
            {
                return HalfSpaceAndCapsule(halfSpace, capsule, data);
            }

            if (primitive is CylinderCollider cylinder)
            {
                return HalfSpaceAndCylinder(halfSpace, cylinder, data);
            }

            if (primitive is BoxCollider box)
            {
                return HalfSpaceAndPointList(halfSpace, box.OrientedBoundingBox.GetVertices(), box.RigidBody, data);
            }

            if (primitive is TriangleCollider triangle)
            {
                return HalfSpaceAndPointList(halfSpace, triangle.GetVertices(true), triangle.RigidBody, data);
            }

            if (primitive is MeshCollider soup)
            {
                return HalfSpaceAndPointList(halfSpace, soup.GetVertices(true), soup.RigidBody, data);
            }

            return false;
        }
        /// <summary>
        /// Detects the collision between a sphere and a half space
        /// </summary>
        /// <param name="halfSpace">Half space</param>
        /// <param name="sphere">Sphere</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool HalfSpaceAndSphere(HalfSpaceCollider halfSpace, SphereCollider sphere, ContactResolver data)
        {
            var plane = halfSpace.GetPlane(true);
            Vector3 normal = plane.Normal;
            float d = plane.D;

            // Distance from center to plane
            float centerToPlane = Vector3.Dot(normal, sphere.RigidBody.Position) + d;

            // Obtain the penetration of the sphere in the plane.
            float penetration = centerToPlane - sphere.Radius;
            if (penetration > 0f && !MathUtil.IsZero(penetration))
            {
                return false;
            }

            // Create the contact. It has a normal in the direction of the plane.
            var position = sphere.RigidBody.Position - normal * centerToPlane;
            data.AddContact(sphere.RigidBody, halfSpace.RigidBody, position, normal, -penetration);

            return true;
        }
        /// <summary>
        /// Detects the collision between a cylinder and a half space
        /// </summary>
        /// <param name="halfSpace">Half space</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool HalfSpaceAndCylinder(HalfSpaceCollider halfSpace, CylinderCollider cylinder, ContactResolver data)
        {
            var plane = halfSpace.GetPlane(true);
            Vector3 normal = plane.Normal;
            float d = plane.D;

            bool intersectionExists = false;

            // Gets cylinder test points based on the plane normal projection
            var points = cylinder.GetProjectionPoints(normal, true);
            foreach (var point in points)
            {
                float penetration = d + Vector3.Dot(point, normal);
                if (penetration > 0f && !MathUtil.IsZero(penetration))
                {
                    continue;
                }

                intersectionExists = true;

                data.AddContact(cylinder.RigidBody, halfSpace.RigidBody, point, normal, -penetration);
                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
        }
        /// <summary>
        /// Detects the collision between a capsule and a half space
        /// </summary>
        /// <param name="halfSpace">Half space</param>
        /// <param name="capsule">Capsule</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool HalfSpaceAndCapsule(HalfSpaceCollider halfSpace, CapsuleCollider capsule, ContactResolver data)
        {
            var plane = halfSpace.GetPlane(true);
            Vector3 normal = plane.Normal;

            var points = capsule.GetPoints(true);

            bool intersectionExists = false;

            foreach (var point in points)
            {
                var p = point;
                float d = Collision.DistancePlanePoint(ref plane, ref p);
                if (d > capsule.Radius)
                {
                    continue;
                }

                intersectionExists = true;

                var contactPoint = p - (normal * capsule.Radius);
                float penetration = capsule.Radius - d;
                data.AddContact(capsule.RigidBody, halfSpace.RigidBody, contactPoint, normal, penetration);
                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
        }
        /// <summary>
        /// Detects the collision between a point list and a half pace
        /// </summary>
        /// <param name="halfSpace">Half space</param>
        /// <param name="points">Point list</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool HalfSpaceAndPointList(HalfSpaceCollider halfSpace, IEnumerable<Vector3> points, IRigidBody rigidBody, ContactResolver data)
        {
            if (!points.Any())
            {
                return false;
            }

            var plane = halfSpace.GetPlane(true);
            Vector3 normal = plane.Normal;
            float d = plane.D;

            bool intersectionExists = false;

            foreach (var point in points)
            {
                // Distance to plane
                float penetration = d + Vector3.Dot(point, normal);
                if (penetration > 0f && !MathUtil.IsZero(penetration))
                {
                    continue;
                }

                intersectionExists = true;

                data.AddContact(rigidBody, halfSpace.RigidBody, point, normal, -penetration);
                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
        }

        public static bool SphereAndPrimitive(SphereCollider sphere, ICollider primitive, ContactResolver data)
        {
            if (primitive is SphereCollider sphere1)
            {
                return SphereAndSphere(sphere, sphere1, data);
            }

            if (primitive is TriangleCollider triangle)
            {
                return SphereAndTriangle(sphere, triangle, data);
            }

            if (Solver.GJK(sphere, primitive, true, out var position, out var normal, out var penetration))
            {
                data.AddContact(sphere.RigidBody, primitive.RigidBody, position, normal, penetration);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Detects the collision between two spheres
        /// </summary>
        /// <param name="one">First sphere</param>
        /// <param name="two">Second sphere</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndSphere(SphereCollider one, SphereCollider two, ContactResolver data)
        {
            // Find the vector between the objects
            var positionOne = one.RigidBody.Position;
            var positionTwo = two.RigidBody.Position;
            var midline = positionOne - positionTwo;
            float size = midline.Length();

            // Check for errors
            if (size <= float.Epsilon || size >= one.Radius + two.Radius)
            {
                return false;
            }

            var normal = Vector3.Normalize(midline);
            var position = positionOne + midline * 0.5f;
            var penetration = one.Radius + two.Radius - size;
            data.AddContact(one.RigidBody, two.RigidBody, position, normal, penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between a sphere and a triangle
        /// </summary>
        /// <param name="one">Sphere</param>
        /// <param name="two">Triangle</param>
        /// <param name="closestPoint">Closest point</param>
        /// <param name="penetration">Penetration</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndTriangle(SphereCollider one, TriangleCollider two, ContactResolver data)
        {
            // Check if the sphere and triangle are separated in the X, Y and Z axis
            var tri = two.GetTriangle(true);
            var triCenter = tri.Center;
            float triRadius = tri.GetRadius();
            float radius = one.Radius + triRadius;
            if (Math.Abs(one.RigidBody.Position.X - triCenter.X) > radius)
            {
                return false;
            }
            if (Math.Abs(one.RigidBody.Position.Y - triCenter.Y) > radius)
            {
                return false;
            }
            if (Math.Abs(one.RigidBody.Position.Z - triCenter.Z) > radius)
            {
                return false;
            }

            // Get the point of the triangle closest to the center of the sphere
            var closestPoint = Intersection.ClosestPointInTriangle(one.RigidBody.Position, tri);

            // Obtain the distance of the obtained point to the center of the sphere
            float distance = Vector3.Distance(closestPoint, one.RigidBody.Position);
            if (distance > one.Radius)
            {
                return false;
            }

            float penetration = one.Radius - distance;

            data.AddContact(one.RigidBody, two.RigidBody, closestPoint, tri.Normal, penetration);

            return true;
        }
    }
}
