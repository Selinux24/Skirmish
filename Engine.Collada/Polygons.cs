using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class Polygons : MeshElements
    {
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray[] P { get; set; }
        [XmlElement("ph", typeof(Ph))]
        public Ph[] Ph { get; set; }
    }
}
