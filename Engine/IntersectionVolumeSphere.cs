using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sphere intersection volume
    /// </summary>
    public struct IntersectionVolumeSphere : IIntersectionVolume
    {
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private readonly BoundingSphere sphere;

        /// <summary>
        /// Gets the center of the sphere
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public IntersectionVolumeSphere(BoundingSphere sphere)
        {
            this.sphere = sphere;

            Position = sphere.Center;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Sphere radius</param>
        public IntersectionVolumeSphere(Vector3 center, float radius)
        {
            sphere = new BoundingSphere(center, radius);

            Position = sphere.Center;
        }

        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Intersection.SphereContainsSphere(this.sphere, sphere);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.SphereContainsBox(sphere, bbox);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.SphereContainsFrustum(sphere, frustum);
        }
        /// <summary>
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(IEnumerable<Triangle> mesh)
        {
            return Intersection.SphereContainsMesh(sphere, mesh);
        }

        /// <summary>
        /// Implicit conversion between BoundingSphere and IntersectionVolumeSphere
        /// </summary>
        public static implicit operator BoundingSphere(IntersectionVolumeSphere value)
        {
            return value.sphere;
        }
        /// <summary>
        /// Implicit conversion between IntersectionVolumeSphere and BoundingSphere
        /// </summary>
        public static implicit operator IntersectionVolumeSphere(BoundingSphere value)
        {
            return new IntersectionVolumeSphere(value);
        }
    }
}
