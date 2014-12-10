using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public enum EnumSurfaceTypes
    {
        [XmlEnum("UNTYPED")]
        Untyped,
        [XmlEnum("1D")]
        S1D,
        [XmlEnum("2D")]
        S2D,
        [XmlEnum("3D")]
        S3D,
        [XmlEnum("RECT")]
        Rect,
        [XmlEnum("CUBE")]
        Cube,
        [XmlEnum("DEPTH")]
        Depth,
    }
}
