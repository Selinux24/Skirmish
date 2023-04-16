using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    using Engine.Physics.Colliders;
    using EPAFace = EPA.Face;
    using EPASolver = EPA.Solver;
    using GJKSolver = GJK.Solver;

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

            if (primitive1 is HalfSpaceCollider halfSpace1)
            {
                // Special case with half-spaces
                return HalfSpaceAndPrimitive(halfSpace1, primitive2, data);
            }

            if (GJKSolver.GJK(primitive1, primitive2, out var simplex))
            {
                var (face, dist) = EPASolver.EPA(simplex, primitive1, primitive2);
                var normal = face.Normal;
                var penetration = dist;
                var mtv = normal * penetration;
                var position = ComputeContactPoint(face, mtv);

                data.AddContact(primitive1.RigidBody, primitive2.RigidBody, position, normal, penetration);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Computes the contact point, penetration and normal
        /// </summary>
        /// <param name="face">EPA resulting face</param>
        /// <param name="mtv">Minimum translation vector</param>
        /// <returns>Returns the resulting contact point</returns>
        /// <remarks>
        /// Taken from Jacob Tyndall's lattice3d engine
        /// <see cref="https://bitbucket.org/Hacktank/lattice3d/src/adfb28ffe5b51dbd1a173cbd43c6e387f1b4c12d/Lattice3D/src/physics/contact_generator/GJKEPAGenerator.cpp?at=master"/>
        /// </remarks>
        private static Vector3 ComputeContactPoint(EPAFace face, Vector3 mtv)
        {
            // Calculates barycentric coordinates using minimum translation vector as reference point
            var bc = Triangle.CalculateBarycenter(face.A.Point, face.B.Point, face.C.Point, mtv);

            // Interpolate the barycentric coordinates using the simplex cached support points of the first collider in the collision
            return bc.X * face.A.Support1 + bc.Y * face.B.Support1 + bc.Z * face.C.Support1;
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
    }
}
