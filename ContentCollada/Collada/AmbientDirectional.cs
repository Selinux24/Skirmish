using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Collada.Types;

    [Serializable]
    public class AmbientDirectional
    {
        [XmlElement("color", typeof(BasicColor))]
        public BasicColor Color { get; set; }
    }
}
