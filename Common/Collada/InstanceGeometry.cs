using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class InstanceGeometry
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
        [XmlElement("bind_material")]
        public BindMaterial BindMaterial { get; set; }
    }
}
