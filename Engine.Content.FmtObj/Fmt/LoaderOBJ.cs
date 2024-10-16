using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Content.FmtObj.Fmt
{
    using Engine;
    using Engine.Content;
    using Engine.Content.Persistence;

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
        /// Loads a model content list from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <param name="transform">Transform</param>
        /// <param name="materials">Resulting material list</param>
        /// <returns>Returns a list of model contents</returns>
        private static List<SubMeshContent> Load(string contentFolder, string fileName, Matrix transform, out List<Material> materials)
        {
            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (!modelList.Any())
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }

            string materialsFolder = Path.Combine(contentFolder, Path.GetDirectoryName(fileName));

            var matList = new List<Material>();
            var res = new List<SubMeshContent>();

            foreach (var model in modelList)
            {
                Reader.LoadObj(model, materialsFolder, transform, out var contentList, out var mats);

                matList.AddRange(mats);
                res.AddRange(contentList);
            }

            materials = matList;

            return res;
        }
        /// <summary>
        /// Saves a triangle list in a file
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="fileName">File name</param>
        public static void Save(IEnumerable<Triangle> triangles, string fileName)
        {
            // Write the file
            using var wr = new StreamWriter(fileName, false, Encoding.Default);
            Writer.WriteObj(wr, triangles);
        }
        /// <summary>
        /// Saves a model list into a file
        /// </summary>
        /// <param name="models"></param>
        /// <param name="fileName"></param>
        public static void Save(IEnumerable<ContentData> models, string fileName)
        {
            // Write the file
            using var wr = new StreamWriter(fileName, false, Encoding.Default);
            foreach (var content in models)
            {
                var geom = content.GetGeometryContent();
                foreach (var g in geom)
                {
                    foreach (var s in g.Content.Values)
                    {
                        Writer.WriteObj(wr, s);
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LoaderObj()
        {

        }

        /// <summary>
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate which creates a loader</returns>
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
            return [".obj"];
        }

        /// <summary>
        /// Loads a model content list from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content description</param>
        /// <returns>Returns a list of model contents</returns>
        public async Task<IEnumerable<ContentData>> Load(string contentFolder, ContentDataFile content)
        {
            var transform = content.GetTransform();

            var meshList = Load(contentFolder, content.ModelFileName, transform, out var materials);
            if (meshList.Count == 0)
            {
                return [];
            }

            ContentData m = new()
            {
                Name = Path.GetFileNameWithoutExtension(content.ModelFileName)
            };

            await Task.Run(() =>
            {
                foreach (var mat in materials)
                {
                    if (m.ContainsMaterialContent(mat.Name))
                    {
                        continue;
                    }

                    var matContent = mat.CreateContent();

                    m.AddMaterialContent(mat.Name, matContent);

                    m.TryAddTexture(contentFolder, mat.MapKa);
                    m.TryAddTexture(contentFolder, mat.MapKd);
                    m.TryAddTexture(contentFolder, mat.MapKs);
                    m.TryAddTexture(contentFolder, mat.MapNs);
                    m.TryAddTexture(contentFolder, mat.MapD);
                    m.TryAddTexture(contentFolder, mat.MapBump);
                }

                for (int i = 0; i < meshList.Count; i++)
                {
                    var mesh = meshList[i];

                    if (!m.ContainsMaterialContent(mesh.Material))
                    {
                        mesh.Material = ContentData.NoMaterial;
                    }

                    m.ImportMaterial($"Mesh{i + 1}", mesh.Material, mesh);
                }
            });

            return [m];
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

            matContent.IsTransparent = mat.D < 1f;

            matContent.NormalMapTexture = mat.MapBump;

            return matContent;
        }

        public static void TryAddTexture(this ContentData m, string contentFolder, string texture)
        {
            if (string.IsNullOrWhiteSpace(texture))
            {
                return;
            }

            string path = Path.Combine(contentFolder, texture);
            if (!File.Exists(path))
            {
                return;
            }

            m.AddTextureContent(texture, new FileArrayImageContent(path));
        }
    }
}
