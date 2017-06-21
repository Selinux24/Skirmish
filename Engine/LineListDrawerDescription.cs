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
        public int Count;
        /// <summary>
        /// Initial lines
        /// </summary>
        public Line3D[] Lines;
        /// <summary>
        /// Initial triangles
        /// </summary>
        public Triangle[] Triangles;
        /// <summary>
        /// Initial color
        /// </summary>
        public Color4 Color;

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
