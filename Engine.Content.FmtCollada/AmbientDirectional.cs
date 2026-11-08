using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class AmbientDirectional
    {
        [XmlElement("color", typeof(BasicColor))]
        public BasicColor Color { get; set; }
    }
}
