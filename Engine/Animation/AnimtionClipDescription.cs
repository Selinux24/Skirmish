using System;
using System.Xml.Serialization;

namespace Engine.Animation
{
    /// <summary>
    /// Animation clip description
    /// </summary>
    [Serializable]
    public class AnimtionClipDescription
    {
        /// <summary>
        /// Clip name
        /// </summary>
        [XmlElement("name")]
        public string Name;
        /// <summary>
        /// Index from
        /// </summary>
        [XmlElement("from")]
        public int From;
        /// <summary>
        /// Index to
        /// </summary>
        [XmlElement("to")]
        public int To;
    }
}
