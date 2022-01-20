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

        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.FrustumContainsSphere(frustum, sphere);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.FrustumContainsBox(frustum, bbox);
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
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
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
