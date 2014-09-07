using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class SurfaceType
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlElement("init_from")]
        public SurfaceInitFromType InitFrom { get; set; }
        [XmlElement("format")]
        public string Format { get; set; }
        [XmlElement("format_hint")]
        public SurfaceFormatHintType FormatHint { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1};", this.Type, this.InitFrom);
        }
    }
}
