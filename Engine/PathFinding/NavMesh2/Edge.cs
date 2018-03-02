
namespace Engine.PathFinding.NavMesh2
{
    public struct Edge
    {
        public int[] vert;
        public int[] polyEdge;
        public int[] poly;

        public override string ToString()
        {
            return string.Format("Vert {0}; PolyEdge {1}; Poly {2};",
                vert?.Join(","),
                polyEdge?.Join(","),
                poly?.Join(","));
        }
    };
}
