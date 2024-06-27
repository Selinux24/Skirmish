using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Semantic: {Semantic}; Source: {Source}; Offset: {Offset}; Set: {Set}";
        }
    }
}
