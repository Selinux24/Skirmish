using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Bounding box intersection volume
    /// </summary>
    public struct IntersectionVolumeAxisAlignedBox : IIntersectionVolume
    {
        /// <summary>
        /// Bounding box
        /// </summary>
        private readonly BoundingBox bbox;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bbox">Axis aligned bounding box</param>
        public IntersectionVolumeAxisAlignedBox(BoundingBox bbox)
        {
            this.bbox = bbox;
            Position = bbox.GetCenter();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Minimum point</param>
        /// <param name="max">Maximum point</param>
        public IntersectionVolumeAxisAlignedBox(Vector3 min, Vector3 max)
        {
            bbox = new BoundingBox(min, max);
            Position = bbox.GetCenter();
        }

        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.BoxContainsSphere(bbox, sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.BoxContainsBox(this.bbox, bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.BoxContainsFrustum(bbox, frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Intersection.BoxContainsMesh(bbox, mesh);
        }

        /// <summary>
        /// Implicit conversion between BoundingBox and IntersectionVolumeAxisAlignedBox
        /// </summary>
        public static implicit operator BoundingBox(IntersectionVolumeAxisAlignedBox value)
        {
            return value.bbox;
        }
        /// <summary>
        /// Implicit conversion between IntersectionVolumeAxisAlignedBox and BoundingBox
        /// </summary>
        public static implicit operator IntersectionVolumeAxisAlignedBox(BoundingBox value)
        {
            return new IntersectionVolumeAxisAlignedBox(value);
        }
    }
}
