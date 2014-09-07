using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class SurfaceFormatHintType
    {
        [XmlElement("channels")]
        public SurfaceFormatHintChannelsEnum Channels { get; set; }
        [XmlElement("range")]
        public SurfaceFormatHintRangeEnum Range { get; set; }
        [XmlElement("precision")]
        public SurfaceFormatHintPrecisionEnum Precision { get; set; }
        [XmlElement("option")]
        public SurfaceFormatHintOptionEnum[] Options { get; set; }
    }
}
