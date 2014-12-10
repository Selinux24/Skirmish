using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class SurfaceFormatHint
    {
        [XmlElement("channels")]
        public EnumChannels Channels { get; set; }
        [XmlElement("range")]
        public EnumRange Range { get; set; }
        [XmlElement("precision")]
        public EnumPrecision Precision { get; set; }
        [XmlElement("option")]
        public EnumOption[] Option { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extra { get; set; }
    }
}
