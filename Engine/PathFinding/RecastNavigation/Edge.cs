
namespace Engine.PathFinding.RecastNavigation
{
    public struct Edge
    {
        public int[] Vert { get; set; }
        public int[] PolyEdge { get; set; }
        public int[] Poly { get; set; }

        public override string ToString()
        {
            return string.Format("Vert {0}; PolyEdge {1}; Poly {2};",
                Vert?.Join(","),
                PolyEdge?.Join(","),
                Poly?.Join(","));
        }
    };
}
