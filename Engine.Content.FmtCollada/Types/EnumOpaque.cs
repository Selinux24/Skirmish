using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public enum EnumOpaque
    {
        [XmlEnum("A_ONE")]
        AlphaOne,
        [XmlEnum("RGB_ZERO")]
        RGBZero,
    }
}
