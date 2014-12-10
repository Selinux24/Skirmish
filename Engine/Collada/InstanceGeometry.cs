using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class InstanceGeometry : InstanceWithExtra
    {
        [XmlElement("bind_material", typeof(BindMaterial))]
        public BindMaterial BindMaterial { get; set; }
    }
}
