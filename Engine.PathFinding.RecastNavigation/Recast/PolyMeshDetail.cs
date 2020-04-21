using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    public class PolyMeshDetail
    {
        /// <summary>
        /// The sub-mesh data.
        /// </summary>
        public List<PolyMeshDetailIndices> Meshes { get; set; } = new List<PolyMeshDetailIndices>();
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public List<Vector3> Verts { get; set; } = new List<Vector3>();
        /// <summary>
        /// The mesh triangles.
        /// </summary>
        public List<PolyMeshTriangleIndices> Tris { get; set; } = new List<PolyMeshTriangleIndices>();
    }
}
