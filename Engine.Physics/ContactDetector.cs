using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    using Engine.Common;

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
        public static bool BetweenObjects(ICollisionPrimitive primitive1, ICollisionPrimitive primitive2, ContactResolver data)
        {
            if (!data.HasFreeContacts())
            {
                return false;
            }

            if (primitive1 == null || primitive2 == null)
            {
                return false;
            }

            // The first primitive can never be a plane
            if (primitive1 is CollisionPlane)
            {
                (primitive1, primitive2) = (primitive2, primitive1);
            }
            if (primitive1 is CollisionPlane)
            {
                return false;
            }

            if (primitive1 is CollisionBox box1)
            {
                return BoxAndPrimitive(box1, primitive2, data);
            }

            if (primitive1 is CollisionSphere sphere1)
            {
                return SphereAndPrimitive(sphere1, primitive2, data);
            }

            if (primitive1 is CollisionTriangleSoup triangleSoup)
            {
                return TriangleSoupAndPrimitive(triangleSoup, primitive2, data);
            }

            return false;
        }

        /// <summary>
        /// Detect collision between a box and a primitive
        /// </summary>
        /// <param name="box1">Box</param>
        /// <param name="primitive2">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndPrimitive(CollisionBox box1, ICollisionPrimitive primitive2, ContactResolver data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndBox(box1, box2, data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return BoxAndSphere(box1, sphere2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return BoxAndHalfSpace(box1, plane2, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return BoxAndTriangleSoup(box1, soup2, data);
            }

            return false;
        }
        /// <summary>
        /// Detects the collision between box and plane
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="plane">Plane</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndHalfSpace(CollisionBox box, CollisionPlane plane, ContactResolver data)
        {
            var corners = box.OrientedBoundingBox.GetCorners();

            bool intersectionExists = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 vertexPos = corners[i];

                // Distance to plane
                float vertexDistance = plane.D + Vector3.Dot(vertexPos, plane.Normal);
                if (vertexDistance > 0f && !MathUtil.IsZero(vertexDistance))
                {
                    continue;
                }

                intersectionExists = true;

                // The point of contact is halfway between the vertex and the plane.
                // It is obtained by multiplying the direction by half the separation distance, and adding the position of the vertex.
                data.AddContact(box.RigidBody, plane.RigidBody, vertexPos, plane.Normal, -vertexDistance);

                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
        }
        /// <summary>
        /// Detect the collision between boxes
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndBox(CollisionBox one, CollisionBox two, ContactResolver data)
        {
            if (!DetectBestAxis(one, two, out var toCentre, out var pen, out var best, out var bestSingleAxis))
            {
                return false;
            }

            // We have collision, and we have the axis of collision with less penetration
            if (best < 3)
            {
                // There is a vertex of box two on a face of box one.
                FillPointFaceBoxBox(one, two, toCentre, best, pen, ref data);

                return true;
            }

            if (best < 6)
            {
                // There is a vertex of box one on a face of box two.

                // Swap bodies
                (one, two) = (two, one);
                FillPointFaceBoxBox(one, two, toCentre * -1f, best - 3, pen, ref data);

                return true;
            }

            // Edge-to-edge contact.
            FillEdgeEdgeBoxBox(one, two, toCentre, best - 6, bestSingleAxis, pen, ref data);

            return true;
        }
        /// <summary>
        /// Detect the collision between a box and a sphere
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="sphere">Sphere</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndSphere(CollisionBox box, CollisionSphere sphere, ContactResolver data)
        {
            // Get the point of the box closest to the center of the sphere
            Vector3 closestPoint = Intersection.ClosestPointInBox(sphere.RigidBody.Position, box.OrientedBoundingBox);

            float distance = Vector3.Distance(sphere.RigidBody.Position, closestPoint);
            if (distance > sphere.Radius)
            {
                return false;
            }

            var normal = Vector3.Normalize(box.RigidBody.Position - closestPoint);
            var penetration = sphere.Radius - distance;
            data.AddContact(box.RigidBody, sphere.RigidBody, closestPoint, normal, penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between a box and a collection of triangles
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="triangleSoup">Triangle soup</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndTriangleSoup(CollisionBox box, CollisionTriangleSoup triangleSoup, ContactResolver data)
        {
            if (triangleSoup?.Triangles?.Any() != true)
            {
                return false;
            }

            //Convert obb to aabb
            var obb = box.OrientedBoundingBox;
            var trn = obb.Transformation;
            var invTrn = Matrix.Invert(trn);
            var origBox = new BoundingBox(-obb.Extents, obb.Extents);

            List<(Vector3 position, Vector3 normal)> contacts = new List<(Vector3 position, Vector3 normal)>();

            foreach (var triangle in triangleSoup.Triangles)
            {
                var origTri = Triangle.Transform(triangle, invTrn);

                if (!Intersection.BoxIntersectsTriangle(origBox, origTri))
                {
                    continue;
                }

                var contactList = GenerateBoxAndTriangleContacts(origBox, origTri);
                foreach (var position in contactList)
                {
                    contacts.Add((position, triangle.Normal));
                }
            }

            if (!contacts.Any())
            {
                return false;
            }

            foreach (var contact in contacts.Distinct())
            {
                var contactPosition = Vector3.TransformCoordinate(contact.position, trn);

                var axis = contact.normal;
                float oneProject = obb.ProjectToVector(axis);
                float twoProject = Math.Abs(Vector3.Dot(contact.position, axis));
                float pen = oneProject - twoProject;

                data.AddContact(box.RigidBody, triangleSoup.RigidBody, contactPosition, contact.normal, -pen);
                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return true;
        }
        /// <summary>
        /// Detect collision between box and triangle
        /// </summary>
        /// <param name="box">box</param>
        /// <param name="tri">Triangle</param>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndTriangle(CollisionBox box, Triangle triangle, IRigidBody rigidBody, ContactResolver data)
        {
            //Convert obb to aabb
            var obb = box.OrientedBoundingBox;
            var trn = obb.Transformation;
            var invTrn = Matrix.Invert(trn);
            var origBox = new BoundingBox(-obb.Extents, obb.Extents);
            var origTri = Triangle.Transform(triangle, invTrn);

            //Detect contacts
            var contacts = GenerateBoxAndTriangleContacts(origBox, origTri);
            foreach (var contact in contacts)
            {
                var contactPosition = Vector3.TransformCoordinate(contact, trn);

                var axis = triangle.Normal;
                float oneProject = obb.ProjectToVector(axis);
                float twoProject = Math.Abs(Vector3.Dot(contact, axis));
                float pen = oneProject - twoProject;

                data.AddContact(box.RigidBody, rigidBody, contactPosition, triangle.Normal, pen);
                if (!data.HasFreeContacts())
                {
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Detect collision between a sphere and a primitive
        /// </summary>
        /// <param name="sphere1">Sphere</param>
        /// <param name="primitive2">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndPrimitive(CollisionSphere sphere1, ICollisionPrimitive primitive2, ContactResolver data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndSphere(box2, sphere1, data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return SphereAndSphere(sphere1, sphere2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return SphereAndHalfSpace(sphere1, plane2, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return SphereAndTriangleSoup(sphere1, soup2, data);
            }

            return false;
        }
        /// <summary>
        /// Detect the collision between a sphere and a plane
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="plane">Plane</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndHalfSpace(CollisionSphere sphere, CollisionPlane plane, ContactResolver data)
        {
            // Distance from center to plane
            float centerToPlane = Math.Abs(Vector3.Dot(plane.Normal, sphere.RigidBody.Position) + plane.D);

            // Obtain the penetration of the sphere in the plane.
            float penetration = centerToPlane - sphere.Radius;
            if (penetration > 0f && !MathUtil.IsZero(penetration))
            {
                return false;
            }

            // Create the contact. It has a normal in the direction of the plane.
            var position = sphere.RigidBody.Position - plane.Normal * centerToPlane;
            data.AddContact(sphere.RigidBody, plane.RigidBody, position, plane.Normal, -penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between two spheres
        /// </summary>
        /// <param name="one">First sphere</param>
        /// <param name="two">Second sphere</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndSphere(CollisionSphere one, CollisionSphere two, ContactResolver data)
        {
            // Find the vector between the objects
            Vector3 positionOne = one.RigidBody.Position;
            Vector3 positionTwo = two.RigidBody.Position;
            Vector3 midline = positionOne - positionTwo;
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
        /// Detect collision between a sphere and a triangle soup
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangleSoup">Triangle soup</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndTriangleSoup(CollisionSphere sphere, CollisionTriangleSoup triangleSoup, ContactResolver data)
        {
            if (triangleSoup?.Triangles?.Any() != true)
            {
                return false;
            }

            bool contact = false;

            foreach (var triangle in triangleSoup.Triangles)
            {
                if (SphereAndTriangle(sphere, triangle, out var closestPoint, out var penetration))
                {
                    // Create the contact.
                    data.AddContact(sphere.RigidBody, triangleSoup.RigidBody, closestPoint, triangle.Normal, penetration);

                    contact = true;
                }

                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return contact;
        }
        /// <summary>
        /// Detect the collision between a sphere and a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="tri">Triangle</param>
        /// <param name="closestPoint">Closest point</param>
        /// <param name="penetration">Penetration</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndTriangle(CollisionSphere sphere, Triangle tri, out Vector3 closestPoint, out float penetration)
        {
            closestPoint = Vector3.Zero;
            penetration = 0f;

            // Check if the sphere and triangle are separated in the X, Y and Z axis
            var triCenter = tri.Center;
            float triRadius = tri.GetRadius();
            float radius = sphere.Radius + triRadius;
            if (Math.Abs(sphere.RigidBody.Position.X - triCenter.X) > radius)
            {
                return false;
            }
            if (Math.Abs(sphere.RigidBody.Position.Y - triCenter.Y) > radius)
            {
                return false;
            }
            if (Math.Abs(sphere.RigidBody.Position.Z - triCenter.Z) > radius)
            {
                return false;
            }

            // Get the point of the triangle closest to the center of the sphere
            closestPoint = Intersection.ClosestPointInTriangle(sphere.RigidBody.Position, tri);

            // Obtain the distance of the obtained point to the center of the sphere
            float distance = Vector3.Distance(closestPoint, sphere.RigidBody.Position);
            if (distance > sphere.Radius)
            {
                return false;
            }

            penetration = sphere.Radius - distance;

            return true;
        }

        /// <summary>
        /// Detect collision between a triangle soup and a primitive
        /// </summary>
        /// <param name="triangleSoup1">Triangle soup</param>
        /// <param name="primitive2">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleSoupAndPrimitive(CollisionTriangleSoup triangleSoup1, ICollisionPrimitive primitive2, ContactResolver data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndTriangleSoup(box2, triangleSoup1, data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return SphereAndTriangleSoup(sphere2, triangleSoup1, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return TriangleSoupAndHalfSpace(triangleSoup1, plane2, data);
            }

            return false;
        }
        /// <summary>
        /// Detect the collision between a triangle soup and a plane
        /// </summary>
        /// <param name="triangleSoup">Triangle soup</param>
        /// <param name="plane">Plane</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleSoupAndHalfSpace(CollisionTriangleSoup triangleSoup, CollisionPlane plane, ContactResolver data)
        {
            var tris = triangleSoup.Vertices.ToArray();

            bool intersectionExists = false;
            for (int i = 0; i < tris.Length; i++)
            {
                Vector3 vertexPos = tris[i];

                // Distance to plane
                float vertexDistance = plane.D + Vector3.Dot(vertexPos, plane.Normal);
                if (vertexDistance > 0f)
                {
                    continue;
                }

                intersectionExists = true;

                // The point of contact is halfway between the vertex and the plane.
                // It is obtained by multiplying the direction by half the separation distance, and adding the position of the vertex.
                data.AddContact(triangleSoup.RigidBody, plane.RigidBody, vertexPos, plane.Normal, -vertexDistance);

                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
        }

        /// <summary>
        /// Detects the best collision axis in a box to box collision
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCentre">To centre position</param>
        /// <param name="pen">Penetration value</param>
        /// <param name="best">Best axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <returns>Returns true if best axis detected</returns>
        private static bool DetectBestAxis(CollisionBox one, CollisionBox two, out Vector3 toCentre, out float pen, out uint best, out uint bestSingleAxis)
        {
            toCentre = two.RigidBody.Position - one.RigidBody.Position;
            pen = float.MaxValue;
            best = uint.MaxValue;
            bestSingleAxis = uint.MaxValue;

            var oneTrn = one.RigidBody.Transform;
            var twoTrn = two.RigidBody.Transform;

            // Check each axis, storing penetration and the best axis
            if (!TryAxis(one, two, oneTrn.Left, toCentre, 0, ref pen, ref best)) return false;
            if (!TryAxis(one, two, oneTrn.Up, toCentre, 1, ref pen, ref best)) return false;
            if (!TryAxis(one, two, oneTrn.Backward, toCentre, 2, ref pen, ref best)) return false;

            if (!TryAxis(one, two, twoTrn.Left, toCentre, 3, ref pen, ref best)) return false;
            if (!TryAxis(one, two, twoTrn.Up, toCentre, 4, ref pen, ref best)) return false;
            if (!TryAxis(one, two, twoTrn.Backward, toCentre, 5, ref pen, ref best)) return false;

            // Store the best axis so far, in case of being in a parallel axis collision later.
            bestSingleAxis = best;

            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Left), toCentre, 6, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Up), toCentre, 7, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Left, twoTrn.Backward), toCentre, 8, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Left), toCentre, 9, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Up), toCentre, 10, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Up, twoTrn.Backward), toCentre, 11, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Left), toCentre, 12, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Up), toCentre, 13, ref pen, ref best)) return false;
            if (!TryAxis(one, two, Vector3.Cross(oneTrn.Backward, twoTrn.Backward), toCentre, 14, ref pen, ref best)) return false;

            // Making sure we have a result.
            if (best == uint.MaxValue)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether there is penetration between the projections of the boxes in the specified axis
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="axis">Axis</param>
        /// <param name="toCentre">Distance to center</param>
        /// <param name="index">Index</param>
        /// <param name="smallestPenetration">Smallest penetration</param>
        /// <param name="smallestCase">Smallest test case</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static bool TryAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCentre, uint index, ref float smallestPenetration, ref uint smallestCase)
        {
            if (axis.LengthSquared() < 0.0001)
            {
                return true;
            }

            axis.Normalize();

            float penetration = PenetrationOnAxis(one, two, axis, toCentre);
            if (penetration < 0)
            {
                return false;
            }

            if (penetration < smallestPenetration)
            {
                smallestPenetration = penetration;
                smallestCase = index;
            }

            return true;
        }
        /// <summary>
        /// Gets the penetration of the projections of the boxes in the specified axis
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="axis">Axis</param>
        /// <param name="toCentre">Distance to center</param>
        /// <returns>Returns true if there has been a penetration</returns>
        private static float PenetrationOnAxis(CollisionBox one, CollisionBox two, Vector3 axis, Vector3 toCentre)
        {
            // Project the extensions of each box onto the axis
            float oneProject = one.OrientedBoundingBox.ProjectToVector(axis);
            float twoProject = two.OrientedBoundingBox.ProjectToVector(axis);

            // Obtain the distance between centers of the boxes on the axis
            float distance = Math.Abs(Vector3.Dot(toCentre, axis));

            // Positive indicates overlap, negative separation
            return oneProject + twoProject - distance;
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is vertex-face contact
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCentre">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="pen">Minor penetration axis</param>
        /// <param name="data">Collision data</param>
        private static void FillPointFaceBoxBox(CollisionBox one, CollisionBox two, Vector3 toCentre, uint best, float pen, ref ContactResolver data)
        {
            // We know which is the axis of the collision, but we have to know which face we have to work with
            var normal = GetAxis(one.RigidBody.Transform, best);
            if (Vector3.Dot(normal, toCentre) > 0f)
            {
                normal *= -1f;
            }

            Vector3 vertex = two.Extents;
            if (Vector3.Dot(two.RigidBody.Transform.Left, normal) < 0f) vertex.X = -vertex.X;
            if (Vector3.Dot(two.RigidBody.Transform.Up, normal) < 0f) vertex.Y = -vertex.Y;
            if (Vector3.Dot(two.RigidBody.Transform.Backward, normal) < 0f) vertex.Z = -vertex.Z;

            var position = Vector3.TransformCoordinate(vertex, two.RigidBody.Transform);

            data.AddContact(one.RigidBody, two.RigidBody, position, normal, pen);
        }
        /// <summary>
        /// Fills the collision information between two boxes, once it is known that there is edge-edge contact
        /// </summary>
        /// <param name="one">First box</param>
        /// <param name="two">Second box</param>
        /// <param name="toCentre">Distance to center</param>
        /// <param name="best">Best penetration axis</param>
        /// <param name="bestSingleAxis">Best single axis</param>
        /// <param name="pen">Minor penetration axis</param>
        /// <param name="data">Collision data</param>
        private static void FillEdgeEdgeBoxBox(CollisionBox one, CollisionBox two, Vector3 toCentre, uint best, uint bestSingleAxis, float pen, ref ContactResolver data)
        {
            // Get the common axis.
            uint oneAxisIndex = best / 3;
            uint twoAxisIndex = best % 3;
            Vector3 oneAxis = GetAxis(one.RigidBody.Transform, oneAxisIndex);
            Vector3 twoAxis = GetAxis(two.RigidBody.Transform, twoAxisIndex);
            Vector3 axis = Vector3.Cross(oneAxis, twoAxis);
            axis.Normalize();

            // The axis should point from box one to box two.
            if (Vector3.Dot(axis, toCentre) > 0f)
            {
                axis *= -1.0f;
            }

            // We have the axes, but not the edges.

            // Each axis has 4 edges parallel to it, we have to find the 4 of each box.
            // We will look for the point in the center of the edge.
            // We know that its component on the collision axis is 0 and we determine which endpoint on each of the other axes is closest.
            Vector3 vOne = one.Extents;
            Vector3 vTwo = two.Extents;
            float[] ptOnOneEdge = new float[] { vOne.X, vOne.Y, vOne.Z };
            float[] ptOnTwoEdge = new float[] { vTwo.X, vTwo.Y, vTwo.Z };
            for (uint i = 0; i < 3; i++)
            {
                if (i == oneAxisIndex)
                {
                    ptOnOneEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(one.RigidBody.Transform, i), axis) > 0f)
                {
                    ptOnOneEdge[i] = -ptOnOneEdge[i];
                }

                if (i == twoAxisIndex)
                {
                    ptOnTwoEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(two.RigidBody.Transform, i), axis) < 0f)
                {
                    ptOnTwoEdge[i] = -ptOnTwoEdge[i];
                }
            }

            vOne.X = ptOnOneEdge[0];
            vOne.Y = ptOnOneEdge[1];
            vOne.Z = ptOnOneEdge[2];

            vTwo.X = ptOnTwoEdge[0];
            vTwo.Y = ptOnTwoEdge[1];
            vTwo.Z = ptOnTwoEdge[2];

            // Go to world coordinates
            vOne = Vector3.TransformCoordinate(vOne, one.RigidBody.Transform);
            vTwo = Vector3.TransformCoordinate(vTwo, two.RigidBody.Transform);

            // We have a point and a direction for the colliding edges.
            // We need to find the closest point of the two segments.
            float[] vOneAxis = new float[] { one.Extents.X, one.Extents.Y, one.Extents.Z };
            float[] vTwoAxis = new float[] { two.Extents.X, two.Extents.Y, two.Extents.Z };
            Vector3 vertex = ContactPoint(
                vOne, oneAxis, vOneAxis[oneAxisIndex],
                vTwo, twoAxis, vTwoAxis[twoAxisIndex],
                bestSingleAxis > 2);

            // Fill in the contact.
            data.AddContact(one.RigidBody, two.RigidBody, vertex, axis, pen);
        }
        /// <summary>
        /// Gets the closest point to the segments involved in an edge-to-edge or face-to-edge, or face-to-face collision
        /// </summary>
        /// <param name="pOne"></param>
        /// <param name="dOne"></param>
        /// <param name="oneSize"></param>
        /// <param name="pTwo"></param>
        /// <param name="dTwo"></param>
        /// <param name="twoSize"></param>
        /// <param name="useOne">If true, and the contact point is off edge (face-to-edge collision), only box one will be used, otherwise box two.</param>
        /// <returns>Returns the closest point to the two segments involved in an edge-to-edge collision</returns>
        private static Vector3 ContactPoint(Vector3 pOne, Vector3 dOne, float oneSize, Vector3 pTwo, Vector3 dTwo, float twoSize, bool useOne)
        {
            Vector3 toSt, cOne, cTwo;
            float dpStaOne, dpStaTwo, dpOneTwo, smOne, smTwo;
            float denom, mua, mub;

            smOne = dOne.LengthSquared();
            smTwo = dTwo.LengthSquared();
            dpOneTwo = Vector3.Dot(dTwo, dOne);

            toSt = pOne - pTwo;
            dpStaOne = Vector3.Dot(dOne, toSt);
            dpStaTwo = Vector3.Dot(dTwo, toSt);

            denom = smOne * smTwo - dpOneTwo * dpOneTwo;

            // Zero denominator indicates parallel lines
            if (Math.Abs(denom) < 0.0001f)
            {
                return useOne ? pOne : pTwo;
            }

            mua = (dpOneTwo * dpStaTwo - smTwo * dpStaOne) / denom;
            mub = (smOne * dpStaTwo - dpOneTwo * dpStaOne) / denom;

            // If any of the edges has the closest point out of bounds, the edges are not closed, and we have an edge-to-face collision..
            // The point is in the edge, which we know from the useOne parameter.
            if (mua > oneSize ||
                mua < -oneSize ||
                mub > twoSize ||
                mub < -twoSize)
            {
                return useOne ? pOne : pTwo;
            }
            else
            {
                cOne = pOne + dOne * mua;
                cTwo = pTwo + dTwo * mub;

                return cOne * 0.5f + cTwo * 0.5f;
            }
        }
        /// <summary>
        /// Gets the axis vector value from the specified trasnform
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <param name="axis">Axis value</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Vector3 GetAxis(Matrix transform, uint axis)
        {
            if (axis == 0) return transform.Left;

            if (axis == 1) return transform.Up;

            if (axis == 2) return transform.Backward;

            throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Axis value must be between 0 and 2");
        }
        /// <summary>
        /// Generates box-triangle contacts
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="tri">Triangle</param>
        private static IEnumerable<Vector3> GenerateBoxAndTriangleContacts(BoundingBox box, Triangle tri)
        {
            List<Vector3> res = new List<Vector3>();

            var triEdges = tri.GetEdges();
            foreach (var edge in triEdges)
            {
                if (Intersection.SegmentIntersectsBox(edge, box, out var contactPosition, out _))
                {
                    res.Add(contactPosition);
                }
            }

            var boxEdges = box.GetEdges();
            foreach (var edge in boxEdges)
            {
                if (Intersection.SegmentIntersectsTriangle(edge, tri, out var contactPosition, out _))
                {
                    res.Add(contactPosition);
                }
            }

            return res.Distinct().ToArray();
        }
    }
}