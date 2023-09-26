
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Polygon mesh detail indices
    /// </summary>
    public struct PolyMeshDetailIndices
    {
        /// <summary>
        /// Vertex base
        /// </summary>
        public int VertBase { get; set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// Triangle base
        /// </summary>
        public int TriBase { get; set; }
        /// <summary>
        /// Triangle count
        /// </summary>
        public int TriCount { get; set; }
    }
}
