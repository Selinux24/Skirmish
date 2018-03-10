
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Flags representing the type of a navigation mesh polygon.
    /// </summary>
    public enum PolyTypes
    {
        /// <summary>
        /// The polygon is a standard convex polygon that is part of the surface of the mesh.
        /// </summary>
        Ground = 0,
        /// <summary>
        /// The polygon is an off-mesh connection consisting of two vertices.
        /// </summary>
        OffmeshConnection = 1,
    }
}
