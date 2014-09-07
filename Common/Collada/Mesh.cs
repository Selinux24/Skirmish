using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class Mesh
    {
        [XmlElement("source")]
        public List<MeshSource> Sources { get; set; }
        [XmlElement("vertices")]
        public MeshVertices Vertices { get; set; }
        [XmlElement("polylist")]
        public MeshPolyList PolyList { get; set; }
    }
}
