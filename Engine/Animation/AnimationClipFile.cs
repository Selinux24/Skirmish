using System;
using System.Xml.Serialization;

namespace Engine.Animation
{
    /// <summary>
    /// Animation clip description
    /// </summary>
    [Serializable]
    public class AnimationClipFile
    {
        /// <summary>
        /// Clip name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Index from
        /// </summary>
        [XmlAttribute("from")]
        public int From { get; set; }
        /// <summary>
        /// Index to
        /// </summary>
        [XmlAttribute("to")]
        public int To { get; set; }
        /// <summary>
        /// Skeleton name
        /// </summary>
        [XmlAttribute("skeleton")]
        public string Skeleton { get; set; }
    }
}
