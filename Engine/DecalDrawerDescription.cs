using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Decal description
    /// </summary>
    [Serializable]
    public class DecalDrawerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Texture name
        /// </summary>
        [XmlAttribute("textureName")]
        public string TextureName { get; set; }
        /// <summary>
        /// Maximum decal count
        /// </summary>
        [XmlElement("maxDecalCount")]
        public int MaxDecalCount { get; set; } = 100;
        /// <summary>
        /// Rotate decals
        /// </summary>
        [XmlElement("rotateDecals")]
        public bool RotateDecals { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public DecalDrawerDescription() : base()
        {

        }
    }
}
