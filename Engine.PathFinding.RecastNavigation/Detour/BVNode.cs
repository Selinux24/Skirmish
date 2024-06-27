using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Bounding volume node.
    /// </summary>
    [Serializable]
    public class BVNode
    {
        /// <summary>
        /// Minimum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMin { get; set; }
        /// <summary>
        /// Maximum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMax { get; set; }
        /// <summary>
        /// The node's index. (Negative for escape sequence.)
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BVNode()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BVNode)} Region Id: {I}; BMin: {BMin}; BMax: {BMax};";
        }
    }
}
