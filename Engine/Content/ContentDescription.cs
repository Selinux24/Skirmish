using System.IO;
using System.Linq;

namespace Engine.Content
{
    /// <summary>
    /// Content description
    /// </summary>
    public class ContentDescription
    {
        /// <summary>
        /// Creates a content description from a model content description file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        public static ContentDescription FromFile(string contentFolder, string fileName)
        {
            return new ContentDescription
            {
                ContentFolder = contentFolder,
                ModelContentFilename = fileName,
            };
        }
        /// <summary>
        /// Creates a content description from a generated model content
        /// </summary>
        /// <param name="content">Model content</param>
        public static ContentDescription FromModelContent(ModelContent content)
        {
            return new ContentDescription
            {
                ModelContent = content,
            };
        }
        /// <summary>
        /// Creates a content description from a model content description
        /// </summary>
        /// <param name="description">Model content description</param>
        public static ContentDescription FromModelContentDescription(ModelContentDescription description)
        {
            return new ContentDescription
            {
                ModelContentDescription = description,
            };
        }

        /// <summary>
        /// Content folder
        /// </summary>
        public string ContentFolder { get; set; }
        /// <summary>
        /// Model content file name
        /// </summary>
        public string ModelContentFilename { get; set; }
        /// <summary>
        /// Model content
        /// </summary>
        public ModelContent ModelContent { get; set; }
        /// <summary>
        /// Model content description
        /// </summary>
        public ModelContentDescription ModelContentDescription { get; set; }

        /// <summary>
        /// Reads the model content file
        /// </summary>
        /// <returns></returns>
        public ModelContent ReadModelContent()
        {
            if (!string.IsNullOrEmpty(ModelContentFilename))
            {
                string fileName = Path.GetFileName(ModelContentFilename);
                string directory = Path.Combine(ContentFolder ?? "", Path.GetDirectoryName(ModelContentFilename));

                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(directory, fileName));
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName);
                var t = loader.Load(directory, contentDesc);
                return t.First();
            }
            else if (ModelContentDescription != null)
            {
                var contentDesc = ModelContentDescription;
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName);
                var t = loader.Load(ContentFolder, contentDesc);
                return t.First();
            }
            else if (ModelContent != null)
            {
                return ModelContent;
            }
            else
            {
                throw new EngineException("No geometry found in description.");
            }
        }
    }
}
