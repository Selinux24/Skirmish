
namespace AISamples.SceneCWRVirtualWorld
{
    struct FolderNavigatorPath
    {
        private const string PrevFolderString = "...";
        private const string FolderString = "/";

        public FolderNavigatorPathTypes PathType { get; set; }
        public string Path { get; set; }

        public readonly string GetFileName()
        {
            bool isPrevFolder = PathType == FolderNavigatorPathTypes.PrevFolder;
            bool isFolder = PathType == FolderNavigatorPathTypes.Folder;
            string path = Path;
            string fileName = System.IO.Path.GetFileName(path);

            return isPrevFolder ? PrevFolderString : isFolder ? $"{FolderString}{fileName}" : fileName;
        }

        public static bool FileNameIsPrevFolder(string fileName)
        {
            return fileName == PrevFolderString;
        }
        public static bool FileNameIsFolder(string fileName)
        {
            return fileName?.StartsWith(FolderString) ?? false;
        }
    }
}
