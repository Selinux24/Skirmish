using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Collada.Types;

    [Serializable]
    public class Morph
    {
        [XmlAttribute("method")]
        public EnumMorphMethods Method { get; set; }
        [XmlAttribute("source")]
        public string SourceUri { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Source { get; set; }
        [XmlElement("targets", typeof(Targets))]
        public Targets Targets { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public Morph()
        {
            this.Method = EnumMorphMethods.Normalized;
        }
    }
}
