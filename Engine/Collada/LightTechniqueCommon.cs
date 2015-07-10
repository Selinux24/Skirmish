using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class LightTechniqueCommon
    {
        [XmlElement("ambient", typeof(AmbientDirectional))]
        public AmbientDirectional Ambient { get; set; }
        [XmlElement("directional", typeof(AmbientDirectional))]
        public AmbientDirectional Directional { get; set; }
        [XmlElement("point", typeof(Point))]
        public Point Point { get; set; }
        [XmlElement("spot", typeof(Spot))]
        public Spot Spot { get; set; }
    }
}
