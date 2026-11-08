using System.Collections.Generic;

namespace Engine.Mathematics
{
    /// <summary>
    /// Frustum intersection volume
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="frustum">Camera view frustum</param>
    public struct IntersectionVolumeFrustum(BoundingFrustum frustum) : ICullingVolume
    {
        /// <summary>
        /// Internal volume
        /// </summary>
        private BoundingFrustum frustum = frustum;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; } = frustum.GetCameraParams().Position;
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; private set; } = frustum.GetCameraParams().ZFar;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewProj">Camera view * projection matrix</param>
        public IntersectionVolumeFrustum(Matrix4x4 viewProj) : this(new BoundingFrustum(viewProj))
        {

        }

        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Collision.FrustumContainsSphere(ref frustum, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Collision.FrustumContainsBox(ref frustum, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Collision.FrustumContainsFrustum(ref this.frustum, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Collision.FrustumContainsMesh(ref frustum, mesh);
        }

        /// <inheritdoc/>
        public static implicit operator BoundingFrustum(IntersectionVolumeFrustum value)
        {
            return value.frustum;
        }
        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeFrustum(BoundingFrustum value)
        {
            return new IntersectionVolumeFrustum(value);
        }
    }
}
