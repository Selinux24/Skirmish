using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class LineStrips : MeshElements
    {
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray[] P { get; set; }
    }
}
