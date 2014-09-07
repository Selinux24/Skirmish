using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class BindMaterial
    {
        [XmlElement("technique_common")]
        public TechniqueCommon TechniqueCommon { get; set; }
    }
}
