using System;
using System.Xml.Serialization;

namespace Engine.Content
{
    /// <summary>
    /// Animation content data description
    /// </summary>
    [Serializable]
    public class AnimationLibContentDataDescription
    {
        /// <summary>
        /// Animation file name
        /// </summary>
        [XmlElement("animation_filename")]
        public string AnimationFileName { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationLibContentDataDescription()
        {

        }
    }
}
