using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class AssetType
    {
        [XmlElement("contributor")]
        public Contributor Contributor { get; set; }
        [XmlElement("coverage")]
        public Coverage Coverage { get; set; }
        [XmlElement("created")]
        public DateTime Created { get; set; }
        [XmlElement("keywords")]
        public string Keywords { get; set; }
        [XmlElement("modified")]
        public DateTime Modified { get; set; }
        [XmlElement("revision")]
        public string Revision { get; set; }
        [XmlElement("subject")]
        public string Subject { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("unit")]
        public UnitType Unit { get; set; }
        [XmlElement("up_axis")]
        public UpAxisType UpAxisType { get; set; }
    }
}
