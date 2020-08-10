using System.Collections.Generic;

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
        /// Creates a model description from a content description
        /// </summary>
        /// <param name="content">Content description</param>
        /// <returns>Returns a new mode description</returns>
        public static ModelDescription FromContent(ContentDescription content)
        {
            return new ModelDescription()
            {
                Content = content,
            };
        }

        public static ModelDescription FromData(IEnumerable<VertexData> vertices, IEnumerable<uint> indices, MaterialContent material = null)
        {
            var content = ModelContent.GenerateTriangleList(vertices, indices, material);

            return new ModelDescription
            {
                Content = new ContentDescription
                {
                    ModelContent = content,
                }
            };
        }

        public static ModelDescription FromData(GeometryDescriptor geometry, MaterialContent material = null)
        {
            var content = ModelContent.GenerateTriangleList(geometry, material);

            return new ModelDescription
            {
                Content = new ContentDescription
                {
                    ModelContent = content,
                }
            };
        }

        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Transform names
        /// </summary>
        public string[] TransformNames { get; set; }
        /// <summary>
        /// Transform dependences
        /// </summary>
        public int[] TransformDependences { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelDescription()
            : base()
        {

        }
    }
}
