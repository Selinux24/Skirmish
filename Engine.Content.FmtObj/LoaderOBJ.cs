using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Content.FmtObj
{
    /// <summary>
    /// Loader for .obj files
    /// </summary>
    public class LoaderObj : ILoader
    {
        /// <summary>
        /// Default locale
        /// </summary>
        public static CultureInfo Locale { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Constructor
        /// </summary>
        public LoaderObj()
        {

        }

        /// <summary>
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate wich creates a loader</returns>
        public Func<ILoader> GetLoaderDelegate()
        {
            return () => { return new LoaderObj(); };
        }

        /// <summary>
        /// Gets the extensions list which this loader is valid
        /// </summary>
        /// <returns>Returns a extension array list</returns>
        public IEnumerable<string> GetExtensions()
        {
            return new string[] { ".obj" };
        }

        /// <summary>
        /// Loads a model content list from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content description</param>
        /// <returns>Returns a list of model contents</returns>
        public IEnumerable<ModelContent> Load(string contentFolder, ModelContentDescription content)
        {
            Matrix transform = Matrix.Identity;

            if (content.Scale != 1f)
            {
                transform = Matrix.Scaling(content.Scale);
            }

            var meshList = Load(contentFolder, content.ModelFileName, transform);
            if (meshList.Any())
            {
                ModelContent m = new ModelContent();

                for (int i = 0; i < meshList.Count(); i++)
                {
                    var mesh = meshList.ElementAt(i);
                    m.Geometry.Add($"Mesh{i + 1}", ModelContent.NoMaterial, mesh);
                }

                return new[] { m };
            }

            return new ModelContent[] { };
        }
        /// <summary>
        /// Loads a model content list from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <param name="transform">Transform</param>
        /// <returns>Returns a list of model contents</returns>
        private IEnumerable<SubMeshContent> Load(string contentFolder, string fileName, Matrix transform)
        {
            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList.Any())
            {
                List<SubMeshContent> res = new List<SubMeshContent>();

                foreach (var model in modelList)
                {
                    Reader.LoadObj(model, transform, out var contentList);

                    res.AddRange(contentList);
                }

                return res.ToArray();
            }
            else
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }
        }
        
        /// <summary>
        /// Saves a triangle list in a file
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="fileName">File name</param>
        public void Save(IEnumerable<Triangle> triangles, string fileName)
        {
            // Write the file
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                Writer.WriteObj(wr, triangles);
            }
        }
        /// <summary>
        /// Saves a model list into a file
        /// </summary>
        /// <param name="models"></param>
        /// <param name="fileName"></param>
        public void Save(IEnumerable<ModelContent> models, string fileName)
        {
            // Write the file
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                foreach (var geo in models)
                {
                    foreach (var g in geo.Geometry.Values)
                    {
                        foreach (var s in g.Values)
                        {
                            Writer.WriteObj(wr, s);
                        }
                    }
                }
            }
        }
    }
}
