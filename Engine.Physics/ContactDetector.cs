using Engine.Common;
using SharpDX;
using System.Linq;

namespace Engine.Physics
{
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

            if (primitive1 is CollisionTriangle triangle)
            {
                return TriangleAndPrimitive(triangle, primitive2, data);
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

            if (primitive2 is CollisionTriangle triangle2)
            {
                return BoxAndTriangle(box1, triangle2, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return BoxAndTriangleSoup(box1, soup2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return BoxAndHalfSpace(box1, plane2, data);
            }

            return false;
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
            var collider1 = new BoxCollider(one.Extents);
            collider1.Position = one.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(one.RigidBody.Rotation);

            var collider2 = new BoxCollider(two.Extents);
            collider2.Position = two.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(two.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(one.RigidBody, two.RigidBody, position, normal, penetration);

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
        /// Detect collision between box and triangle
        /// </summary>
        /// <param name="box">box</param>
        /// <param name="triangle">Triangle</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool BoxAndTriangle(CollisionBox box, CollisionTriangle triangle, ContactResolver data)
        {
            var collider1 = new BoxCollider(box.Extents);
            collider1.Position = box.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(box.RigidBody.Rotation);

            var collider2 = new TriangleCollider(triangle.GetTriangle());
            collider2.Position = triangle.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangle.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(box.RigidBody, triangle.RigidBody, position, normal, penetration);

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
            var collider1 = new BoxCollider(box.Extents);
            collider1.Position = box.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(box.RigidBody.Rotation);

            var collider2 = new PolytopeCollider(triangleSoup.GetVertices());
            collider2.Position = triangleSoup.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangleSoup.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(box.RigidBody, triangleSoup.RigidBody, position, normal, penetration);

            return true;
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
            var corners = box.OrientedBoundingBox.GetVertices();

            bool intersectionExists = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 vertexPos = corners.ElementAt(i);

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

            if (primitive2 is CollisionTriangle triangle2)
            {
                return SphereAndTriangle(sphere1, triangle2, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return SphereAndTriangleSoup(sphere1, soup2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return SphereAndHalfSpace(sphere1, plane2, data);
            }

            return false;
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
            var collider1 = new SphereCollider(sphere.Radius);
            collider1.Position = sphere.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(sphere.RigidBody.Rotation);

            var collider2 = new PolytopeCollider(triangleSoup.GetVertices());
            collider2.Position = triangleSoup.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangleSoup.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(sphere.RigidBody, triangleSoup.RigidBody, position, normal, penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between a sphere and a triangle
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="triangle">Triangle</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool SphereAndTriangle(CollisionSphere sphere, CollisionTriangle triangle, ContactResolver data)
        {
            var collider1 = new SphereCollider(sphere.Radius);
            collider1.Position = sphere.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(sphere.RigidBody.Rotation);

            var collider2 = new TriangleCollider(triangle.GetTriangle());
            collider2.Position = triangle.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangle.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(sphere.RigidBody, triangle.RigidBody, position, normal, penetration);

            return true;
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
            float centerToPlane = Vector3.Dot(plane.Normal, sphere.RigidBody.Position) + plane.D;

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
        /// Detect collision between a triangle and a primitive
        /// </summary>
        /// <param name="triangle1">Triangle</param>
        /// <param name="primitive2">Primitive</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleAndPrimitive(CollisionTriangle triangle1, ICollisionPrimitive primitive2, ContactResolver data)
        {
            if (primitive2 is CollisionBox box2)
            {
                return BoxAndTriangle(box2, triangle1, data);
            }

            if (primitive2 is CollisionSphere sphere2)
            {
                return SphereAndTriangle(sphere2, triangle1, data);
            }

            if (primitive2 is CollisionTriangle triangle2)
            {
                return TriangleAndTriangle(triangle1, triangle2, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return TriangleAndTriangleSoup(triangle1, soup2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return TriangleAndHalfSpace(triangle1, plane2, data);
            }

            return false;
        }
        /// <summary>
        /// Detect the collision between two triangles
        /// </summary>
        /// <param name="triangle1">First triangle</param>
        /// <param name="triangle2">Second triangle</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleAndTriangle(CollisionTriangle triangle1, CollisionTriangle triangle2, ContactResolver data)
        {
            var collider1 = new TriangleCollider(triangle1.GetTriangle());
            collider1.Position = triangle1.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(triangle1.RigidBody.Rotation);

            var collider2 = new TriangleCollider(triangle2.GetTriangle());
            collider2.Position = triangle2.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangle2.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(triangle1.RigidBody, triangle2.RigidBody, position, normal, penetration);

            return true;
        }
        /// <summary>
        /// Detect collision between a triangle and a triangle soup
        /// </summary>
        /// <param name="triangle1">Triangle</param>
        /// <param name="triangleSoup2">Triangle soup</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleAndTriangleSoup(CollisionTriangle triangle1, CollisionTriangleSoup triangleSoup2, ContactResolver data)
        {
            var collider1 = new TriangleCollider(triangle1.GetTriangle());
            collider1.Position = triangle1.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(triangle1.RigidBody.Rotation);

            var collider2 = new PolytopeCollider(triangleSoup2.GetVertices());
            collider2.Position = triangleSoup2.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangleSoup2.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(triangle1.RigidBody, triangleSoup2.RigidBody, position, normal, penetration);

            return true;
        }
        /// <summary>
        /// Detect the collision between a triangle and a plane
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="plane">Plane</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleAndHalfSpace(CollisionTriangle triangle, CollisionPlane plane, ContactResolver data)
        {
            var tris = triangle.GetVertices(true);

            bool intersectionExists = false;
            for (int i = 0; i < tris.Count(); i++)
            {
                Vector3 vertexPos = tris.ElementAt(i);

                // Distance to plane
                float vertexDistance = plane.D + Vector3.Dot(vertexPos, plane.Normal);
                if (vertexDistance > 0f)
                {
                    continue;
                }

                intersectionExists = true;

                // The point of contact is halfway between the vertex and the plane.
                // It is obtained by multiplying the direction by half the separation distance, and adding the position of the vertex.
                data.AddContact(triangle.RigidBody, plane.RigidBody, vertexPos, plane.Normal, -vertexDistance);

                if (!data.HasFreeContacts())
                {
                    break;
                }
            }

            return intersectionExists;
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

            if (primitive2 is CollisionTriangle triangle2)
            {
                return TriangleAndTriangleSoup(triangle2, triangleSoup1, data);
            }

            if (primitive2 is CollisionTriangleSoup soup2)
            {
                return TriangleSoupAndTriangleSoup(triangleSoup1, soup2, data);
            }

            if (primitive2 is CollisionPlane plane2)
            {
                return TriangleSoupAndHalfSpace(triangleSoup1, plane2, data);
            }

            return false;
        }
        /// <summary>
        /// Detect collision between triangle soups
        /// </summary>
        /// <param name="triangleSoup1">Triangle soup 1</param>
        /// <param name="triangleSoup2">Triangle soup 2</param>
        /// <param name="data">Collision data</param>
        /// <returns>Returns true if there has been a collision</returns>
        public static bool TriangleSoupAndTriangleSoup(CollisionTriangleSoup triangleSoup1, CollisionTriangleSoup triangleSoup2, ContactResolver data)
        {
            var collider1 = new PolytopeCollider(triangleSoup1.GetVertices());
            collider1.Position = triangleSoup1.RigidBody.Position;
            collider1.RotationScale = Matrix.RotationQuaternion(triangleSoup1.RigidBody.Rotation);

            var collider2 = new PolytopeCollider(triangleSoup2.GetVertices());
            collider2.Position = triangleSoup2.RigidBody.Position;
            collider2.RotationScale = Matrix.RotationQuaternion(triangleSoup2.RigidBody.Rotation);

            if (!Solver.GJK(collider1, collider2, true, out var position, out var normal, out var penetration))
            {
                return false;
            }

            data.AddContact(triangleSoup1.RigidBody, triangleSoup2.RigidBody, position, normal, penetration);

            return true;
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
            var tris = triangleSoup.GetVertices(true);

            bool intersectionExists = false;
            for (int i = 0; i < tris.Count(); i++)
            {
                Vector3 vertexPos = tris.ElementAt(i);

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
    }
}
