using System;
using System.Xml.Serialization;

namespace Engine.Content
{
    using Engine.Animation;

    /// <summary>
    /// Model content description
    /// </summary>
    [Serializable]
    public class ContentDataDescription
    {
        /// <summary>
        /// Model file name
        /// </summary>
        [XmlElement("model_filename")]
        public string ModelFileName { get; set; } = null;
        /// <summary>
        /// Meshes by level of detail in the file
        /// </summary>
        /// <remarks>For model files containing several levels of details for the same model</remarks>
        [XmlArray("LOD_meshes")]
        [XmlArrayItem("mesh", typeof(string))]
        public string[] LODMeshes { get; set; } = null;
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
        /// Armature name
        /// </summary>
        [XmlElement("armature_name")]
        public string ArmatureName { get; set; } = null;
        /// <summary>
        /// Use controller transforms
        /// </summary>
        [XmlElement("use_controller_transform")]
        public bool UseControllerTransform { get; set; } = true;
        /// <summary>
        /// Bake transforms
        /// </summary>
        [XmlElement("bake_transforms")]
        public bool BakeTransforms { get; set; } = true;
    }
}
