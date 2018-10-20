using SharpDX;

namespace Engine
{
    /// <summary>
    /// Line drawer description
    /// </summary>
    public class LineListDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Maximum line count
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Initial lines
        /// </summary>
        public Line3D[] Lines { get; set; }
        /// <summary>
        /// Initial triangles
        /// </summary>
        public Triangle[] Triangles { get; set; }
        /// <summary>
        /// Initial color
        /// </summary>
        public Color4 Color { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LineListDrawerDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
