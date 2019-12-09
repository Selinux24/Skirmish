using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class Input
    {
        [XmlAttribute("offset")]
        public int Offset { get; set; }
        [XmlAttribute("semantic")]
        public EnumSemantics Semantic { get; set; }
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("set")]
        public int Set { get; set; }

        public override string ToString()
        {
            return string.Format("Semantic: {0}; Source: {1}; Offset: {2}; Set: {3}", this.Semantic, this.Source, this.Offset, this.Set);
        }
    }
}
