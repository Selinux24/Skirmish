
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Edge
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// Polygon
        /// </summary>
        public int[] Poly { get; set; }
        /// <summary>
        /// Vertices
        /// </summary>
        public int[] Vert { get; set; }
        /// <summary>
        /// Polygon edges
        /// </summary>
        public int[] PolyEdge { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Poly {Poly?.Join(",")}; Vert {Vert?.Join(",")}; PolyEdge {PolyEdge?.Join(",")};";
        }
    };
}
