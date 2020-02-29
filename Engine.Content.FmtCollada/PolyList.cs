using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class PolyList : MeshElements
    {
        [XmlElement("vcount", typeof(BasicIntArray))]
        public BasicIntArray VCount { get; set; }
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray P { get; set; }
    }
}
