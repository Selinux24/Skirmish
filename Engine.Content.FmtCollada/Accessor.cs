using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class Accessor
    {
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlAttribute("offset")]
        public int Offset { get; set; }
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("stride")]
        public int Stride { get; set; }
        [XmlElement("param", typeof(Param))]
        public Param[] Params { get; set; }

        public Accessor()
        {
            Offset = 0;
            Stride = 1;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Source: {Source}; Offset: {Offset}; Stride: {Stride};";
        }
    }
}
