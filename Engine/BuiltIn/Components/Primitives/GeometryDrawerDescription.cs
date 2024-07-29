using Engine.Common;
using Engine.Content;
using SharpDX;

namespace Engine.BuiltIn.Components.Primitives
{
    /// <summary>
    /// Geometry drawer description
    /// </summary>
    public class GeometryDrawerDescription<T> : BaseModelDescription where T : IVertexData
    {
        /// <summary>
        /// Maximum triangle count
        /// </summary>
        public int Count { get; set; } = 1000;
        /// <summary>
        /// Initial vertices
        /// </summary>
        public T[] Vertices { get; set; }
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; } = Color4.White;
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; } = false;
        /// <summary>
        /// Image collection
        /// </summary>
        public (string Name, string ResourcePath)[] Images { get; set; } = [];
        /// <summary>
        /// Material content
        /// </summary>
        public IMaterialContent Material { get; set; } = MaterialBlinnPhongContent.Default;

        /// <summary>
        /// Constructor
        /// </summary>
        public GeometryDrawerDescription()
            : base()
        {
            DeferredEnabled = true;
            DepthEnabled = true;
        }
    }
}
