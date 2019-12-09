using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class ForceField : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }

        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
