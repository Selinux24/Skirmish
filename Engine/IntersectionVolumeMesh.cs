using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Mesh intersection volume
    /// </summary>
    public struct IntersectionVolumeMesh : IIntersectionVolume
    {
        /// <summary>
        /// Triangle list
        /// </summary>
        private readonly IEnumerable<Triangle> mesh;

        /// <summary>
        /// Gets the center of the mesh
        /// </summary>
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
                center += mesh.ElementAt(i).Center;
            }

            Position = center / mesh.Count();
        }

        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.MeshContainsBox(mesh, bbox);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.MeshContainsSphere(mesh, sphere);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.MeshContainsFrustum(mesh, frustum);
        }
        /// <summary>
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Intersection.MeshContainsMesh(this.mesh, mesh);
        }

        /// <summary>
        /// Implicit conversion between Triangle[] and IntersectionVolumeMesh
        /// </summary>
        public static implicit operator Triangle[](IntersectionVolumeMesh value)
        {
            return value.mesh?.ToArray();
        }
        /// <summary>
        /// Implicit conversion between IntersectionVolumeMesh and Triangle[]
        /// </summary>
        public static implicit operator IntersectionVolumeMesh(Triangle[] value)
        {
            return new IntersectionVolumeMesh(value);
        }
    }
}
