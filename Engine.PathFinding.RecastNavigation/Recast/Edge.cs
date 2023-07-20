
namespace Engine.PathFinding.RecastNavigation.Recast
{
    public struct Edge
    {
        public int[] Vert { get; set; }
        public int[] PolyEdge { get; set; }
        public int[] Poly { get; set; }

        public override readonly string ToString()
        {
            return $"Vert {Vert?.Join(",")}; PolyEdge {PolyEdge?.Join(",")}; Poly {Poly?.Join(",")};";
        }
    };
}
