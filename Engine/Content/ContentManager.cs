using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            public static string[] ReadEntryNames(string file)
            {
                List<string> files = new List<string>();

                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (var compressedFile in archive.Entries)
                    {
                        files.Add(compressedFile.Name);
                    }
                }

                return files.ToArray();
            }
            /// <summary>
            /// Gets an entry name, comparing names using ordinal ignore case
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="entryName">Entry name</param>
            /// <returns>Returns entry name if exists</returns>
            public static string GetEntryName(string file, string entryName)
            {
                string[] entries = ReadEntryNames(file);

                return Array.Find(entries, e => e.Equals(entryName, StringComparison.OrdinalIgnoreCase));
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
                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    ZipArchiveEntry entry = archive.GetEntry(GetEntryName(file, entryName));

                    using (var stream = entry.Open())
                    {
                        return stream.WriteToMemory();
                    }
                }
            }
            /// <summary>
            /// Gets file stream of the entry pattern
            /// </summary>
            /// <param name="file">Zip file name</param>
            /// <param name="pattern">Entry name</param>
            /// <returns>Returns file streams for the entry pattern</returns>
            public static MemoryStream[] GetFiles(string file, string pattern)
            {
                List<MemoryStream> res = new List<MemoryStream>();

                string regexMask = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");

                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        ZipArchiveEntry entry = archive.Entries[i];

                        Match match = Regex.Match(entry.Name, regexMask);
                        if (match.Success)
                        {
                            using (var stream = entry.Open())
                            {
                                res.Add(stream.WriteToMemory());
                            }
                        }
                    }
                }

                return res.ToArray();
            }
        }

        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source can be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        public static MemoryStream[] FindContent(string contentSource, string resourcePath, bool throwException = true)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return new MemoryStream[] { };
            }
            else if (File.Exists(resourcePath))
            {
                return new[] { resourcePath.WriteToMemory() };
            }
            else
            {
                if (Directory.Exists(contentSource))
                {
                    //Directory
                    resourcePath = Path.Combine(contentSource, resourcePath);
                    if (File.Exists(resourcePath))
                    {
                        return new[] { resourcePath.WriteToMemory() };
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(
                            contentSource,
                            Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                        if (files != null && files.Length > 0)
                        {
                            MemoryStream[] msList = new MemoryStream[files.Length];

                            for (int i = 0; i < files.Length; i++)
                            {
                                msList[i] = files[i].WriteToMemory();
                            }

                            return msList;
                        }
                        else if (throwException)
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
                else if (File.Exists(contentSource))
                {
                    //Compressed file
                    if (ZipManager.Contains(contentSource, resourcePath))
                    {
                        return new[] { ZipManager.GetFile(contentSource, resourcePath) };
                    }
                    else
                    {
                        MemoryStream[] res = ZipManager.GetFiles(contentSource, Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                        if (res != null && res.Length > 0)
                        {
                            return res;
                        }
                        else if (throwException)
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
                else if (throwException)
                {
                    throw new DirectoryNotFoundException(string.Format("El origen de contenido [{0}] no existe", resourcePath));
                }
            }

            return new MemoryStream[] { };
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
        public static MemoryStream[] FindContent(string contentSource, string[] resourcePaths, bool throwException = true)
        {
            List<MemoryStream> res = new List<MemoryStream>();

            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                for (int i = 0; i < resourcePaths.Length; i++)
                {
                    var resourceRes = FindContent(contentSource, resourcePaths[i], throwException);
                    if (resourceRes != null && resourceRes.Length > 0)
                    {
                        res.AddRange(resourceRes);
                    }
                    else if (throwException)
                    {
                        throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePaths[i]);
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
        public static string[] FindPaths(string contentSource, string resourcePath, bool throwException = true)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return new string[] { };
            }
            else if (File.Exists(resourcePath))
            {
                return new[] { resourcePath };
            }
            else
            {
                if (Directory.Exists(contentSource))
                {
                    //Directory
                    resourcePath = Path.Combine(contentSource, resourcePath);
                    if (File.Exists(resourcePath))
                    {
                        return new[] { resourcePath };
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(
                            contentSource,
                            Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                        if (files != null && files.Length > 0)
                        {
                            return files;
                        }
                        else if (throwException)
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
                else if (File.Exists(contentSource))
                {
                    //Compressed file
                    throw new NotImplementedException("Compressed files not implemented yet");
                }
                else if (throwException)
                {
                    throw new DirectoryNotFoundException(string.Format("El origen de contenido [{0}] no existe", resourcePath));
                }
            }

            return new string[] { };
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
        public static string[] FindPaths(string contentSource, string[] resourcePaths, bool throwException = true)
        {
            List<string> res = new List<string>();

            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                for (int i = 0; i < resourcePaths.Length; i++)
                {
                    var resourceRes = FindPaths(contentSource, resourcePaths[i], throwException);
                    if (resourceRes != null && resourceRes.Length > 0)
                    {
                        res.AddRange(resourceRes);
                    }
                    else if (throwException)
                    {
                        throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePaths[i]);
                    }
                }
            }

            return res.ToArray();
        }
    }
}
