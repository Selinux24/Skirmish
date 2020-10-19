using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Imager
    {
        [XmlElement("technique", typeof(Technique))]
        public Technique[] Techniques { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
