using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicIDREF
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("@ref", DataType = "IDREF")]
        public string Reference { get; set; }

        public BasicIDREF()
        {

        }

        public BasicIDREF(string value)
        {
            this.Reference = value;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Reference);
        }
    }
}
