using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class Input
    {
        [XmlAttribute("semantic")]
        public InputSemantics Semantic { get; set; }
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("offset")]
        public int Offset { get; set; }
    }
}
