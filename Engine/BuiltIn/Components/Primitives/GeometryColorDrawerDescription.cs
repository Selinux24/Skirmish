using Engine.Common;
using SharpDX;

namespace Engine.BuiltIn.Components.Primitives
{
    /// <summary>
    /// Geometry color drawer description
    /// </summary>
    public class GeometryColorDrawerDescription<T> : BaseModelDescription where T : IVertexList
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
        public GeometryColorDrawerDescription()
            : base()
        {
            DeferredEnabled = false;
            DepthEnabled = false;
        }
    }
}
