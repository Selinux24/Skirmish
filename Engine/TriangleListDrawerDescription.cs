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
        public int Count;
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
