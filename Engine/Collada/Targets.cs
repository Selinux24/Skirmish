using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Targets
    {
        [XmlElement("input", typeof(Input))]
        public Input[] Inputs { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
