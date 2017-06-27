
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Model description
    /// </summary>
    public class ModelDescription : ModelBaseDescription
    {
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex = 0;
        /// <summary>
        /// Transform names
        /// </summary>
        public string[] TransformNames;
        /// <summary>
        /// Transform dependeces
        /// </summary>
        public int[] TransformDependeces;

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelDescription()
            : base()
        {

        }
    }
}
