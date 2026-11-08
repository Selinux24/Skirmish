using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class InstanceGeometry : InstanceWithExtra
    {
        [XmlElement("bind_material", typeof(BindMaterial))]
        public BindMaterial BindMaterial { get; set; }
    }
}
