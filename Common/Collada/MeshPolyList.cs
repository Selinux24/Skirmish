using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class MeshPolyList
    {
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlAttribute("material")]
        public string Material { get; set; }
        [XmlElement("input")]
        public List<Input> Input { get; set; }
        [XmlElement("vcount")]
        public string VCountString
        {
            get { return Dae.Convert(this.VCount); }
            set { this.VCount = Dae.ConvertIntArrayList(value); }
        }
        [XmlElement("p")]
        public string PString
        {
            get { return Dae.Convert(this.P); }
            set { this.P = Dae.ConvertIntArray(value); }
        }
        [XmlIgnore]
        public List<List<int>> VCount { get; set; }
        [XmlIgnore]
        public List<int> P { get; set; }

        public int this[InputSemantics semantic]
        {
            get
            {
                Input input = this.Input.Find(i => i.Semantic == semantic);
                if (input != null)
                {
                    return input.Offset;
                }

                return -1;
            }
        }
    }
}
