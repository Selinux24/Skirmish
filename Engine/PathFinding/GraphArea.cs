using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area
    /// </summary>
    public class GraphArea : IGraphArea
    {
        /// <summary>
        /// Id counter
        /// </summary>
        private static int ID = 1;
        /// <summary>
        /// Gets the next id
        /// </summary>
        /// <returns>Returns the next id</returns>
        private static int GetNextId()
        {
            return ID++;
        }

        /// <summary>
        /// Maximum polygon vertices points
        /// </summary>
        public const int MaxPoints = 12;

        /// <summary>
        /// Area id
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Vertices (convex polygon)
        /// </summary>
        public Vector3[] Vertices { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        public float MinHeight { get; set; }
        /// <summary>
        /// Maximum height
        /// </summary>
        public float MaxHeight { get; set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertexCount { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public GraphAreaTypes AreaType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphArea()
        {
            Id = GetNextId();
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("AreaType {0}; MinHeight {1} MaxHeight {2} VertexCount {3} -> {4}",
                AreaType, MinHeight, MaxHeight, VertexCount,
                Vertices != null ? string.Join(" ", Vertices) : "");
        }
    }
}
