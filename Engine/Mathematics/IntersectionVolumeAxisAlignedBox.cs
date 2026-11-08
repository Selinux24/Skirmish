using System.Collections.Generic;

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
        private BoundingBox bbox = bbox;

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
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Collision.BoxContainsSphere(ref bbox, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Collision.BoxContainsBox(ref this.bbox, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Collision.BoxContainsFrustum(ref bbox, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Collision.BoxContainsMesh(ref bbox, mesh);
        }

        /// <inheritdoc/>
        public static implicit operator BoundingBox(IntersectionVolumeAxisAlignedBox value)
        {
            return value.bbox;
        }
        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeAxisAlignedBox(BoundingBox value)
        {
            return new IntersectionVolumeAxisAlignedBox(value);
        }
    }
}
