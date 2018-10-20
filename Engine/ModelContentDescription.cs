using System;
using System.IO;
using System.Xml.Serialization;

namespace Engine
{
    using Engine.Content;

    /// <summary>
    /// Model content description
    /// </summary>
    [Serializable]
    public class ModelContentDescription
    {
        /// <summary>
        /// Model file name
        /// </summary>
        [XmlElement("model_filename")]
        public string ModelFileName { get; set; } = null;
        /// <summary>
        /// Volume meshes collection
        /// </summary>
        [XmlArray("volumes")]
        [XmlArrayItem("volume", typeof(string))]
        public string[] VolumeMeshes { get; set; } = null;
        /// <summary>
        /// Animation description
        /// </summary>
        [XmlElement("animation_description")]
        public AnimationDescription Animation { get; set; } = null;
        /// <summary>
        /// Model scale
        /// </summary>
        [XmlElement("scale")]
        public float Scale { get; set; } = 1f;
        /// <summary>
        /// Use controller transforms
        /// </summary>
        [XmlElement("use_controller_transform")]
        public bool UseControllerTransform { get; set; } = true;

        /// <summary>
        /// Gets the loader for the current file extension
        /// </summary>
        /// <returns>Returns a loader</returns>
        public virtual ILoader GetLoader()
        {
            if (string.Equals(Path.GetExtension(ModelFileName), ".dae", StringComparison.OrdinalIgnoreCase))
            {
                return new LoaderCollada();
            }
            else if (string.Equals(Path.GetExtension(ModelFileName), ".obj", StringComparison.OrdinalIgnoreCase))
            {
                return new LoaderObj();
            }

            return null;
        }
    }
}
