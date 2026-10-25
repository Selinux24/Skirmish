using Engine.Common;
using System.Numerics;

namespace Engine.BuiltIn.Components.Geometry
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
        public Vector4 Color { get; set; } = new(0f, 0f, 0f, 1f);

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
