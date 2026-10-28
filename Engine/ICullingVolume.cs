using System.Collections.Generic;
using System.Numerics;

namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Culling volume interface
    /// </summary>
    public interface ICullingVolume
    {
        /// <summary>
        /// Gets the volume center position
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets if the current volume contains the bounding sphere
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <returns>Returns the containment type</returns>
        Vortice.Mathematics.ContainmentType Contains(BoundingSphere sphere);
        /// <summary>
        /// Gets if the current volume contains the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the containment type</returns>
        Vortice.Mathematics.ContainmentType Contains(BoundingBox bbox);
        /// <summary>
        /// Gets if the current volume contains the bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the containment type</returns>
        Vortice.Mathematics.ContainmentType Contains(BoundingFrustum frustum);
        /// <summary>
        /// Gets if the current volume contains the mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <returns>Returns the containment type</returns>
        Vortice.Mathematics.ContainmentType Contains(IEnumerable<Triangle> mesh);
    }
}
