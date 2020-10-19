using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class TechniqueHint
    {
        [XmlAttribute("platform")]
        public string Platform { get; set; }
        [XmlAttribute("profile")]
        public string Profile { get; set; }
        [XmlAttribute("ref")]
        public string Ref { get; set; }
    }
}
