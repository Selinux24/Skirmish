using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class MeshSource
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("float_array")]
        public FloatArrayType FloatArray { get; set; }
        [XmlElement("technique_common")]
        public MeshTechnique TechniqueCommon { get; set; }
    }
}
