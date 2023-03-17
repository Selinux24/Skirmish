using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    using Engine.Physics.Colliders;
    using Engine.Physics.GJK;

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

            if (primitive1 is SphereCollider sphere1 && primitive2 is SphereCollider sphere2)
            {
                // Special case sphere-sphere
                return SphereAndSphere(sphere1, sphere2, data);
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
            // Distance from center to plane
            Vector3 normal = Vector3.TransformNormal(halfSpace.Normal, halfSpace.RigidBody.Transform);
            float centerToPlane = Vector3.Dot(normal, sphere.RigidBody.Position) + halfSpace.D;

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
            bool contact = false;
            Vector3 contactPoint = Vector3.Zero;
            float penetration = float.MaxValue;

            // Set plane as world origin
            var plane = new Plane(Vector3.Up, 0);
            var planeTrn = halfSpace.RigidBody.Transform;

            // Transform cylinder to its space using the rigid body matrix
            var cylTrn = cylinder.RigidBody.Transform;
            var cylDir = Vector3.Up;
            var cylCenter = Vector3.Zero;
            if (!cylTrn.IsIdentity)
            {
                cylDir = Vector3.TransformNormal(Vector3.Up, cylTrn);
                cylCenter = Vector3.TransformCoordinate(Vector3.Zero, cylTrn);
            }

            // Transform cylinder to plane space using the plane inverse matrix
                var invTrn = Matrix.Invert(planeTrn);
            if (!planeTrn.IsIdentity)
            {
                cylDir = Vector3.TransformNormal(cylDir, invTrn);
                cylCenter = Vector3.TransformCoordinate(cylCenter, invTrn);
            }

            float cylHeight = cylinder.CapHeight - cylinder.BaseHeight;
            float hh = cylHeight * 0.5f;

            // dir points towards the plane and -dir in the opposite direction
            var dir = Vector3.Cross(plane.Normal, cylDir);
            if (MathUtil.IsZero(dir.Length()))
            {
                // Perfect base contact. Test base and cap

                var bse = new Vector3(cylCenter.X, cylCenter.Y - hh, cylCenter.Z);
                float d = Vector3.Dot(plane.Normal, bse) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = bse;
                    penetration = d;
                }

                var cap = new Vector3(cylCenter.X, cylCenter.Y + hh, cylCenter.Z);
                d = Vector3.Dot(plane.Normal, cap) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = cap;
                    penetration = d;
                }
            }
            else
            {
                // Find the 4 points to test with the plane
                var capPosition = cylCenter + (cylDir * hh);
                var basePosition = cylCenter - (cylDir * hh);

                dir = Vector3.Normalize(Vector3.Cross(dir, cylDir)) * cylinder.Radius;
                var base1 = basePosition + dir;
                var base2 = basePosition - dir;
                var cap1 = capPosition + dir;
                var cap2 = capPosition - dir;

                float d = Vector3.Dot(plane.Normal, base1) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = base1;
                    penetration = d;
                }
                d = Vector3.Dot(plane.Normal, base2) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = base2;
                    penetration = d;
                }
                d = Vector3.Dot(plane.Normal, cap1) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = cap1;
                    penetration = d;
                }
                d = Vector3.Dot(plane.Normal, cap2) + plane.D;
                if (d <= 0 && d < penetration)
                {
                    contact = true;
                    contactPoint = cap2;
                    penetration = d;
                }
            }

            if (!contact)
            {
                return false;
            }

            var contactNormal = plane.Normal;

            if (!planeTrn.IsIdentity)
            {
                contactPoint = Vector3.TransformCoordinate(contactPoint, planeTrn);
                contactNormal = Vector3.TransformNormal(plane.Normal, planeTrn);
            }

            if (!cylTrn.IsIdentity)
            {
                var invCylTrn = Matrix.Invert(cylTrn);
                contactPoint = Vector3.TransformCoordinate(contactPoint, invCylTrn);
                contactNormal = Vector3.TransformNormal(contactNormal, invCylTrn);
            }

            data.AddContact(cylinder.RigidBody, halfSpace.RigidBody, contactPoint, contactNormal, -penetration);

            return true;
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

            Vector3 normal = Vector3.TransformNormal(halfSpace.Normal, halfSpace.RigidBody.Transform);

            bool intersectionExists = false;

            foreach (var point in points)
            {
                // Distance to plane
                float penetration = halfSpace.D + Vector3.Dot(point, normal);
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
    }
}
