using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class TechniqueCommon
    {
        [XmlElement("instance_material")]
        public InstanceMaterial InstanceMaterial { get; set; }
    }
}
