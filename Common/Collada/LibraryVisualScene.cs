using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class LibraryVisualScene
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("node")]
        public List<LibraryVisualSceneNode> Nodes { get; set; }
    }
}
