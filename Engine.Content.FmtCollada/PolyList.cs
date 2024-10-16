using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class PolyList : MeshElements
    {
        [XmlElement("vcount", typeof(BasicIntArray))]
        public BasicIntArray VCount { get; set; }
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray P { get; set; }
    }
}
