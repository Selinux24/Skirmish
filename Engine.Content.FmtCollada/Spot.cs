using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Spot
    {
        [XmlElement("color", typeof(BasicColor))]
        public BasicColor Color { get; set; }
        [XmlElement("constant_attenuation", typeof(BasicFloat))]
        public BasicFloat ConstantAttenuation { get; set; }
        [XmlElement("linear_attenuation", typeof(BasicFloat))]
        public BasicFloat LinearAttenuation { get; set; }
        [XmlElement("quadratic_attenuation", typeof(BasicFloat))]
        public BasicFloat QuadraticAttenuation { get; set; }
        [XmlElement("falloff_angle", typeof(BasicFloat))]
        public BasicFloat FalloffAngle { get; set; }
        [XmlElement("falloff_exponent", typeof(BasicFloat))]
        public BasicFloat FalloffExponent { get; set; }
    }
}
