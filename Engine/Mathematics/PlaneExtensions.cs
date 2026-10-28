using System.Numerics;
using Vortice.Mathematics;

namespace Engine.Mathematics
{
    public static class PlaneExtensions
    {
        public static Plane CreatePlane(Vector3 point, Vector3 normal)
        {
            float d = -Vector3.Dot(normal, point);

            return new Plane(normal, d);
        }

        public static PlaneIntersectionType Intersects(this Plane plane, ref Vector3 point)
        {
            return Collision.PlaneIntersectsPoint(ref plane, ref point);
        }
        public static bool Intersects(this Plane plane, ref Ray ray)
        {
            return Collision.RayIntersectsPlane(ref ray, ref plane, out float _);
        }
        public static bool Intersects(this Plane plane, ref Ray ray, out float distance)
        {
            return Collision.RayIntersectsPlane(ref ray, ref plane, out distance);
        }
        public static bool Intersects(this Plane plane, ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsPlane(ref ray, ref plane, out point);
        }
        public static bool Intersects(this Plane plane1, ref Plane plane)
        {
            return Collision.PlaneIntersectsPlane(ref plane1, ref plane);
        }
        public static bool Intersects(this Plane plane1, ref Plane plane, out Ray line)
        {
            return Collision.PlaneIntersectsPlane(ref plane1, ref plane, out line);
        }
        public static PlaneIntersectionType Intersects(this Plane plane, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            return Collision.PlaneIntersectsTriangle(ref plane, ref vertex1, ref vertex2, ref vertex3);
        }
        public static PlaneIntersectionType Intersects(this Plane plane, ref BoundingBox box)
        {
            return Collision.PlaneIntersectsBox(ref plane, ref box);
        }
        public static PlaneIntersectionType Intersects(this Plane plane, ref BoundingSphere sphere)
        {
            return Collision.PlaneIntersectsSphere(ref plane, ref sphere);
        }
    }
}
