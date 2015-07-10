using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Animation : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Sources { get; set; }
        [XmlElement("sampler", typeof(Sampler))]
        public Sampler[] Samplers { get; set; }
        [XmlElement("channel", typeof(Channel))]
        public Channel[] Channels { get; set; }
        [XmlElement("animation", typeof(Animation))]
        public Animation[] Animations { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public Source this[string id]
        {
            get
            {
                return Array.Find(this.Sources, s => string.Equals("#" + s.Id, id));
            }
        }

        public override string ToString()
        {
            return "Animation; " + base.ToString();
        }
    }
}
