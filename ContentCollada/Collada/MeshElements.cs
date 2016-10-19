using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Collada.Types;

    [Serializable]
    public class MeshElements
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlAttribute("material")]
        public string Material { get; set; }
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
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
