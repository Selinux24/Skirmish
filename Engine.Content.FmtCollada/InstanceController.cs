using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class InstanceController : InstanceWithExtra
    {
        [XmlElement("skeleton", DataType = "anyURI")]
        public string[] Skeleton { get; set; }
        [XmlElement("bind_material", typeof(BindMaterial))]
        public BindMaterial BindMaterial { get; set; }
    }
}
