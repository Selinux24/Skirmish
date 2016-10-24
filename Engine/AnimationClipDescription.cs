using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Animation clip description
    /// </summary>
    [Serializable]
    public class AnimationClipDescription
    {
        /// <summary>
        /// Clip name
        /// </summary>
        [XmlAttribute("name")]
        public string Name;
        /// <summary>
        /// Index from
        /// </summary>
        [XmlAttribute("from")]
        public int From;
        /// <summary>
        /// Index to
        /// </summary>
        [XmlAttribute("to")]
        public int To;
    }
}
