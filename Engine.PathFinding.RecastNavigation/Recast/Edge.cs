
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Edge
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// Vertices
        /// </summary>
        public int[] Vert { get; set; }
        /// <summary>
        /// Polygon edgest
        /// </summary>
        public int[] PolyEdge { get; set; }
        /// <summary>
        /// Polygon
        /// </summary>
        public int[] Poly { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Vert {Vert?.Join(",")}; PolyEdge {PolyEdge?.Join(",")}; Poly {Poly?.Join(",")};";
        }
    };
}
