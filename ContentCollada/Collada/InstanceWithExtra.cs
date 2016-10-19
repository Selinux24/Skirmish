using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class InstanceWithExtra
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
