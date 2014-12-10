using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Controller : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("skin", typeof(Skin))]
        public Skin Skin { get; set; }
        [XmlElement("morph", typeof(Morph))]
        public Morph Morph { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public override string ToString()
        {
            return "Controller; " + base.ToString();
        }
    }
}
