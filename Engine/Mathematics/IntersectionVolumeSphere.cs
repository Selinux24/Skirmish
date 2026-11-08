using System.Collections.Generic;

namespace Engine.Mathematics
{
    /// <summary>
    /// Sphere intersection volume
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="sphere">Sphere</param>
    public struct IntersectionVolumeSphere(BoundingSphere sphere) : ICullingVolume
    {
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere sphere = sphere;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; } = sphere.Center;

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
            return Collision.SphereContainsSphere(ref this.sphere, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Collision.SphereContainsBox(ref this.sphere, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Collision.SphereContainsFrustum(ref this.sphere, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Collision.SphereContainsMesh(ref sphere, mesh);
        }

        /// <inheritdoc/>
        public static implicit operator BoundingSphere(IntersectionVolumeSphere value)
        {
            return value.sphere;
        }
        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeSphere(BoundingSphere value)
        {
            return new IntersectionVolumeSphere(value);
        }
    }
}
