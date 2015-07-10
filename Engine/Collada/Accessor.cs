using System;
using System.Xml.Serialization;

namespace Engine.Collada
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
            this.Offset = 0;
            this.Stride = 1;
        }

        public override string ToString()
        {
            return string.Format("Source: {0}; Offset: {1}; Stride: {2};", this.Source, this.Offset, this.Stride);
        }
    }
}
