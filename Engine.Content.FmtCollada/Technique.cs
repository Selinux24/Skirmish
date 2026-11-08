using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Technique
    {
        [XmlAttribute("profile")]
        public string Profile { get; set; }
        [XmlAnyElement]
        public object[] Any { get; set; }
        [XmlElement("bump", typeof(TechniqueBumpMapping))]
        public TechniqueBumpMapping[] BumpMaps { get; set; }
    }

    [Serializable]
    public class TechniqueBumpMapping
    {
        [XmlElement("texture", typeof(BasicTexture))]
        public BasicTexture Texture { get; set; }
    }
}
