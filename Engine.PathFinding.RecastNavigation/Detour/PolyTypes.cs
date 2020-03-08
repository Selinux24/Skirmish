
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Flags representing the type of a navigation mesh polygon.
    /// </summary>
    public enum PolyTypes
    {
        /// <summary>
        /// The polygon is a standard convex polygon that is part of the surface of the mesh.
        /// </summary>
        DT_POLYTYPE_GROUND = 0,
        /// <summary>
        /// The polygon is an off-mesh connection consisting of two vertices.
        /// </summary>
        DT_POLYTYPE_OFFMESH_CONNECTION = 1,
    }
}
