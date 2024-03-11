
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Model description
    /// </summary>
    public class ModelDescription : BaseModelDescription
    {
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Transform names
        /// </summary>
        public string[] TransformNames { get; set; } = [];
        /// <summary>
        /// Transform dependences
        /// </summary>
        public int[] TransformDependences { get; set; } = [];
    }
}
