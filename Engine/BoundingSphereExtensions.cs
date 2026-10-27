using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine
{
    public static class BoundingSphereExtensions
    {
        /// <summary>
        /// Constructs the minimum BoundingSphere that fully contains the given points.
        /// </summary>
        /// <param name="points">Point list</param>
        public static BoundingSphere FromPoints(IEnumerable<Vector3> points)
        {
            Vector3 center = Vector3.Zero;
            float radius = 0f;

            int count = points.Count();

            for (int i = 0; i < count; i++)
            {
                center = Vector3.Add(points.ElementAt(i), center);
            }
            center /= count;

            for (int j = 0; j < count; j++)
            {
                var d = Vector3.DistanceSquared(center, points.ElementAt(j));
                if (d > radius)
                {
                    radius = d;
                }
            }

            radius = MathF.Sqrt(radius);

            return new(center, radius);
        }
        /// <summary>
        /// Gets a sphere transformed by the given matrix
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="transform">Transform</param>
        public static BoundingSphere SetTransform(this BoundingSphere sphere, Matrix4x4 transform)
        {
            if (transform.IsIdentity)
            {
                return sphere;
            }

            // Try to extract scaling component
            if (!Matrix4x4.Decompose(transform, out var scale, out _, out _))
            {
                return sphere;
            }

            // Adjust the radius
            var radius = sphere.Radius * MathF.Max(MathF.Max(scale.X, scale.Y), scale.Z);

            // Gets the new position
            var center = Vector3.Transform(sphere.Center, transform);

            return new BoundingSphere(center, radius);
        }

        public static bool Intersects(this BoundingSphere sph, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            return Collision.SphereIntersectsTriangle(ref sph, ref vertex1, ref vertex2, ref vertex3);
        }
        public static bool Intersects(this BoundingSphere sph, ref Ray ray)
        {
            return Collision.RayIntersectsSphere(ref ray, ref sph, out float _);
        }
        public static bool Intersects(this BoundingSphere sph, ref Ray ray, out float distance)
        {
            return Collision.RayIntersectsSphere(ref ray, ref sph, out distance);
        }
        public static bool Intersects(this BoundingSphere sph, ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsSphere(ref ray, ref sph, out point);
        }
    }
}
