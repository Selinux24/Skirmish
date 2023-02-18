using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// BoundingSphere extensions
    /// </summary>
    public static class BoundingSphereExtensions
    {
        /// <summary>
        /// Gets a sphere transformed by the given matrix
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="transform">Transform</param>
        public static BoundingSphere SetTransform(this BoundingSphere sphere, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return sphere;
            }

            // Gets the new position
            var center = Vector3.TransformCoordinate(sphere.Center, transform);

            // Calculates the scale vector
            var scale = new Vector3(
                transform.Column1.Length(),
                transform.Column2.Length(),
                transform.Column3.Length());

            // Gets the new sphere radius, based on the maximum scale axis value
            float radius = sphere.Radius * Math.Max(Math.Max(scale.X, scale.Y), scale.Z);


            return new BoundingSphere(center, radius);
        }
    }
}