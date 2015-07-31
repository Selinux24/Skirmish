using System.Collections.Generic;
using System.IO;

namespace Engine.Content
{
    /// <summary>
    /// Content manager
    /// </summary>
    public static class ContentManager
    {
        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePath">Resource path</param>
        /// <returns>Returns resource paths found</returns>
        /// <remarks>
        /// Content source could be a folder or a zip file
        /// If not unique file found, searchs pattern "[filename]*[extension]" and returns result array
        /// </remarks>
        public static MemoryStream[] FindContent(string contentSource, string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }
            else if (File.Exists(resourcePath))
            {
                return new[] { ReadToMemory(resourcePath) };
            }
            else
            {
                if (Directory.Exists(contentSource))
                {
                    //Directory
                    resourcePath = Path.Combine(contentSource, resourcePath);
                    if (File.Exists(resourcePath))
                    {
                        return new[] { ReadToMemory(resourcePath) };
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(
                            contentSource,
                            Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                        if (files != null && files.Length > 0)
                        {
                            return ReadToMemory(files);
                        }
                        else
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
                else if (File.Exists(contentSource))
                {
                    //Compressed file
                    if (Compression.Contains(contentSource, resourcePath))
                    {
                        return new[] { Compression.GetFile(contentSource, resourcePath) };
                    }
                    else
                    {
                        MemoryStream[] res = Compression.GetFiles(contentSource, Path.GetFileNameWithoutExtension(resourcePath) + "*" + Path.GetExtension(resourcePath));
                        if (res != null && res.Length > 0)
                        {
                            return res;
                        }
                        else
                        {
                            throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePath);
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format("El origen de contenido [{0}] no existe", resourcePath));
                }
            }
        }
        /// <summary>
        /// Finds content
        /// </summary>
        /// <param name="contentSource">Content source</param>
        /// <param name="resourcePaths">Resource path list</param>
        /// <returns>Returns resource path list</returns>
        /// <remarks>
        /// Content source could be a folder or a zip file
        /// </remarks>
        public static MemoryStream[] FindContent(string contentSource, string[] resourcePaths)
        {
            List<MemoryStream> res = new List<MemoryStream>();

            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                for (int i = 0; i < resourcePaths.Length; i++)
                {
                    var resourceRes = FindContent(contentSource, resourcePaths[i]);
                    if (resourceRes != null && resourceRes.Length > 0)
                    {
                        res.AddRange(resourceRes);
                    }
                    else
                    {
                        throw new FileNotFoundException("El fichero especificado no se encuentra en la ruta de contenidos", resourcePaths[i]);
                    }
                }
            }

            return res.ToArray();
        }

        private static MemoryStream ReadToMemory(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                MemoryStream ms = new MemoryStream();

                stream.CopyTo(ms);

                ms.Position = 0;

                return ms;
            }
        }

        private static MemoryStream[] ReadToMemory(string[] files)
        {
            MemoryStream[] msList = new MemoryStream[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                msList[i] = ReadToMemory(files[i]);
            }

            return msList;
        }
    }
}
