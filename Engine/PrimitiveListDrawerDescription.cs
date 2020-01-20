using SharpDX;

namespace Engine
{
    /// <summary>
    /// Primitive drawer description
    /// </summary>
    public class PrimitiveListDrawerDescription<T> : ModelDescription where T : IVertexList
    {
        /// <summary>
        /// Maximum triangle count
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Initial primitives
        /// </summary>
        public T[] Primitives { get; set; }
        /// <summary>
        /// Initial color
        /// </summary>
        public Color4 Color { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PrimitiveListDrawerDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
