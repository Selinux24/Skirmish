using Engine.Common;
using Engine.Content;

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
        /// Mesh image data collection
        /// </summary>
        public (string Name, IImageContent Content)[] Textures { get; set; } = [];
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

        /// <summary>
        /// Reads the material data
        /// </summary>
        public IMeshMaterial ReadMaterial()
        {
            var textures = Textures ?? [];

            MeshImageDataCollection values = new();
            foreach (var texture in textures)
            {
                values.SetValue(texture.Name, MeshImageData.FromContent(texture.Content));
            }

            return Material?.CreateMeshMaterial(values);
        }
    }
}
