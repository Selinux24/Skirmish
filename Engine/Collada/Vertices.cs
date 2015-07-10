using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Vertices : NamedNode
    {
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
