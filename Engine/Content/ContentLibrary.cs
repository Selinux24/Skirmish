using System;
using System.IO;

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
        private readonly ContentData[] contentDataList;

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
            ArgumentNullException.ThrowIfNull(contentData);

            contentDataList = [contentData];
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentLibrary(ContentData[] contentDataList)
        {
            this.contentDataList = contentDataList ?? throw new ArgumentNullException(nameof(contentDataList));
        }

        /// <summary>
        /// Gets the first content data by name
        /// </summary>
        /// <param name="name">Name</param>
        public ContentData GetContentDataByName(string name)
        {
            if (contentDataList == null || contentDataList.Length == 0) return null;

            foreach (var c in contentDataList)
            {
                if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)) return c;

                var filtered = c.FilterMask(name);
                if (filtered != null) return filtered;
            }

            return null;
        }
    }
}
