using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    [XmlRoot("COLLADA")]
    public class Collada
    {
        #region Description

        [XmlAttribute("version")]
        public EnumVersions Version { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("scene", typeof(Scene))]
        public Scene Scene { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        #endregion

        #region Library

        [XmlArray("library_animations")]
        [XmlArrayItem("animation", typeof(Animation))]
        public Animation[] LibraryAnimations { get; set; }
        [XmlArray("library_animation_clips")]
        [XmlArrayItem("animation_clip", typeof(AnimationClip))]
        public AnimationClip[] LibraryAnimationClips { get; set; }
        [XmlArray("library_cameras")]
        [XmlArrayItem("camera", typeof(Camera))]
        public Camera[] LibraryCameras { get; set; }
        [XmlArray("library_controllers")]
        [XmlArrayItem("controller", typeof(Controller))]
        public Controller[] LibraryControllers { get; set; }
        [XmlArray("library_effects")]
        [XmlArrayItem("effect", typeof(Effect))]
        public Effect[] LibraryEffects { get; set; }
        [XmlArray("library_force_fields")]
        [XmlArrayItem("force_field", typeof(ForceField))]
        public ForceField[] LibraryForceFields { get; set; }
        [XmlArray("library_geometries")]
        [XmlArrayItem("geometry", typeof(Geometry))]
        public Geometry[] LibraryGeometries { get; set; }
        [XmlArray("library_images")]
        [XmlArrayItem("image", typeof(Image))]
        public Image[] LibraryImages { get; set; }
        [XmlArray("library_lights")]
        [XmlArrayItem("light", typeof(Light))]
        public Light[] LibraryLights { get; set; }
        [XmlArray("library_materials")]
        [XmlArrayItem("material", typeof(Material))]
        public Material[] LibraryMaterials { get; set; }
        [XmlArray("library_nodes")]
        [XmlArrayItem("node", typeof(Node))]
        public Node[] LibraryNodes { get; set; }
        [XmlArray("library_physics_materials")]
        [XmlArrayItem("physic_material", typeof(PhysicMaterial))]
        public PhysicMaterial[] LibraryPhysicsMaterials { get; set; }
        [XmlArray("library_physics_models")]
        [XmlArrayItem("physics_model", typeof(PhysicsModel))]
        public PhysicsModel[] LibraryPhysicsModels { get; set; }
        [XmlArray("library_physics_scenes")]
        [XmlArrayItem("physics_scene", typeof(PhysicsScene))]
        public PhysicsScene[] LibraryPhysicsScenes { get; set; }
        [XmlArray("library_visual_scenes")]
        [XmlArrayItem("visual_scene", typeof(VisualScene))]
        public VisualScene[] LibraryVisualScenes { get; set; }

        #endregion

        #region Converters

        internal static T Convert<T>(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return (T)System.Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            else
            {
                return default;
            }
        }
        internal static T[] ConvertArray<T>(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var res = new List<T>();

                string oneLineText = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");

                string[] vList = oneLineText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < vList.Length; i++)
                {
                    T cnv = (T)System.Convert.ChangeType(vList[i], typeof(T), CultureInfo.InvariantCulture);

                    res.Add(cnv);
                }

                return [.. res];
            }
            else
            {
                return [];
            }
        }
        internal static string ConvertToString<T>(T value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
        internal static string ConvertArrayToString<T>(T[] values)
        {
            if (values != null && values.Length > 0)
            {
                var res = new StringBuilder();

                for (int i = 0; i < values.Length; i++)
                {
                    if (i > 0) res.Append(' ');

                    res.Append(string.Format(CultureInfo.InvariantCulture, "{0}", values[i]));
                }

                return res.ToString();
            }
            else
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Load model into helper clases
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="upAxis">Desired up axis</param>
        /// <returns>Return helper clases</returns>
        public static Collada Load(MemoryStream file)
        {
            Collada dae = null;

            try
            {
                using var rd = new StreamReader(file, Encoding.Default);
                var sr = new XmlSerializer(typeof(Collada), "http://www.collada.org/2005/11/COLLADASchema");

                dae = (Collada)sr.Deserialize(rd);
            }
            catch (Exception ex)
            {
                throw new ContentColladaException("Invalid COLLADA file.", ex);
            }

            return dae;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Collada()
        {

        }
    }
}
