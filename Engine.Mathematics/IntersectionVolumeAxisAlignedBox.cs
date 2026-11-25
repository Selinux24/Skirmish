
namespace Engine.Mathematics
{
    /// <summary>
    /// Bounding box intersection volume
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="bbox">Axis aligned bounding box</param>
    public struct IntersectionVolumeAxisAlignedBox(BoundingBox bbox) : ICullingVolume
    {
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BBox = bbox;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; } = bbox.Center;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Minimum point</param>
        /// <param name="max">Maximum point</param>
        public IntersectionVolumeAxisAlignedBox(Vector3 min, Vector3 max) : this(new BoundingBox(min, max))
        {

        }

        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.BoxContainsSphere(ref BBox, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingBox bbox)
        {
            return Collision.BoxContainsBox(ref BBox, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Contains(ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingFrustum frustum)
        {
            return Collision.BoxContainsFrustum(ref BBox, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Contains(ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(Triangle[] mesh)
        {
            return Collision.BoxContainsMesh(ref BBox, mesh);
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
        public static implicit operator IntersectionVolumeAxisAlignedBox(BoundingBox value)
        {
            return new IntersectionVolumeAxisAlignedBox(value);
        }
    }
}
