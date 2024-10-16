using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class InputLocal
    {
        [XmlAttribute("semantic")]
        public string Semantic { get; set; }
        [XmlAttribute("source")]
        public string Source { get; set; }
    }
}
