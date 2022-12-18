using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sphere intersection volume
    /// </summary>
    public struct IntersectionVolumeSphere : IIntersectionVolume
    {
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private readonly BoundingSphere sphere;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public IntersectionVolumeSphere(BoundingSphere sphere)
        {
            this.sphere = sphere;

            Position = sphere.Center;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Sphere radius</param>
        public IntersectionVolumeSphere(Vector3 center, float radius) : this(new BoundingSphere(center, radius))
        {

        }

        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.SphereContainsSphere(this.sphere, sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.SphereContainsBox(sphere, bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.SphereContainsFrustum(sphere, frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Intersection.SphereContainsMesh(sphere, mesh);
        }

        /// <summary>
        /// Implicit conversion between BoundingSphere and IntersectionVolumeSphere
        /// </summary>
        public static implicit operator BoundingSphere(IntersectionVolumeSphere value)
        {
            return value.sphere;
        }
        /// <summary>
        /// Implicit conversion between IntersectionVolumeSphere and BoundingSphere
        /// </summary>
        public static implicit operator IntersectionVolumeSphere(BoundingSphere value)
        {
            return new IntersectionVolumeSphere(value);
        }
    }
}
