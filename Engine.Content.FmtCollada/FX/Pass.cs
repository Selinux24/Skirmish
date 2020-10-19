using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class Pass
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlElement("annotate", typeof(Annotate))]
        public Annotate[] Annotates { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
