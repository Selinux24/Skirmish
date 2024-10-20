using System.IO;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld
{
    /// <summary>
    /// Folder navigator
    /// </summary>
    static class FolderNavigator
    {
        public static int PageIndex { get; set; }
        public static int ItemsPerPage { get; set; }
        public static int TotalCount { get; set; }

        public static FolderNavigatorPath SelectedFile { get; private set; }
        public static FolderNavigatorPath SelectedFolder { get; private set; }

        public static bool LoadFolder(string folder, string searchPattern, out FolderNavigatorPath[] result)
        {
            if (!Directory.Exists(folder))
            {
                result = [];

                return false;
            }

            DirectoryInfo info = new(folder);

            SelectedFolder = new() { PathType = FolderNavigatorPathTypes.Folder, Path = info.FullName };
            SelectedFile = new() { PathType = FolderNavigatorPathTypes.None };

            EnumerationOptions o = new()
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
                IgnoreInaccessible = true,
                RecurseSubdirectories = false,
                ReturnSpecialDirectories = false,
            };

            var files = info.GetFiles(searchPattern, o)
                .Select(f => new FolderNavigatorPath() { PathType = FolderNavigatorPathTypes.File, Path = f.FullName });
            var folders = info.GetDirectories("*", o)
                .Select(f => new FolderNavigatorPath() { PathType = FolderNavigatorPathTypes.Folder, Path = f.FullName });
            FolderNavigatorPath[] paths = [.. folders, .. files];

            var parentFolder = info.Parent;
            if (parentFolder != null)
            {
                FolderNavigatorPath[] prevFolder = [new() { PathType = FolderNavigatorPathTypes.PrevFolder, Path = parentFolder.FullName }];
                paths = [.. prevFolder, .. paths];
            }

            TotalCount = paths.Length;
            if (PageIndex > 0 && PageIndex < TotalCount)
            {
                paths = paths.Skip(PageIndex).ToArray();
            }

            result = paths;

            return true;
        }

        public static bool PageUp()
        {
            if (PageIndex > 0)
            {
                PageIndex--;

                return true;
            }

            return false;
        }
        public static bool PageDown()
        {
            if (PageIndex < TotalCount - ItemsPerPage)
            {
                PageIndex++;

                return true;
            }

            return false;
        }
    }
}
