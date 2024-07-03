using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

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
        private readonly BoundingFrustum frustum = frustum;

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
        public IntersectionVolumeFrustum(Matrix viewProj) : this(new BoundingFrustum(viewProj))
        {

        }

        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.FrustumContainsSphere(frustum, sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.FrustumContainsBox(frustum, bbox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.FrustumContainsFrustum(this.frustum, frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Intersection.FrustumContainsMesh(frustum, mesh);
        }

        /// <summary>
        /// Implicit conversion between BoundingFrustum and IntersectionVolumeFrustum
        /// </summary>
        public static implicit operator BoundingFrustum(IntersectionVolumeFrustum value)
        {
            return value.frustum;
        }
        /// <summary>
        /// Implicit conversion between IntersectionVolumeFrustum and BoundingFrustum
        /// </summary>
        public static implicit operator IntersectionVolumeFrustum(BoundingFrustum value)
        {
            return new IntersectionVolumeFrustum(value);
        }
    }
}
