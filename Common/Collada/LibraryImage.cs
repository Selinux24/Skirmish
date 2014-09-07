using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class LibraryImage
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("init_from")]
        public string InitFrom { get; set; }
    }
}
