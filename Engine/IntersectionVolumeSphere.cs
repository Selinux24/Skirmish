using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sphere intersection volume
    /// </summary>
    public class IntersectionVolumeSphere : IIntersectionVolume
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
        /// <param name="sph">Sphere</param>
        public IntersectionVolumeSphere(BoundingSphere sph)
        {
            this.sphere = sph;

            this.Position = sphere.Center;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Sphere radius</param>
        public IntersectionVolumeSphere(Vector3 center, float radius)
        {
            this.sphere = new BoundingSphere(center, radius);

            this.Position = sphere.Center;
        }

        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingSphere sph)
        {
            return Intersection.SphereContainsSphere(this.sphere, sph);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sph">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingBox bbox)
        {
            return Intersection.SphereContainsBox(this.sphere, bbox);
        }
        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            return Intersection.SphereContainsFrustum(this.sphere, frustum);
        }
        /// <summary>
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
        public ContainmentType Contains(Triangle[] mesh)
        {
            return Intersection.SphereContainsMesh(this.sphere, mesh);
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
