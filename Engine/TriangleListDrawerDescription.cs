using SharpDX;

namespace Engine
{
    /// <summary>
    /// Triangle drawer description
    /// </summary>
    public class TriangleListDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Maximum triangle count
        /// </summary>
        public int Count { get; set; }
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
        public TriangleListDrawerDescription()
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
