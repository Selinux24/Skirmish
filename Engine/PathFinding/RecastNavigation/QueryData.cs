using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class QueryData
    {
        public Status Status { get; set; }
        public Node LastBestNode { get; set; }
        public float LastBestNodeCost { get; set; }
        public int StartRef { get; set; }
        public int EndRef { get; set; }
        public Vector3 StartPos { get; set; }
        public Vector3 EndPos { get; set; }
        public QueryFilter Filter { get; set; }
        public FindPathOptions Options { get; set; }
        public float RaycastLimitSqr { get; set; }
    }
}
