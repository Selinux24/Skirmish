using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class InstanceEffect
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("technique_hint", typeof(TechniqueHint))]
        public TechniqueHint TechniqueHint { get; set; }
        [XmlElement("setparam", typeof(SetParam))]
        public SetParam[] SetParams { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
