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
        private readonly Triangle[] mesh = [];

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

            this.mesh = mesh;

            Vector3 center = Vector3.Zero;
            for (int i = 0; i < this.mesh.Length; i++)
            {
                center += this.mesh[i].GetCenter();
            }

            Position = center / this.mesh.Length;
        }

        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingBox bbox)
        {
            return Collision.MeshContainsBox(mesh, ref bbox);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingSphere sphere)
        {
            return Collision.MeshContainsSphere(mesh, ref sphere);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(BoundingFrustum frustum)
        {
            return Collision.MeshContainsFrustum(mesh, ref frustum);
        }
        /// <inheritdoc/>
        public readonly ContainmentType Contains(Triangle[] mesh)
        {
            return Collision.MeshContainsMesh(this.mesh, mesh);
        }

        /// <inheritdoc/>
        public readonly Triangle[] GetTriangles()
        {
            return mesh;
        }
    }
}
