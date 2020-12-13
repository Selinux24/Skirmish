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
        /// Default decal drawer description
        /// </summary>
        /// <param name="textureFileName">Texture file</param>
        /// <param name="maxCount">Max decal count</param>
        public static DecalDrawerDescription Default(string textureFileName, int maxCount)
        {
            return new DecalDrawerDescription
            {
                TextureName = textureFileName,
                MaxDecalCount = maxCount,
            };
        }
        /// <summary>
        /// Default rotating decal drawer description
        /// </summary>
        /// <param name="textureFileName">Texture file</param>
        /// <param name="maxCount">Max decal count</param>
        public static DecalDrawerDescription DefaultRotate(string textureFileName, int maxCount)
        {
            return new DecalDrawerDescription
            {
                TextureName = textureFileName,
                MaxDecalCount = maxCount,
                RotateDecals = true,
            };
        }

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
