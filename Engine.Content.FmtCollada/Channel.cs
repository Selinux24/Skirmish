using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Channel
    {
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("target")]
        public string Target { get; set; }
    }
}
