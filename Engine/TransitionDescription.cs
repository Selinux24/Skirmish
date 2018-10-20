using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Transition description
    /// </summary>
    [Serializable]
    public class TransitionDescription
    {
        /// <summary>
        /// Clip from name
        /// </summary>
        [XmlAttribute("from")]
        public string ClipFrom { get; set; }
        /// <summary>
        /// Clip to name
        /// </summary>
        [XmlAttribute("to")]
        public string ClipTo { get; set; }
        /// <summary>
        /// Clip from start
        /// </summary>
        [XmlAttribute("startFrom")]
        public float StartFrom { get; set; }
        /// <summary>
        /// Clip to start
        /// </summary>
        [XmlAttribute("startTo")]
        public float StartTo { get; set; }
    }
}
