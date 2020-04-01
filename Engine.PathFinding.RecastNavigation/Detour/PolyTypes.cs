
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Flags representing the type of a navigation mesh polygon.
    /// </summary>
    public enum PolyTypes
    {
        /// <summary>
        /// DT_POLYTYPE_GROUND. The polygon is a standard convex polygon that is part of the surface of the mesh.
        /// </summary>
        Ground = 0,
        /// <summary>
        /// DT_POLYTYPE_OFFMESH_CONNECTION. The polygon is an off-mesh connection consisting of two vertices.
        /// </summary>
        OffmeshConnection = 1,
    }
}
