using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Point
    {
        [XmlElement("color", typeof(BasicColor))]
        public BasicColor Color { get; set; }
        [XmlElement("constant_attenuation", typeof(BasicFloat))]
        public BasicFloat ConstantAttenuation { get; set; }
        [XmlElement("linear_attenuation", typeof(BasicFloat))]
        public BasicFloat LinearAttenuation { get; set; }
        [XmlElement("quadratic_attenuation", typeof(BasicFloat))]
        public BasicFloat QuadraticAttenuation { get; set; }

        public Point()
        {
            ConstantAttenuation = new BasicFloat { Value = 1 };
            LinearAttenuation = new BasicFloat { Value = 0 };
            QuadraticAttenuation = new BasicFloat { Value = 0 };
        }
    }
}
