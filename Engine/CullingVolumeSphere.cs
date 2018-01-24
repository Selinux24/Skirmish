using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sphere culling volume
    /// </summary>
    public class CullingVolumeSphere : ICullingVolume
    {
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere sphere;

        /// <summary>
        /// Gets the center of the sphere
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sph">Sphere</param>
        public CullingVolumeSphere(BoundingSphere sph)
        {
            this.sphere = sph;

            this.Position = sphere.Center;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Sphere radius</param>
        public CullingVolumeSphere(Vector3 center, float radius)
        {
            this.sphere = new BoundingSphere(center, radius);

            this.Position = sphere.Center;
        }

        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return frustum.Contains(this.sphere);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sph)
        {
            return this.sphere.Contains(ref sph);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return this.sphere.Contains(ref bbox);
        }
    }
}
