using SharpDX;

namespace Engine.PathFinding
{
    public interface IGraph
    {
        IGraphNode[] GetNodes();

        PathFindingPath FindPath(Vector3 from, Vector3 to);
    }
}
