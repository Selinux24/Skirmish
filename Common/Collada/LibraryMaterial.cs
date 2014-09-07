using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class LibraryMaterial
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("instance_effect")]
        public InstanceEffect InstanceEffect { get; set; }
    }
}
