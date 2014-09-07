using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class InstanceMaterial
    {
        [XmlAttribute("symbol")]
        public string Symbol { get; set; }
        [XmlAttribute("target")]
        public string Target { get; set; }
    }
}
