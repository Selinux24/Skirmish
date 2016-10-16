using System;
using System.Xml.Serialization;

namespace Engine.Animation
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
        [XmlElement("from")]
        public string ClipFrom;
        /// <summary>
        /// Clip to name
        /// </summary>
        [XmlElement("to")]
        public string ClipTo;
        /// <summary>
        /// Clip from start
        /// </summary>
        [XmlElement("startFrom")]
        public float StartFrom;
        /// <summary>
        /// Clip to start
        /// </summary>
        [XmlElement("startTo")]
        public float StartTo;
    }
}
