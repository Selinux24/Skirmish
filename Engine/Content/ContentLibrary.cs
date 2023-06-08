using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    /// <summary>
    /// Content library
    /// </summary>
    public class ContentLibrary
    {
        /// <summary>
        /// Content data list
        /// </summary>
        private readonly IEnumerable<ContentData> contentDataList;

        /// <summary>
        /// Reads the content library from disk
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="filename">File name</param>
        public static ContentLibrary ReadContentLibrary(string contentFolder, string filename)
        {
            return SerializationHelper.DeserializeFromFile<ContentLibrary>(Path.Combine(contentFolder, filename));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentLibrary(ContentData contentData)
        {
            if (contentData == null)
            {
                throw new ArgumentNullException(nameof(contentData));
            }

            contentDataList = new[] { contentData };
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentLibrary(IEnumerable<ContentData> contentDataList)
        {
            this.contentDataList = contentDataList ?? throw new ArgumentNullException(nameof(contentDataList));
        }

        /// <summary>
        /// Gets the first content data by name
        /// </summary>
        /// <param name="name">Name</param>
        public ContentData GetContentDataByName(string name)
        {
            return
                contentDataList.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)) ??
                contentDataList.Select(c => c.FilterMask(name)).FirstOrDefault();
        }
    }
}
