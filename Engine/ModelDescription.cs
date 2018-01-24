
namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Model description
    /// </summary>
    public class ModelDescription : BaseModelDescription
    {
        /// <summary>
        /// Creates a model description from the specified xml file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <returns>Returns a new mode description</returns>
        public static ModelDescription FromXml(string name, string contentFolder, string fileName)
        {
            return new ModelDescription()
            {
                Name = name,
                Content = new ContentDescription()
                {
                    ContentFolder = contentFolder,
                    ModelContentFilename = fileName,
                }
            };
        }

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
