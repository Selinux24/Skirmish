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
        public int Count { get; set; } = 1000;
        /// <summary>
        /// Initial primitives
        /// </summary>
        public T[] Primitives { get; set; }
        /// <summary>
        /// Initial color
        /// </summary>
        public Color4 Color { get; set; } = Color4.Black;

        /// <summary>
        /// Constructor
        /// </summary>
        public PrimitiveListDrawerDescription()
            : base()
        {
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
        }
    }
}
