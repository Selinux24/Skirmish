using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area
    /// </summary>
    public class GraphAreaPolygon : GraphArea, IGraphAreaPolygon
    {
        /// <summary>
        /// Maximum polygon vertices points
        /// </summary>
        public const int MaxPoints = 12;

        /// <inheritdoc/>
        public IEnumerable<Vector3> Vertices { get; set; }
        /// <inheritdoc/>
        public float MinHeight { get; set; }
        /// <inheritdoc/>
        public float MaxHeight { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaPolygon() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaPolygon(IEnumerable<Vector3> vertices, float minHeight, float maxHeight) : base()
        {
            Vertices = vertices;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => AreaType {AreaType}; MinHeight {MinHeight} MaxHeight {MaxHeight} -> {(Vertices != null ? string.Join(" ", Vertices) : "")}";
        }
    }
}
