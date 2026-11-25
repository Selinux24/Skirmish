using System;

namespace Engine.Mathematics
{
    /// <summary>
    /// Mesh intersection volume
    /// </summary>
    public struct IntersectionVolumeMesh : ICullingVolume
    {
        /// <summary>
        /// Triangle list
        /// </summary>
        public readonly Triangle[] Mesh;
        /// <inheritdoc/>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh">Mesh</param>
        public IntersectionVolumeMesh(Triangle[] mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            if (mesh.Length == 0)
            {
                throw new ArgumentException("Must specify at least one triangle in the mesh.", nameof(mesh));
            }

            Mesh = mesh;

            Vector3 center = Vector3.Zero;
            for (int i = 0; i < Mesh.Length; i++)
            {
                center += Mesh[i].GetCenter();
            }

            Position = center / Mesh.Length;
        }

        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref BoundingBox bbox)
        {
            return Collision.MeshContainsBox(Mesh, ref bbox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingBox bbox)
        {
            return Contains(ref bbox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.MeshContainsSphere(Mesh, ref sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref BoundingFrustum frustum)
        {
            return Collision.MeshContainsFrustum(Mesh, ref frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingFrustum frustum)
        {
            return Contains(ref frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(Triangle[] mesh)
        {
            return Collision.MeshContainsMesh(Mesh, mesh);
        }

        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(IntersectionVolumeAxisAlignedBox bbox)
        {
            return Contains(ref bbox.BBox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(IntersectionVolumeSphere sphere)
        {
            return Contains(ref sphere.Sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(IntersectionVolumeFrustum frustum)
        {
            return Contains(ref frustum.Frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(ref IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(IntersectionVolumeMesh mesh)
        {
            return Contains(mesh.Mesh);
        }
    }
}
