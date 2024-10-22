
namespace Engine.Helpers
{
    /// <summary>
    /// Folder navigator path
    /// </summary>
    public struct FolderNavigatorPath
    {
        /// <summary>
        /// Folder separator string
        /// </summary>
        private const string FolderString = "/";

        /// <summary>
        /// Previous folder string
        /// </summary>
        public static string PrevFolderString { get; set; } = "...";

        /// <summary>
        /// Path type
        /// </summary>
        public FolderNavigatorPathTypes PathType { get; set; }
        /// <summary>
        /// Path string
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the formated file name based in the path type
        /// </summary>
        /// <returns></returns>
        public readonly string GetFileName()
        {
            if (PathType == FolderNavigatorPathTypes.PrevFolder)
            {
                return PrevFolderString;
            }

            string path = Path;
            string fileName = System.IO.Path.GetFileName(path);

            if (PathType == FolderNavigatorPathTypes.Folder)
            {
                return $"{FolderString}{fileName}";
            }

            return fileName;
        }

        /// <summary>
        /// File name is previous folder
        /// </summary>
        /// <param name="fileName">Filename</param>
        public static bool FileNameIsPrevFolder(string fileName)
        {
            return fileName == PrevFolderString;
        }
        /// <summary>
        /// File name is folder
        /// </summary>
        /// <param name="fileName">Filename</param>
        public static bool FileNameIsFolder(string fileName)
        {
            return fileName?.StartsWith(FolderString) ?? false;
        }
    }
}
