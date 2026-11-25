
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
        public BoundingFrustum Frustum = frustum;
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
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.FrustumContainsSphere(ref Frustum, ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingBox bbox)
        {
            return Collision.FrustumContainsBox(ref Frustum, ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Contains(ref bbox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref BoundingFrustum frustum)
        {
            return Collision.FrustumContainsFrustum(ref Frustum, ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Contains(ref frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(Triangle[] mesh)
        {
            return Collision.FrustumContainsMesh(ref Frustum, mesh);
        }

        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(ref IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }
        /// <inheritdoc/>
        public ContainmentType Contains(IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }

        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeFrustum(BoundingFrustum value)
        {
            return new IntersectionVolumeFrustum(value);
        }
    }
}
