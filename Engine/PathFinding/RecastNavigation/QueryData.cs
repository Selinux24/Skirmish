using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class QueryData
    {
        public Status status;
        public Node lastBestNode;
        public float lastBestNodeCost;
        public int startRef;
        public int endRef;
        public Vector3 startPos;
        public Vector3 endPos;
        public QueryFilter filter;
        public FindPathOptions options;
        public float raycastLimitSqr;
    }
}
