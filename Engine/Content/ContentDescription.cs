
namespace Engine.Content
{
    /// <summary>
    /// Content description
    /// </summary>
    public class ContentDescription
    {
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
        /// Heightmap description
        /// </summary>
        public HeightmapDescription HeightmapDescription { get; set; }
    }
}
