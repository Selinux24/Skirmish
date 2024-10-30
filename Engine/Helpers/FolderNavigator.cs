using System.IO;
using System.Linq;

namespace Engine.Helpers
{
    /// <summary>
    /// Folder navigator
    /// </summary>
    public class FolderNavigator
    {
        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; set; } = 0;
        /// <summary>
        /// Items per page
        /// </summary>
        public int ItemsPerPage { get; set; } = 10;
        /// <summary>
        /// Total items
        /// </summary>
        public int TotalCount { get; set; } = 0;

        /// <summary>
        /// Gets the selected file
        /// </summary>
        public FolderNavigatorPath SelectedFile { get; private set; }
        /// <summary>
        /// Gets the selected folder
        /// </summary>
        public FolderNavigatorPath SelectedFolder { get; private set; }

        /// <summary>
        /// Loads the folder
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="searchPattern">Search pattern for the files in the folder</param>
        /// <param name="result">Returns de path list</param>
        /// <returns>Returns true if the folder exists</returns>
        public bool LoadFolder(string folder, string searchPattern, out FolderNavigatorPath[] result)
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

        /// <summary>
        /// Moves the page index up
        /// </summary>
        public bool PageUp()
        {
            if (PageIndex > 0)
            {
                PageIndex--;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Moves the page index down
        /// </summary>
        public bool PageDown()
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
