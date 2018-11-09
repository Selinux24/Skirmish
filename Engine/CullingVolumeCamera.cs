using Engine.Common;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Culling volume for camera (Viewing frustum)
    /// </summary>
    public class CullingVolumeCamera : ICullingVolume
    {
        /// <summary>
        /// Internal volume
        /// </summary>
        private readonly BoundingFrustum frustum;

        /// <summary>
        /// Gets the view position
        /// </summary>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frustum">Camera view frustum</param>
        public CullingVolumeCamera(BoundingFrustum frustum)
        {
            this.frustum = frustum;

            this.Position = frustum.GetCameraParams().Position;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewProj">Camera view * projection matrix</param>
        public CullingVolumeCamera(Matrix viewProj)
        {
            this.frustum = new BoundingFrustum(viewProj);

            this.Position = frustum.GetCameraParams().Position;
            this.Radius = frustum.GetCameraParams().ZFar;
        }

        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.FrustumContainsFrustum(this.frustum, frustum);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sph)
        {
            return this.frustum.Contains(sph);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return this.frustum.Contains(bbox);
        }
    }
}
