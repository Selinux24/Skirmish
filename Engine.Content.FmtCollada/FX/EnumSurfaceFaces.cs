using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public enum EnumSurfaceFaces
    {
        [XmlEnum("POSITIVE_X")]
        PositiveX,
        [XmlEnum("POSITIVE_Y")]
        PositiveY,
        [XmlEnum("POSITIVE_Z")]
        PositiveZ,
        [XmlEnum("NEGATIVE_X")]
        NegativeX,
        [XmlEnum("NEGATIVE_Y")]
        Negativey,
        [XmlEnum("NEGATIVE_Z")]
        NegativeZ,
    }
}
