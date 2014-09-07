using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using SharpDX;
using SharpDX.Direct3D;

namespace Common.Collada
{
    using Common.Collada.Types;
    using Common.Utils;

    [Serializable]
    [XmlRoot("COLLADA")]
    public class Dae
    {
        public static Dae Load(string fileName)
        {
            using (StreamReader rd = new StreamReader(fileName))
            {
                XmlSerializer sr = new XmlSerializer(typeof(Dae), "http://www.collada.org/2005/11/COLLADASchema");

                return (Dae)sr.Deserialize(rd);
            }
        }

        internal static string Convert(Color4 color)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", color.Red, color.Green, color.Blue, color.Alpha);
        }

        internal static string Convert(Vector3 vector)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", vector.X, vector.Y, vector.Z);
        }

        internal static string Convert(List<int> intList)
        {
            string text = null;

            for (int x = 0; x < intList.Count; x++)
            {
                if (text != null) text += " ";

                text += string.Format("{0}", intList[x], CultureInfo.InvariantCulture);
            }

            return text;
        }

        internal static string Convert(List<float> intList)
        {
            string text = null;

            for (int x = 0; x < intList.Count; x++)
            {
                if (text != null) text += " ";

                text += string.Format("{0}", intList[x], CultureInfo.InvariantCulture);
            }

            return text;
        }

        internal static string Convert(List<List<int>> intList)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < intList.Count; i++)
            {
                string text = null;

                for (int x = 0; x < intList[i].Count; x++)
                {
                    if (text != null) text += " ";

                    text += string.Format("{0}", intList[i][x], CultureInfo.InvariantCulture);
                }

                list.Add(text);
            }

            return string.Join(Environment.NewLine, list.ToArray());
        }

        internal static string Convert(List<List<float>> floatList)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < floatList.Count; i++)
            {
                string text = null;

                for (int x = 0; x < floatList[i].Count; x++)
                {
                    if (text != null) text += " ";

                    text += string.Format("{0}", floatList[i][x], CultureInfo.InvariantCulture);
                }

                list.Add(text);
            }

            return string.Join(Environment.NewLine, list.ToArray());
        }

        internal static Color4 ConvertColor4(string stringValue)
        {
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] v = stringValue.Split(" ".ToCharArray());
                if (v.Length == 4)
                {
                    return new Color4(
                        float.Parse(v[0], CultureInfo.InvariantCulture),
                        float.Parse(v[1], CultureInfo.InvariantCulture),
                        float.Parse(v[2], CultureInfo.InvariantCulture),
                        float.Parse(v[3], CultureInfo.InvariantCulture));
                }
            }

            return new Color4(0);
        }

        internal static Vector3 ConvertVector3(string stringValue)
        {
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] v = stringValue.Split(" ".ToCharArray());
                if (v.Length == 3)
                {
                    return new Vector3(
                        float.Parse(v[0], CultureInfo.InvariantCulture),
                        float.Parse(v[1], CultureInfo.InvariantCulture),
                        float.Parse(v[2], CultureInfo.InvariantCulture));
                }
            }

            return new Vector3(0);
        }

        internal static List<int> ConvertIntArray(string stringList)
        {
            List<int> valueList = new List<int>();

            string[] strings = stringList.Replace("\t", "").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strings.Length; i++)
            {
                string[] values = strings[i].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < values.Length; x++)
                {
                    valueList.Add(int.Parse(values[x], CultureInfo.InvariantCulture));
                }
            }

            return valueList;
        }

        internal static List<float> ConvertFloatArray(string stringList)
        {
            List<float> valueList = new List<float>();

            string[] strings = stringList.Replace("\t", "").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strings.Length; i++)
            {
                string[] values = strings[i].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < values.Length; x++)
                {
                    valueList.Add(float.Parse(values[x], CultureInfo.InvariantCulture));
                }
            }

            return valueList;
        }

        internal static List<List<int>> ConvertIntArrayList(string stringList)
        {
            List<List<int>> list = new List<List<int>>();

            string[] strings = stringList.Replace("\t", "").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strings.Length; i++)
            {
                List<int> valueList = new List<int>();

                string[] values = strings[i].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < values.Length; x++)
                {
                    valueList.Add(int.Parse(values[x], CultureInfo.InvariantCulture));
                }

                list.Add(valueList);
            }

            return list;
        }

        internal static List<List<float>> ConvertFloatArrayList(string stringList)
        {
            List<List<float>> list = new List<List<float>>();

            string[] strings = stringList.Replace("\t", "").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strings.Length; i++)
            {
                List<float> valueList = new List<float>();

                string[] values = strings[i].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < values.Length; x++)
                {
                    valueList.Add(float.Parse(values[x], CultureInfo.InvariantCulture));
                }

                list.Add(valueList);
            }

            return list;
        }

        [XmlAttribute("version")]
        public Versions Version { get; set; }
        [XmlElement("asset")]
        public AssetType Asset { get; set; }
        [XmlArray("library_images")]
        [XmlArrayItem("image")]
        public List<LibraryImage> Images { get; set; }
        [XmlArray("library_effects")]
        [XmlArrayItem("effect")]
        public List<LibraryEffect> Effects { get; set; }
        [XmlArray("library_materials")]
        [XmlArrayItem("material")]
        public List<LibraryMaterial> Materials { get; set; }
        [XmlArray("library_geometries")]
        [XmlArrayItem("geometry")]
        public List<LibraryGeometry> Geometries { get; set; }
        [XmlArray("library_visual_scenes")]
        [XmlArrayItem("visual_scene")]
        public List<LibraryVisualScene> VisualScenes { get; set; }
        [XmlElement("scene")]
        public Scene Scene { get; set; }

        public Dae()
        {

        }

        public ColladaGeometryInfo[] MapScene(
            LibraryVisualScene scene,
            Matrix translation, Matrix rotation, Matrix scale,
            bool normalizeNormals,
            string contentPath)
        {
            List<ColladaGeometryInfo> geometryList = new List<ColladaGeometryInfo>();

            foreach (LibraryVisualSceneNode node in scene.Nodes)
            {
                foreach (InstanceGeometry instGeo in node.InstanceGeometry)
                {
                    ColladaGeometryInfo info = new ColladaGeometryInfo();

                    if (instGeo.BindMaterial != null)
                    {
                        string matId = instGeo.BindMaterial.TechniqueCommon.InstanceMaterial.Target;

                        LibraryMaterial mat = this.Materials.Find(m => "#" + m.Id == matId);
                        if (mat != null)
                        {
                            LibraryEffect effect = this.Effects.Find(e => "#" + e.Id == mat.InstanceEffect.Url);
                            if (effect != null)
                            {
                                Material emat = effect.Profile.CreateMaterial();

                                string image = effect.Profile.GetImage();
                                if (!string.IsNullOrEmpty(image))
                                {
                                    LibraryImage img = this.Images.Find(i => i.Id == image);
                                    if (img != null)
                                    {
                                        string path = Uri.UnescapeDataString(img.InitFrom);

                                        if (!string.IsNullOrEmpty(contentPath))
                                        {
                                            path = Path.Combine(contentPath, path);
                                        }

                                        emat.Texture = new TextureDescription()
                                        {
                                            Name = Uri.UnescapeDataString(img.InitFrom),
                                            TextureArray = new string[] { path },
                                        };
                                    }
                                }

                                info.Material = emat;
                            }
                        }
                    }

                    //Get Geometry
                    LibraryGeometry geo = this.Geometries.Find(g => "#" + g.Id == instGeo.Url);
                    if (geo != null)
                    {
                        info.AddVertices(geo.Map(translation, rotation, scale, normalizeNormals), PrimitiveTopology.TriangleList);
                    }

                    geometryList.Add(info);
                }
            }

            return geometryList.ToArray();
        }
    }
}
