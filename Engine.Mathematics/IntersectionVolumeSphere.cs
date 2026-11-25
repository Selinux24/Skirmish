
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
        public BoundingSphere Sphere = sphere;
        /// <inheritdoc/>
        public Vector3 Position { get; } = sphere.Center;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Sphere radius</param>
        public IntersectionVolumeSphere(Vector3 center, float radius) : this(new BoundingSphere(center, radius))
        {

        }

        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.SphereContainsSphere(ref Sphere, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingBox bbox)
        {
            return Collision.SphereContainsBox(ref Sphere, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Contains(ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingFrustum frustum)
        {
            return Collision.SphereContainsFrustum(ref Sphere, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Contains(ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(Triangle[] mesh)
        {
            return Collision.SphereContainsMesh(ref Sphere, mesh);
        }

        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }

        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeSphere(BoundingSphere value)
        {
            return new IntersectionVolumeSphere(value);
        }
    }
}
