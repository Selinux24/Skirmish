using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area
    /// </summary>
    public class GraphAreaBox : GraphArea, IGraphAreaBox
    {
        /// <inheritdoc/>
        public Vector3 BMin { get; set; }
        /// <inheritdoc/>
        public Vector3 BMax { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaBox() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaBox(Vector3 bMin, Vector3 bMax) : base()
        {
            BMin = bMin;
            BMax = bMax;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaBox(BoundingBox box) : base()
        {
            BMin = box.Minimum;
            BMax = box.Maximum;
        }

        /// <inheritdoc/>
        public override BoundingBox GetBounds()
        {
            return new(BMin, BMax);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => AreaType {AreaType}; BMin {BMin} BMax {BMax}";
        }
    }
}
