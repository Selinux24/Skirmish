﻿using System;
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
        /// File not found string
        /// </summary>
        private const string FileNotFoundString = "File not found";

        private const string CompressedFileString = "Compressed files not implemented yet";

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
            public static List<string> ReadEntryNames(string file)
            {
                List<string> files = [];

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

                return entries.Find(e => e.Equals(entryName, StringComparison.OrdinalIgnoreCase));
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
            public static List<MemoryStream> GetFiles(string file, string pattern)
            {
                List<MemoryStream> res = [];

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
        /// <param name="throwException">Sets whether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        private static MemoryStream[] FindContentDirectory(string contentSource, string resourcePath, bool throwException)
        {
            var path = Path.Combine(contentSource, resourcePath);
            if (File.Exists(path))
            {
                return [path.CopyToMemory()];
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
                throw new FileNotFoundException(FileNotFoundString, path);
            }

            return [];
        }
        /// <summary>
        /// Finds content int zippped file
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <param name="throwException">Sets whether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        private static MemoryStream[] FindContentZip(string contentSource, string resourcePath, bool throwException)
        {
            if (ZipManager.Contains(contentSource, resourcePath))
            {
                return [ZipManager.GetFile(contentSource, resourcePath)];
            }

            var res = ZipManager.GetFiles(contentSource, Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
            if (res.Count != 0)
            {
                return [.. res];
            }

            if (throwException)
            {
                throw new FileNotFoundException(FileNotFoundString, resourcePath);
            }

            return [];
        }

        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <param name="throwException">Sets whether throw exception or not</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        public static IEnumerable<MemoryStream> FindContent(string contentSource, string resourcePath, bool throwException = true)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return [];
            }

            if (File.Exists(resourcePath))
            {
                return [resourcePath.CopyToMemory()];
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
                throw new DirectoryNotFoundException($"Content source {resourcePath} not exists");
            }

            return [];
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
                        throw new FileNotFoundException(FileNotFoundString, resourcePath);
                    }
                }
            }

            return [.. res];
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
                return [];
            }

            if (File.Exists(resourcePath))
            {
                return [resourcePath];
            }

            if (Directory.Exists(contentSource))
            {
                //Directory
                resourcePath = Path.Combine(contentSource, resourcePath);
                if (File.Exists(resourcePath))
                {
                    return [resourcePath];
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
                    throw new FileNotFoundException(FileNotFoundString, resourcePath);
                }
            }

            if (File.Exists(contentSource))
            {
                //Compressed file
                throw new NotImplementedException(CompressedFileString);
            }

            if (throwException)
            {
                throw new DirectoryNotFoundException($"Content source {resourcePath} not exists");
            }

            return [];
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
                        throw new FileNotFoundException(FileNotFoundString, resourcePath);
                    }
                }
            }

            return [.. res];
        }
    }
}
