using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class ParamType
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("use")]
        public string Use { get; set; }
    }
}
