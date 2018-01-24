using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sphere culling volume
    /// </summary>
    public class CullingVolumeAABB : ICullingVolume
    {
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingBox bbox;

        /// <summary>
        /// Gets the center of the sphere
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bbox">Axis aligned bounding box</param>
        public CullingVolumeAABB(BoundingBox bbox)
        {
            this.bbox = bbox;

            this.Position = bbox.GetCenter();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Minimum point</param>
        /// <param name="max">Maximum point</param>
        public CullingVolumeAABB(Vector3 min, Vector3 max)
        {
            this.bbox = new BoundingBox(min, max);

            this.Position = bbox.GetCenter();
        }

        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return frustum.Contains(this.bbox);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sph)
        {
            return this.bbox.Contains(ref sph);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return this.bbox.Contains(ref bbox);
        }
    }
}
