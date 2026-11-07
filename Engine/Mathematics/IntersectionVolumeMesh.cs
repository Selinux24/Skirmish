using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IEnumerable<Triangle> mesh;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mesh">Mesh</param>
        public IntersectionVolumeMesh(IEnumerable<Triangle> mesh)
        {
            this.mesh = mesh ?? throw new ArgumentNullException(nameof(mesh), "Must specify a mesh.");
            if (!mesh.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(mesh), "Must specify at least one triangle in the mesh.");
            }

            Vector3 center = Vector3.Zero;
            for (int i = 0; i < mesh.Count(); i++)
            {
                center += mesh.ElementAt(i).GetCenter();
            }

            Position = center / mesh.Count();
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
        public readonly ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Collision.MeshContainsMesh(this.mesh, [.. mesh]);
        }

        /// <inheritdoc/>
        public static implicit operator Triangle[](IntersectionVolumeMesh value)
        {
            return value.mesh?.ToArray();
        }
        /// <inheritdoc/>
        public static implicit operator IntersectionVolumeMesh(Triangle[] value)
        {
            return new IntersectionVolumeMesh(value);
        }
    }
}
