using System.Xml.Serialization;

namespace Common.Collada.Types
{
    public enum SurfaceFaceEnum
    {
        [XmlEnum("POSITIVE_X")]
        PositiveX,
        [XmlEnum("NEGATIVE_X")]
        NegativeX,
        [XmlEnum("POSITIVE_Y")]
        PositiveY,
        [XmlEnum("NEGATIVE_Y")]
        NegativeY,
        [XmlEnum("POSITIVE_Z")]
        PositiveZ,
        [XmlEnum("NEGATIVE_Z")]
        NegativeZ,
    }
}
