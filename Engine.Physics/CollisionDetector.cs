using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Collision detector
    /// </summary>
    static class CollisionDetector
    {
        /// <summary>
        /// Detect collision between two primitives
        /// </summary>
        /// <param name="primitive1">First primitive</param>
        /// <param name="primitive2">Second primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BetweenObjects(ICollisionPrimitive primitive1, ICollisionPrimitive primitive2, ref CollisionData data)
        {
            if (primitive1 == null || primitive2 == null)
            {
                return false;
            }

            if (primitive1 is CollisionBox box1)
            {
                return BoxAndPrimitive(box1, primitive2, ref data);
            }

            if (primitive1 is CollisionSphere sphere1)
            {
                return SphereAndPrimitive(sphere1, primitive2, ref data);
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
        private static bool BoxAndPrimitive(CollisionBox box1, ICollisionPrimitive primitive2, ref CollisionData data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndBox(box1, box2, ref data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return BoxAndSphere(box1, sphere2, ref data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return BoxAndHalfSpace(box1, plane2, ref data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return BoxAndTriangleSoup(box1, soup2, ref data);
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
        private static bool BoxAndHalfSpace(CollisionBox box, CollisionPlane plane, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            if (!Intersection.BoxIntersectsPlane(box.AABB, plane.Plane))
            {
                return false;
            }

            bool intersectionExists = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 vertexPos = box.GetCorner(i);

                // Distance to plane
                float vertexDistance = plane.D + Vector3.Dot(vertexPos, plane.Normal);
                if (vertexDistance > 0f)
                {
                    continue;
                }

                intersectionExists = true;

                // The point of contact is halfway between the vertex and the plane.
                // It is obtained by multiplying the direction by half the separation distance, and adding the position of the vertex.
                data.AddContact(box.RigidBody, null, vertexPos, plane.Normal, -vertexDistance);

                if (data.ContactsLeft <= 0)
                {
                    break;
                }
            }

            return intersectionExists;
        }
        /// <summary>
        /// Detect collision between box and triangle
        /// </summary>
        /// <param name="box">box</param>
        /// <param name="tri">Triangle</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool BoxAndTriangle(CollisionBox box, Triangle tri, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            bool intersectionExists = false;
            for (int i = 0; i < 8; i++)
            {
                var vertexPos = box.GetCorner(i);

                // Distance to plane
                float vertexDistance = tri.Plane.D + Vector3.Dot(vertexPos, tri.Plane.Normal);
                if (vertexDistance > 0f)
                {
                    continue;
                }

                // Intersection between line and triangle
                Vector3 direction = Vector3.Normalize(box.RigidBody.Position - vertexPos);
                Ray r = new Ray(vertexPos, direction);
                if (!Intersection.RayIntersectsTriangle(r, tri, out var contactPoint, out _))
                {
                    continue;
                }

                intersectionExists = true;

                // The point of contact is halfway between the vertex and the plane.
                // It is obtained by multiplying the direction by half the separation distance, and adding the position of the vertex.
                data.AddContact(box.RigidBody, null, contactPoint, tri.Normal, -vertexDistance);

                if (data.ContactsLeft <= 0)
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
        private static bool BoxAndBox(CollisionBox one, CollisionBox two, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

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
        private static bool BoxAndSphere(CollisionBox box, CollisionSphere sphere, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            // Get the point of the box closest to the center of the sphere
            Vector3 closestPoint = Intersection.ClosestPointInBox(sphere.RigidBody.Position, box.OBB);

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
        private static bool BoxAndTriangleSoup(CollisionBox box, CollisionTriangleSoup triangleSoup, ref CollisionData data)
        {
            return BoxAndTriangleList(box, triangleSoup.Triangles, ref data);
        }
        /// <summary>
        /// Detect collision between a box and a list of triangles
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="triangleList">Triangle list</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool BoxAndTriangleList(CollisionBox box, IEnumerable<Triangle> triangleList, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            bool intersection = false;

            foreach (Triangle triangle in triangleList)
            {
                if (BoxAndTriangle(box, triangle, ref data))
                {
                    intersection = true;
                }

                if (data.ContactsLeft <= 0)
                {
                    break;
                }
            }

            return intersection;
        }

        /// <summary>
        /// Detect collision between a sphere and a primitive
        /// </summary>
        /// <param name="sphere1">Sphere</param>
        /// <param name="primitive2">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool SphereAndPrimitive(CollisionSphere sphere1, ICollisionPrimitive primitive2, ref CollisionData data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndSphere(box2, sphere1, ref data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return SphereAndSphere(sphere1, sphere2, ref data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return SphereAndHalfSpace(sphere1, plane2, ref data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return SphereAndTriangleSoup(sphere1, soup2, ref data);
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
        private static bool SphereAndHalfSpace(CollisionSphere sphere, CollisionPlane plane, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            // Distance from center to plane
            float centerToPlane = Math.Abs(Vector3.Dot(plane.Normal, sphere.RigidBody.Position) + plane.D);

            // Obtain the penetration of the sphere in the plane.
            float penetration = centerToPlane - sphere.Radius;
            if (penetration >= 0)
            {
                return false;
            }

            // Create the contact. It has a normal in the direction of the plane.
            var position = sphere.RigidBody.Position - plane.Normal * centerToPlane;
            data.AddContact(sphere.RigidBody, null, position, plane.Normal, -penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between a sphere and a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="tri">Triangle</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool SphereAndTriangle(CollisionSphere sphere, Triangle tri, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            // Get the point of the triangle closest to the center of the sphere
            Vector3 closestPoint = Intersection.ClosestPointInTriangle(sphere.RigidBody.Position, tri);

            // Obtain the distance of the obtained point to the center of the sphere
            float distance = Vector3.Distance(closestPoint, sphere.RigidBody.Position);
            if (distance > sphere.Radius)
            {
                return false;
            }

            // Create the contact.
            data.AddContact(sphere.RigidBody, null, closestPoint, tri.Normal, sphere.Radius - distance);

            return true;
        }
        /// <summary>
        /// Detect the collision between two spheres
        /// </summary>
        /// <param name="one">First sphere</param>
        /// <param name="two">Second sphere</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool SphereAndSphere(CollisionSphere one, CollisionSphere two, ref CollisionData data)
        {
            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            // Find the vector between the objects
            Vector3 positionOne = one.RigidBody.Position;
            Vector3 positionTwo = two.RigidBody.Position;
            Vector3 midline = positionOne - positionTwo;
            float size = midline.Length();

            if (size <= 0.0f || size >= one.Radius + two.Radius)
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
        private static bool SphereAndTriangleSoup(CollisionSphere sphere, CollisionTriangleSoup triangleSoup, ref CollisionData data)
        {
            return SphereAndTriangleList(sphere, triangleSoup.Triangles, ref data);
        }
        /// <summary>
        /// Detect collision between a sphere and a list of triangles
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangleList">Triangle list</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        private static bool SphereAndTriangleList(CollisionSphere sphere, IEnumerable<Triangle> triangleList, ref CollisionData data)
        {
            if (triangleList?.Any() != true)
            {
                return false;
            }

            if (data.ContactsLeft <= 0)
            {
                return false;
            }

            bool contact = false;

            foreach (var triangle in triangleList)
            {
                if (data.ContactsLeft <= 0)
                {
                    break;
                }

                if (SphereAndTriangle(sphere, triangle, ref data))
                {
                    contact = true;
                }
            }

            return contact;
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

            var oneTrn = one.RigidBody.TransformMatrix;
            var twoTrn = two.RigidBody.TransformMatrix;

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
            float oneProject = Core.ProjectToVector(one.OBB, axis);
            float twoProject = Core.ProjectToVector(two.OBB, axis);

            // Obtain the distance between centers of the boxes on the axis
            float distance = Convert.ToSingle(Math.Abs(Vector3.Dot(toCentre, axis)));

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
        private static void FillPointFaceBoxBox(CollisionBox one, CollisionBox two, Vector3 toCentre, uint best, float pen, ref CollisionData data)
        {
            // We know which is the axis of the collision, but we have to know which face we have to work with
            var normal = GetAxis(one.RigidBody.TransformMatrix, best);
            if (Vector3.Dot(normal, toCentre) > 0f)
            {
                normal *= -1f;
            }

            Vector3 vertex = two.HalfSize;
            if (Vector3.Dot(two.RigidBody.TransformMatrix.Left, normal) < 0f) vertex.X = -vertex.X;
            if (Vector3.Dot(two.RigidBody.TransformMatrix.Up, normal) < 0f) vertex.Y = -vertex.Y;
            if (Vector3.Dot(two.RigidBody.TransformMatrix.Backward, normal) < 0f) vertex.Z = -vertex.Z;

            var position = Vector3.TransformCoordinate(vertex, two.RigidBody.TransformMatrix);

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
        private static void FillEdgeEdgeBoxBox(CollisionBox one, CollisionBox two, Vector3 toCentre, uint best, uint bestSingleAxis, float pen, ref CollisionData data)
        {
            // Get the common axis.
            uint oneAxisIndex = best / 3;
            uint twoAxisIndex = best % 3;
            Vector3 oneAxis = GetAxis(one.RigidBody.TransformMatrix, oneAxisIndex);
            Vector3 twoAxis = GetAxis(two.RigidBody.TransformMatrix, twoAxisIndex);
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
            Vector3 vOne = one.HalfSize;
            Vector3 vTwo = two.HalfSize;
            float[] ptOnOneEdge = new float[] { vOne.X, vOne.Y, vOne.Z };
            float[] ptOnTwoEdge = new float[] { vTwo.X, vTwo.Y, vTwo.Z };
            for (uint i = 0; i < 3; i++)
            {
                if (i == oneAxisIndex)
                {
                    ptOnOneEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(one.RigidBody.TransformMatrix, i), axis) > 0f)
                {
                    ptOnOneEdge[i] = -ptOnOneEdge[i];
                }

                if (i == twoAxisIndex)
                {
                    ptOnTwoEdge[i] = 0;
                }
                else if (Vector3.Dot(GetAxis(two.RigidBody.TransformMatrix, i), axis) < 0f)
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
            vOne = Vector3.TransformCoordinate(vOne, one.RigidBody.TransformMatrix);
            vTwo = Vector3.TransformCoordinate(vTwo, two.RigidBody.TransformMatrix);

            // We have a point and a direction for the colliding edges.
            // We need to find the closest point of the two segments.
            float[] vOneAxis = new float[] { one.HalfSize.X, one.HalfSize.Y, one.HalfSize.Z };
            float[] vTwoAxis = new float[] { two.HalfSize.X, two.HalfSize.Y, two.HalfSize.Z };
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
    }
}