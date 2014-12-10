using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Technique
    {
        [XmlAttribute("profile")]
        public string Profile { get; set; }
        [XmlAnyElement]
        public object[] Any { get; set; }
    }
}
