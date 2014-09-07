using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class ImageType
    {
        [XmlElement("data", typeof(byte[]), DataType = "hexBinary")]
        public byte[] Data { get; set; }
        [XmlElement("init_from", typeof(string), DataType = "anyURI")]
        public string InitFrom { get; set; }
        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }
        [XmlAttribute(DataType = "NCName")]
        public string Name { get; set; }
        [XmlAttribute(DataType = "token")]
        public string Format { get; set; }
        [XmlAttribute("height")]
        public ulong Height { get; set; }
        [XmlAttribute("width")]
        public ulong Width { get; set; }
        [XmlAttribute("depth")]
        [DefaultValueAttribute(typeof(ulong), "1")]
        public ulong Depth { get; set; }
    }
}
