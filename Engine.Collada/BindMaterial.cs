using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class BindMaterial
    {
        [XmlElement("param", typeof(Param))]
        public Param[] Param { get; set; }
        [XmlElement("technique_common", typeof(BindMaterialTechniqueCOMMON))]
        public BindMaterialTechniqueCOMMON[] TechniqueCOMMON { get; set; }
        [XmlElement("technique", typeof(Technique))]
        public Technique[] Techniques { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }

    [Serializable]
    public class BindMaterialTechniqueCOMMON
    {
        [XmlElement("instance_material", typeof(InstanceMaterial))]
        public InstanceMaterial[] InstanceMaterial { get; set; }
    }
}
