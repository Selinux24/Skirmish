﻿using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Bounding box intersection volume
    /// </summary>
    public class IntersectionVolumeAxisAlignedBox : IIntersectionVolume
    {
        /// <summary>
        /// Bounding box
        /// </summary>
        private readonly BoundingBox bbox;

        /// <summary>
        /// Gets the center of the box
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bbox">Axis aligned bounding box</param>
        public IntersectionVolumeAxisAlignedBox(BoundingBox bbox)
        {
            this.bbox = bbox;

            this.Position = bbox.GetCenter();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Minimum point</param>
        /// <param name="max">Maximum point</param>
        public IntersectionVolumeAxisAlignedBox(Vector3 min, Vector3 max)
        {
            this.bbox = new BoundingBox(min, max);

            this.Position = bbox.GetCenter();
        }

        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sph)
        {
            return Intersection.BoxContainsSphere(this.bbox, sph);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.BoxContainsBox(this.bbox, bbox);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.BoxContainsFrustum(this.bbox, frustum);
        }
        /// <summary>
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(Triangle[] mesh)
        {
            return Intersection.BoxContainsMesh(this.bbox, mesh);
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