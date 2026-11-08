using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Sampler
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
        [XmlIgnore]
        public Input this[EnumSemantics semantic]
        {
            get
            {
                return Array.Find(Inputs, i => i.Semantic == semantic);
            }
        }
    }
}
