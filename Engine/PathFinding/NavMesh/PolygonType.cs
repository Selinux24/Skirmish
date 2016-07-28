using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Flags representing the type of a navmesh polygon.
    /// </summary>
    [Flags]
    public enum PolygonType : byte
    {
        /// <summary>
        /// A polygon that is part of the navmesh.
        /// </summary>
        Ground = 0,
        /// <summary>
        /// An off-mesh connection consisting of two vertices.
        /// </summary>
        OffMeshConnection = 1
    }
}
