using System.IO;

namespace Engine
{
    /// <summary>
    /// Images helper interface
    /// </summary>
    public interface IImages
    {
        /// <summary>
        /// Gets an image from a stream
        /// </summary>
        /// <param name="data">Stream data</param>
        Image FromStream(Stream data);
        /// <summary>
        /// Saves an image to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="image">Image data</param>
        void SaveToFile(string fileName, Image image);
    }
}
