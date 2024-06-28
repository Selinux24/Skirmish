using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Engine.Content
{
    using Engine.Common;
    using Engine.Content.Persistence;

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
        /// <returns>Returns a new content description</returns>
        public static ContentDescription FromFile(string contentFolder, string fileName)
        {
            return new ContentDescription
            {
                ContentFolder = contentFolder,
                ContentFilename = fileName,
            };
        }
        /// <summary>
        /// Creates a content description from a generated content data
        /// </summary>
        /// <param name="contentData">Content data</param>
        /// <remarks>Returns a new content description</remarks>
        public static ContentDescription FromContentData(ContentData contentData)
        {
            return new ContentDescription
            {
                ContentData = contentData,
            };
        }
        /// <summary>
        /// Creates a model descriptor from scratch
        /// </summary>
        /// <param name="vertices">Vertex data</param>
        /// <param name="indices">Index data</param>
        /// <param name="material">Material</param>
        /// <returns>Returns a new content description</returns>
        public static ContentDescription FromContentData(IEnumerable<VertexData> vertices, IEnumerable<uint> indices, IMaterialContent material = null)
        {
            var contentData = ContentData.GenerateTriangleList(vertices, indices, material);

            return new ContentDescription
            {
                ContentData = contentData,
            };
        }
        /// <summary>
        /// Creates a model descriptor from scratch
        /// </summary>
        /// <param name="vertices">Vertex data</param>
        /// <param name="indices">Index data</param>
        /// <param name="materials">Materials</param>
        /// <returns>Returns a new content description</returns>
        public static ContentDescription FromContentData(IEnumerable<VertexData> vertices, IEnumerable<uint> indices, IEnumerable<IMaterialContent> materials)
        {
            var contentData = ContentData.GenerateTriangleList(vertices, indices, materials);

            return new ContentDescription
            {
                ContentData = contentData,
            };
        }
        /// <summary>
        /// Creates a model descriptor from scratch
        /// </summary>
        /// <param name="geometry">Geometry descriptor</param>
        /// <param name="material">Material</param>
        /// <returns>Returns a new content description</returns>
        public static ContentDescription FromContentData(GeometryDescriptor geometry, IMaterialContent material = null)
        {
            var contentData = ContentData.GenerateTriangleList(geometry, material);

            return new ContentDescription
            {
                ContentData = contentData,
            };
        }
        /// <summary>
        /// Creates a model descriptor from scratch
        /// </summary>
        /// <param name="geometry">Geometry descriptor</param>
        /// <param name="materials">Material list</param>
        /// <returns>Returns a new content description</returns>
        public static ContentDescription FromContentData(GeometryDescriptor geometry, IEnumerable<IMaterialContent> materials)
        {
            var contentData = ContentData.GenerateTriangleList(geometry, materials);

            return new ContentDescription
            {
                ContentData = contentData,
            };
        }

        /// <summary>
        /// Content folder
        /// </summary>
        public string ContentFolder { get; set; }
        /// <summary>
        /// Content file name
        /// </summary>
        public string ContentFilename { get; set; }
        /// <summary>
        /// Content data
        /// </summary>
        public ContentData ContentData { get; set; }

        /// <summary>
        /// Reads the content data file
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ContentData>> ReadContentData()
        {
            if (!string.IsNullOrWhiteSpace(ContentFilename))
            {
                string directory = Path.Combine(ContentFolder ?? string.Empty, Path.GetDirectoryName(ContentFilename));
                string fileName = Path.GetFileName(ContentFilename);

                return await ContentDataFile.ReadContentData(directory, fileName);
            }
            else if (ContentData != null)
            {
                return await Task.FromResult(new[] { ContentData });
            }
            else
            {
                throw new EngineException("No geometry found in description.");
            }
        }
    }
}
