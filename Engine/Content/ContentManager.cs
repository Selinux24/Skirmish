using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Engine.Content
{
    /// <summary>
    /// Content manager
    /// </summary>
    public static class ContentManager
    {
        /// <summary>
        /// Zip files manager
        /// </summary>
        static class ZipManager
        {
            /// <summary>
            /// Reads zip file entry names
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <returns>Returns zip file entry names array</returns>
            public static IEnumerable<string> ReadEntryNames(string file)
            {
                var files = new List<string>();

                using (var archive = ZipFile.OpenRead(file))
                {
                    foreach (var compressedFile in archive.Entries)
                    {
                        files.Add(compressedFile.Name);
                    }
                }

                return files;
            }
            /// <summary>
            /// Gets an entry name, comparing names using ordinal ignore case
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="entryName">Entry name</param>
            /// <returns>Returns entry name if exists</returns>
            public static string GetEntryName(string file, string entryName)
            {
                var entries = ReadEntryNames(file);

                return entries.FirstOrDefault(e => e.Equals(entryName, StringComparison.OrdinalIgnoreCase));
            }
            /// <summary>
            /// Gets if an entry name eixts into the zip file, comparing names using ordinal ignore case
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="entryName">Entry name</param>
            /// <returns>Returns true if the entry exists</returns>
            public static bool Contains(string file, string entryName)
            {
                string entry = GetEntryName(file, entryName);

                return !string.IsNullOrEmpty(entry);
            }
            /// <summary>
            /// Gets file stream of the entry
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="entryName">Entry name</param>
            /// <returns>Returns file stream of the entry</returns>
            public static MemoryStream GetFile(string file, string entryName)
            {
                using var archive = ZipFile.OpenRead(file);
                var entry = archive.GetEntry(GetEntryName(file, entryName));

                using var stream = entry.Open();
                return stream.CopyToMemory();
            }
            /// <summary>
            /// Gets file stream of the entry pattern
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="pattern">Entry name</param>
            /// <returns>Returns file streams for the entry pattern</returns>
            public static IEnumerable<MemoryStream> GetFiles(string file, string pattern)
            {
                var res = new List<MemoryStream>();

                string regexMask = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");

                using (var archive = ZipFile.OpenRead(file))
                {
                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        var entry = archive.Entries[i];

                        var match = Regex.Match(entry.Name, regexMask, RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100));
                        if (match.Success)
                        {
                            using var stream = entry.Open();
                            res.Add(stream.CopyToMemory());
                        }
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Finds content into directory
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <param name="throwException">Sets wether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        private static IEnumerable<MemoryStream> FindContentDirectory(string contentSource, string resourcePath, bool throwException)
        {
            var path = Path.Combine(contentSource, resourcePath);
            if (File.Exists(path))
            {
                return new[] { path.CopyToMemory() };
            }

            var files = Directory.GetFiles(
                contentSource,
                Path.GetFileNameWithoutExtension(path) + "*" + Path.GetExtension(path));
            if (files?.Length > 0)
            {
                MemoryStream[] msList = new MemoryStream[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    msList[i] = files[i].CopyToMemory();
                }

                return msList;
            }

            if (throwException)
            {
                throw new FileNotFoundException("File not found", path);
            }

            return Array.Empty<MemoryStream>();
        }
        /// <summary>
        /// Finds content int zippped file
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <param name="throwException">Sets wether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        private static IEnumerable<MemoryStream> FindContentZip(string contentSource, string resourcePath, bool throwException)
        {
            if (ZipManager.Contains(contentSource, resourcePath))
            {
                return new[] { ZipManager.GetFile(contentSource, resourcePath) };
            }

            var res = ZipManager.GetFiles(contentSource, Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
            if (res?.Any() == true)
            {
                return res.ToArray();
            }

            if (throwException)
            {
                throw new FileNotFoundException("File not found", resourcePath);
            }

            return Array.Empty<MemoryStream>();
        }

        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <param name="throwException">Sets wether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        public static IEnumerable<MemoryStream> FindContent(string contentSource, string resourcePath, bool throwException = true)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return Array.Empty<MemoryStream>();
            }

            if (File.Exists(resourcePath))
            {
                return new[] { resourcePath.CopyToMemory() };
            }

            if (Directory.Exists(contentSource))
            {
                //Directory
                return FindContentDirectory(contentSource, resourcePath, throwException);
            }

            if (File.Exists(contentSource))
            {
                //Compressed file
                return FindContentZip(contentSource, resourcePath, throwException);
            }

            if (throwException)
            {
                throw new DirectoryNotFoundException(string.Format("Content source [{0}] not exists", resourcePath));
            }

            return Array.Empty<MemoryStream>();
        }
        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePaths">Resource path list</param>
        /// <returns>Returns resource path list</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// </remarks>
        public static IEnumerable<MemoryStream> FindContent(string contentSource, IEnumerable<string> resourcePaths, bool throwException = true)
        {
            var res = new List<MemoryStream>();

            if (resourcePaths.Any())
            {
                foreach (var resourcePath in resourcePaths)
                {
                    var resourceRes = FindContent(contentSource, resourcePath, throwException);
                    if (resourceRes?.Any() == true)
                    {
                        res.AddRange(resourceRes);
                    }
                    else if (throwException)
                    {
                        throw new FileNotFoundException("File not found", resourcePath);
                    }
                }
            }

            return res.ToArray();
        }
        /// <summary>
        /// Finds paths
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        public static IEnumerable<string> FindPaths(string contentSource, string resourcePath, bool throwException = true)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return Array.Empty<string>();
            }

            if (File.Exists(resourcePath))
            {
                return new[] { resourcePath };
            }

            if (Directory.Exists(contentSource))
            {
                //Directory
                resourcePath = Path.Combine(contentSource, resourcePath);
                if (File.Exists(resourcePath))
                {
                    return new[] { resourcePath };
                }

                string[] files = Directory.GetFiles(
                    contentSource,
                    Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                if (files?.Length > 0)
                {
                    return files;
                }

                if (throwException)
                {
                    throw new FileNotFoundException("File not found", resourcePath);
                }
            }

            if (File.Exists(contentSource))
            {
                //Compressed file
                throw new NotImplementedException("Compressed files not implemented yet");
            }

            if (throwException)
            {
                throw new DirectoryNotFoundException(string.Format("Content source [{0}] not exists", resourcePath));
            }

            return Array.Empty<string>();
        }
        /// <summary>
        /// Finds paths
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePaths">Resource path list</param>
        /// <returns>Returns resource path list</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// </remarks>
        public static IEnumerable<string> FindPaths(string contentSource, IEnumerable<string> resourcePaths, bool throwException = true)
        {
            var res = new List<string>();

            if (resourcePaths.Any())
            {
                foreach (var resourcePath in resourcePaths)
                {
                    var resourceRes = FindPaths(contentSource, resourcePath, throwException);
                    if (resourceRes?.Any() == true)
                    {
                        res.AddRange(resourceRes);
                    }
                    else if (throwException)
                    {
                        throw new FileNotFoundException("File not found", resourcePath);
                    }
                }
            }

            return res.ToArray();
        }
    }
}
