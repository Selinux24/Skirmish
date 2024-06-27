using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Morph
    {
        [XmlAttribute("method")]
        public EnumMorphMethods Method { get; set; }
        [XmlAttribute("source")]
        public string SourceUri { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Sources { get; set; }
        [XmlElement("targets", typeof(Targets))]
        public Targets Targets { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public Source this[string id]
        {
            get
            {
                return Array.Find(Sources, s => string.Equals("#" + s.Id, id));
            }
        }

        public Morph()
        {
            Method = EnumMorphMethods.Normalized;
        }
    }
}
