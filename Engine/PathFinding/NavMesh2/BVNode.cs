
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Bounding volume node.
    /// </summary>
    public struct BVNode
    {
        /// <summary>
        /// Minimum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Trianglei bmin;
        /// <summary>
        /// Maximum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Trianglei bmax;
        /// <summary>
        /// The node's index. (Negative for escape sequence.)
        /// </summary>
        public int i;
    };
}
