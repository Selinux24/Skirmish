using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class VertexWeights
    {
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
        [XmlElement("vcount")]
        public BasicIntArray VCount { get; set; }
        [XmlElement("v")]
        public BasicIntArray V { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public Input this[EnumSemantics semantic]
        {
            get
            {
                return Array.Find(this.Inputs, i => i.Semantic == semantic);
            }
        }
    }
}
