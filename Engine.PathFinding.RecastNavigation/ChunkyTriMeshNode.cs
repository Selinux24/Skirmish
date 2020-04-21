using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Chunky TriMesh Node
    /// </summary>
    public class ChunkyTriMeshNode
    {
        /// <summary>
        /// First index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Triangle count
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Node Bounds
        /// </summary>
        public RectangleF Bounds { get; set; }
        /// <summary>
        /// Is leaf node
        /// </summary>
        public bool IsLeaf
        {
            get { return Index >= 0; }
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"Index {Index}; Count {Count}; Min {Bounds.TopLeft} Max {Bounds.BottomRight}";
        }
    }
}
