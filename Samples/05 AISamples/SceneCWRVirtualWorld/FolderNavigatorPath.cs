
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
