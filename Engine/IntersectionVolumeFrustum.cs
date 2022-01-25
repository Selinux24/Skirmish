using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Frustum intersection volume
    /// </summary>
    public struct IntersectionVolumeFrustum : IIntersectionVolume
    {
        /// <summary>
        /// Internal volume
        /// </summary>
        private readonly BoundingFrustum frustum;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frustum">Camera view frustum</param>
        public IntersectionVolumeFrustum(BoundingFrustum frustum)
        {
            this.frustum = frustum;

            Position = frustum.GetCameraParams().Position;
            Radius = frustum.GetCameraParams().ZFar;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewProj">Camera view * projection matrix</param>
        public IntersectionVolumeFrustum(Matrix viewProj)
        {
            frustum = new BoundingFrustum(viewProj);

            Position = frustum.GetCameraParams().Position;
            Radius = frustum.GetCameraParams().ZFar;
        }

        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.FrustumContainsSphere(frustum, sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.FrustumContainsBox(frustum, bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.FrustumContainsFrustum(this.frustum, frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
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
