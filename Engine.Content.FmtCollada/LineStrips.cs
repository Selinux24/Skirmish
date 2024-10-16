using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class LineStrips : MeshElements
    {
        [XmlElement("p", typeof(BasicIntArray))]
        public BasicIntArray[] P { get; set; }
    }
}
