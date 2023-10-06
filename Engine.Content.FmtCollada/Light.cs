using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Light : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("technique_common", typeof(LightTechniqueCommon))]
        public LightTechniqueCommon[] LightTechniqueCommon { get; set; }
        [XmlElement("technique", typeof(Technique))]
        public Technique[] Technique { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
