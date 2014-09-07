using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class MeshVertices
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("input")]
        public List<Input> Input { get; set; }
    }
}
