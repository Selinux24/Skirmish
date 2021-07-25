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
        public IEnumerable<ContentData> Load(string contentFolder, ContentDataFile content)
        {
            Matrix transform = Matrix.Identity;

            if (content.Scale != 1f)
            {
                transform = Matrix.Scaling(content.Scale);
            }

            var meshList = Load(contentFolder, content.ModelFileName, transform, out var materials);
            if (meshList.Any())
            {
                ContentData m = new ContentData();

                foreach (var mat in materials)
                {
                    if (!m.Materials.ContainsKey(mat.Name))
                    {
                        var matContent = mat.CreateContent();

                        m.Materials.Add(mat.Name, matContent);

                        m.TryAddTexture(contentFolder, mat.MapKa);
                        m.TryAddTexture(contentFolder, mat.MapKd);
                        m.TryAddTexture(contentFolder, mat.MapKs);
                        m.TryAddTexture(contentFolder, mat.MapNs);
                        m.TryAddTexture(contentFolder, mat.MapD);
                        m.TryAddTexture(contentFolder, mat.MapBump);
                    }
                }

                for (int i = 0; i < meshList.Count(); i++)
                {
                    var mesh = meshList.ElementAt(i);
                    m.Geometry.Add($"Mesh{i + 1}", mesh.Material ?? ContentData.NoMaterial, mesh);
                }

                return new[] { m };
            }

            return new ContentData[] { };
        }
        /// <summary>
        /// Loads a model content list from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <param name="transform">Transform</param>
        /// <param name="materials">Resulting material list</param>
        /// <returns>Returns a list of model contents</returns>
        private IEnumerable<SubMeshContent> Load(string contentFolder, string fileName, Matrix transform, out IEnumerable<Material> materials)
        {
            List<Material> matList = new List<Material>();

            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList.Any())
            {
                List<SubMeshContent> res = new List<SubMeshContent>();

                foreach (var model in modelList)
                {
                    Reader.LoadObj(model, contentFolder, transform, out var contentList, out var mats);

                    matList.AddRange(mats);
                    res.AddRange(contentList);
                }

                materials = matList.ToArray();

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
        public void Save(IEnumerable<ContentData> models, string fileName)
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

    static class ContentExtensions
    {
        public static MaterialBlinnPhongContent CreateContent(this Material mat)
        {
            var matContent = MaterialBlinnPhongContent.Default;

            if (mat.MapNs != null)
            {
                matContent.SpecularTexture = mat.MapNs;
            }
            else
            {
                matContent.Shininess = mat.Ns;
            }

            matContent.AmbientTexture = mat.MapKa;

            if (mat.MapKd != null)
            {
                matContent.DiffuseTexture = mat.MapKd;
            }
            else
            {
                matContent.DiffuseColor = new Color4(mat.Kd, 1f);
            }

            if (mat.MapKs != null)
            {
                matContent.SpecularTexture = mat.MapKs;
            }
            else
            {
                matContent.SpecularColor = mat.Ks;
            }

            matContent.EmissiveColor = mat.Ke;
            matContent.Shininess = mat.Ni;

            matContent.IsTransparent = mat.D != 0;

            matContent.NormalMapTexture = mat.MapBump;

            return matContent;
        }

        public static void TryAddTexture(this ContentData m, string contentFolder, string texture)
        {
            if (texture != null && !m.Images.ContainsKey(texture))
            {
                string path = Path.Combine(contentFolder, texture);
                if (File.Exists(path))
                {
                    m.Images.Add(texture, new ImageContent() { Path = path });
                }
            }
        }
    }
}
