using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public enum EnumAxis
    {
        [XmlEnum("X_UP")]
        XUp,
        [XmlEnum("Y_UP")]
        YUp,
        [XmlEnum("Z_UP")]
        ZUp,
    }
}
