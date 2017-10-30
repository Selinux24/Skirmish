
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
        /// Transform dependences
        /// </summary>
        public int[] TransformDependences;

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelDescription()
            : base()
        {

        }
    }
}
